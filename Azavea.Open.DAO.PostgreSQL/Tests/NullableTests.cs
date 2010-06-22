// Copyright (c) 2004-2010 Azavea, Inc.
// 
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using Azavea.Open.Common;
using Azavea.Open.DAO.SQL;
using NUnit.Framework;

namespace Azavea.Open.DAO.PostgreSQL.Tests
{
    /// <exclude />
    [TestFixture]
    public class NullableTests
    {
        private FastDAO<NullableClass> _ncdao;

        /// <exclude/>
        [TestFixtureSetUp]
        public void SetUp()
        {
            try
            {
                _ncdao = new FastDAO<NullableClass>(new Config("..\\..\\Tests\\PostgreSqlDao.config", "PostgreSqlDaoConfig"), "DAO");
                _ncdao.Truncate();
            }
            catch (Exception e)
            {
                string message = "ERROR: Exception while setting up PostGIS Nullable tests: " + e;
                Console.WriteLine(message);
                Assert.Fail(message);
            }
        }

        /// <exclude />
        [Test]
        public void TestInsertNonNull()
        {
            NullableClass nc = new NullableClass();
            nc.SomeNullableInt = 10;
            _ncdao.Insert(nc, true);

            nc = _ncdao.GetFirst("NullableId", nc.NullableId);
            Assert.AreEqual(10, nc.SomeNullableInt);
        }

        /// <exclude />
        [Test]
        public void TestInsertNull()
        {
            NullableClass nc = new NullableClass();
            nc.SomeNullableInt = null;
            _ncdao.Insert(nc, true);

            nc = _ncdao.GetFirst("NullableId", nc.NullableId);
            Assert.AreEqual(null, nc.SomeNullableInt);
        }
        /// <exclude />
        [Test]
        public void TestSimpleInsertNull()
        {
            SqlConnectionUtilities.XSafeCommand(new PostgreSqlDescriptor(
                                                    new Config("..\\..\\Tests\\PostgreSqlDao.config", "PostgreSqlDaoConfig"), "DAO", null),
                                                "insert into nullables(some_nullable_int) values (?)",
                                                new object[] {null});
        }
    }

    /// <exclude/>
    public class NullableClass
    {
        /// <exclude/>
        public int NullableId;
        /// <exclude/>
        public int? SomeNullableInt;
    }
}