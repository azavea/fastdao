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

using System.Collections;
using System.IO;
using System.Threading;
using Azavea.Open.Common;
using System.Data.SQLite;
using Azavea.Open.DAO.SQL;
using NUnit.Framework;

namespace Azavea.Open.DAO.SQLite.Tests
{
    /// <exclude/>
    [TestFixture]
    public class SQLiteBasicTests
    {
        private readonly SQLiteDescriptor _connDesc =
            new SQLiteDescriptor(new Config("..\\..\\Tests\\BasicTest.config", "SQLiteDaoConfig"), "Test", null);
        /// <exclude/>
        [TestFixtureSetUp]
        public void SetUpTestFixture()
        {
            // Blow it away if there was one.
            File.Delete(_connDesc.DatabasePath);
            // Create a fresh one.
            SQLiteConnection.CreateFile(_connDesc.DatabasePath);
        }
        /// <exclude/>

        [Test]
        public void TestBasicOperations()
        {
            SqlConnectionUtilities.XSafeCommand(_connDesc,
                "CREATE TABLE TestTable (field1 int, field2 varchar(100))", null);
            SqlConnectionUtilities.XSafeCommand(_connDesc,
                "INSERT INTO TestTable VALUES (1, 'hi')", null);
            SqlConnectionUtilities.XSafeCommand(_connDesc,
                "INSERT INTO TestTable VALUES (99, 'longer string')", null);
            Assert.AreEqual(2, SqlConnectionUtilities.XSafeIntQuery(_connDesc,
                "SELECT COUNT(*) FROM TestTable", null), "Should have inserted two records.");

            ArrayList sqlParams = new ArrayList();
            sqlParams.Add("hi");
            Assert.AreEqual(1, SqlConnectionUtilities.XSafeCommand(_connDesc,
                "DELETE FROM TestTable WHERE field2 = ?", sqlParams),
                "Should have deleted one record.");
            Assert.AreEqual(0, SqlConnectionUtilities.XSafeCommand(_connDesc,
                "DELETE FROM TestTable WHERE field2 = ?", sqlParams),
                "Record is already gone, should delete none this time.");
            // Give it a moment to finish the updates.
            Thread.Sleep(2000);
            SqlConnectionUtilities.XSafeCommand(_connDesc,
                "DROP TABLE TestTable", null);
        }
    }
}
