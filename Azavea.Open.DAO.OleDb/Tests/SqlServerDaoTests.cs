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

using Azavea.Open.Common;
using Azavea.Open.DAO.SQL;
using Azavea.Open.DAO.Tests;
using NUnit.Framework;

namespace Azavea.Open.DAO.OleDb.Tests
{
    /// <exclude/>
    [TestFixture]
    public class SqlServerDaoTests : AbstractFastDAOTests
    {
        /// <exclude/>
        public SqlServerDaoTests()
            : base(new Config("..\\..\\Tests\\SqlServerDao.config", "SqlServerDaoConfig"), "DAO", 
            true, true, true, true, true, false) { }

        /// <exclude/>
        [TestFixtureSetUp]
        public virtual void Init()
        {
            SqlConnectionUtilities.XSafeCommand((AbstractSqlConnectionDescriptor)_connDesc, "TRUNCATE TABLE EnumTable", null);
            SqlConnectionUtilities.XSafeCommand((AbstractSqlConnectionDescriptor)_connDesc, "TRUNCATE TABLE BoolTable", null);
            SqlConnectionUtilities.XSafeCommand((AbstractSqlConnectionDescriptor)_connDesc, "TRUNCATE TABLE NullableTable", null);
            SqlConnectionUtilities.XSafeCommand((AbstractSqlConnectionDescriptor)_connDesc, "TRUNCATE TABLE NameTable", null);
            SqlConnectionUtilities.XSafeCommand((AbstractSqlConnectionDescriptor)_connDesc, "DBCC CHECKIDENT (NameTable, RESEED, 1)", null);
            SqlConnectionUtilities.XSafeCommand((AbstractSqlConnectionDescriptor)_connDesc, "INSERT INTO NameTable VALUES('Michael')", null);
            SqlConnectionUtilities.XSafeCommand((AbstractSqlConnectionDescriptor)_connDesc, "INSERT INTO NameTable VALUES('Rich')", null);
            SqlConnectionUtilities.XSafeCommand((AbstractSqlConnectionDescriptor)_connDesc, "INSERT INTO NameTable VALUES('Keith')", null);
            SqlConnectionUtilities.XSafeCommand((AbstractSqlConnectionDescriptor)_connDesc, "INSERT INTO NameTable VALUES('Rachel')", null);
            SqlConnectionUtilities.XSafeCommand((AbstractSqlConnectionDescriptor)_connDesc, "INSERT INTO NameTable VALUES('Jeff')", null);
            SqlConnectionUtilities.XSafeCommand((AbstractSqlConnectionDescriptor)_connDesc, "INSERT INTO NameTable VALUES('Megan')", null);
        }
    }
}