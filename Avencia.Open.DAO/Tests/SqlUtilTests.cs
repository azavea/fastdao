// Copyright (c) 2004-2009 Avencia, Inc.
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

using System.Collections.Generic;
using Avencia.Open.DAO.SQL;
using NUnit.Framework;

namespace Avencia.Open.DAO.Tests
{
    /// <exclude/>
    [TestFixture]
    public class SqlUtilTests
    {
        /// <exclude/>
        [Test]
        public void TestMakeDeleteStatement()
        {
            string del = SqlUtilities.MakeDeleteStatement("Table", null, null);
            Assert.AreEqual("DELETE FROM Table", del,
                "Didn't assemble delete statement with no params.");
            Dictionary<string, object> wheres = new Dictionary<string, object>();
            wheres["blah"] = "five";
            List<object> parms = new List<object>();
            del = SqlUtilities.MakeDeleteStatement("Table", wheres, parms);
            Assert.AreEqual("DELETE FROM Table WHERE blah = ?", del,
                "Didn't assemble delete statement with params.");
        }

        /// <exclude/>
        [Test]
        public void TestMakeInsertStatement()
        {
            Dictionary<string, object> wheres = new Dictionary<string, object>();
            wheres["blah"] = "five";
            List<object> parms = new List<object>();
            string insert = SqlUtilities.MakeInsertStatement("Table", wheres, parms);
            Assert.AreEqual("INSERT INTO Table(blah) VALUES (?)", insert,
                "Didn't assemble insert statement correctly.");
        }

        /// <exclude/>
        [Test]
        public void TestMakeUpdateStatement()
        {
            Dictionary<string, object> cols = new Dictionary<string, object>();
            List<object> parms = new List<object>();
            cols["test"] = 5;
            string update = SqlUtilities.MakeUpdateStatement("Table", null, cols, parms);
            Assert.AreEqual("UPDATE Table SET test = ?", update, "Didn't work with update to 5.");
            Assert.AreEqual(1, parms.Count, "Wrong number of sql parameters generated for just update.");
            Assert.AreEqual(5, parms[0], "Wrong sql parameter value generated for just update.");

            parms = new List<object>();
            Dictionary<string, object> wheres = new Dictionary<string, object>();
            wheres["blah"] = "four";
            update = SqlUtilities.MakeUpdateStatement("Table", wheres, cols, parms);
            Assert.AreEqual("UPDATE Table SET test = ? WHERE blah = ?", update,
                "Didn't work with update with where.");
            Assert.AreEqual(2, parms.Count, "Wrong number of sql parameters generated for update with where.");
            Assert.AreEqual(5, parms[0], "Wrong first sql parameter value generated for update with where.");
            Assert.AreEqual("four", parms[1], "Wrong second sql parameter value generated for update with where.");
        }

        /// <exclude/>
        [Test]
        public void TestMakeWhereClause()
        {
            string where = SqlUtilities.MakeWhereClause(null, null);
            Assert.AreEqual("", where, "Should produce an empty string when given no values.");

            Dictionary<string, object> wheres = new Dictionary<string, object>();
            wheres["blah"] = "five";
            List<object> parms = new List<object>();
            where = SqlUtilities.MakeWhereClause(wheres, parms);
            Assert.AreEqual(" WHERE blah = ?", where,
                "Didn't assemble delete statement with params.");
            Assert.AreEqual(1, parms.Count, "Wrong number of sql parameters generated.");
            Assert.AreEqual("five", parms[0], "Wrong sql parameter value generated.");

            wheres["blah"] = null;
            parms = new List<object>();
            where = SqlUtilities.MakeWhereClause(wheres, parms);
            Assert.AreEqual(" WHERE blah IS NULL", where,
                "Didn't assemble delete statement with params.");
            Assert.AreEqual(0, parms.Count, "Wrong number of sql parameters generated for null.");
        }
    }
}
