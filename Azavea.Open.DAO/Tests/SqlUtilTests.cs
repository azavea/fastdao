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
using System.Collections.Generic;
using Azavea.Open.Common;
using Azavea.Open.DAO.SQL;
using NUnit.Framework;

namespace Azavea.Open.DAO.Tests
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

        /// <exclude/>
        // Here are examples of the performance when we last ran it:
        //time to convert Simple: 00:00:00.0781250 (attempt 1)
        //time to convert 5: 00:00:00.0781250 (attempt 1)
        //time to convert 5: 00:00:00.1406250 (attempt 1)
        //time to convert 5: 00:00:00.1562500 (attempt 1)
        //time to convert 5: 00:00:00.1406250 (attempt 2)
        //time to convert 5: 00:00:00.0781250 (attempt 1)
        //time to convert 2 NOV 2007: 00:00:01.8437500 (attempt 1)
        //time to convert 11:35 AM Nov 2 2007: 00:00:02.1093750 (attempt 1)
        //time to convert SEQUENCE: 00:00:00.9218750 (attempt 1)
        //time to convert 1: 00:00:00.1875000 (attempt 1)
        [Ignore("Performance tests are too unstable to have as automated unit tests.")]
        [Test]
        public void TestTypePerformance()
        {
            // Note: Due to subspace field harmonics, this test will occasionally fail due to
            // something taking a half second longer than it should.  Only worry about it if it beings
            // failing consistently.
            int loopCount = 1000000;
            AssertPerformance("Simple", typeof(string), loopCount, new TimeSpan(0, 0, 0, 0, 200));
            AssertPerformance(5, typeof(int), loopCount, new TimeSpan(0, 0, 0, 0, 150));
            AssertPerformance(5.0, typeof(int), loopCount, new TimeSpan(0, 0, 0, 0, 150));
            AssertPerformance(5, typeof(double), loopCount, new TimeSpan(0, 0, 0, 0, 150));
            AssertPerformance(5.0, typeof(double), loopCount, new TimeSpan(0, 0, 0, 0, 150));
            AssertPerformance("2 NOV 2007", typeof(DateTime), loopCount, new TimeSpan(0, 0, 0, 2, 0));
            AssertPerformance("11:35 AM Nov 2 2007", typeof(DateTime), loopCount, new TimeSpan(0, 0, 0, 2, 500));
            AssertPerformance("SEQUENCE", typeof(GeneratorType), loopCount, new TimeSpan(0, 0, 0, 1, 200));
            AssertPerformance(1, typeof(GeneratorType), loopCount, new TimeSpan(0, 0, 0, 0, 200));
        }
        private static void AssertPerformance(object input, Type desiredType, int loopCount, TimeSpan max)
        {
            int retryCount = 3;
            DateTime startMine;
            DateTime endMine;
            do
            {
                SqlDaLayer coercer = new SqlDaLayer(null, true);
                startMine = DateTime.Now;
                for (int x = 0; x < loopCount; x++)
                {
                    coercer.CoerceType(desiredType, input);
                }
                endMine = DateTime.Now;
                Console.WriteLine("time to convert " + input + ": " + (endMine - startMine) + " (attempt " + (4 - retryCount) + ")");
                // Since this test randomly fails, retry a couple times before giving up.
                if (endMine - startMine < max)
                {
                    retryCount = 0;
                }
                else
                {
                    retryCount--;
                }
            } while (retryCount > 0);
            Assert.Less(endMine - startMine, max, "took too long!  Input: " +
                input + ", type: " + desiredType);
        }
        /// <summary>
        /// Tests the classmapping generation off the nullable table schema.
        /// </summary>
        /// <param name="connDesc">Connection descriptor for your particular database.</param>
        /// <param name="nullableTableName">Correctly-cased name of the nullable table (some DBs
        ///                                 are case sensitive (cough oracle, postgresql cough)</param>
        public static void TestGetNullableTableMappingFromSchema(AbstractSqlConnectionDescriptor connDesc,
            string nullableTableName)
        {
            ClassMapping map = SqlConnectionUtilities.GenerateMappingFromSchema(connDesc, nullableTableName);
            Assert.AreEqual(map.TypeName, nullableTableName, "Wrong 'type' name on the generated class map.");
            Assert.AreEqual(map.Table, nullableTableName, "Wrong table name on the generated class map.");
            Assert.AreEqual(5, map.AllObjAttrsByDataCol.Count, "Wrong number of mapped fields.");
            Assert.AreEqual("ID", map.AllObjAttrsByDataCol["ID"], "Column was mapped incorrectly.");
            Assert.AreEqual("BOOLCOL", map.AllObjAttrsByDataCol["BoolCol"], "Column was mapped incorrectly.");
            Assert.AreEqual("INTCOL", map.AllObjAttrsByDataCol["IntCol"], "Column was mapped incorrectly.");
            Assert.AreEqual("FLOATCOL", map.AllObjAttrsByDataCol["FloatCol"], "Column was mapped incorrectly.");
            Assert.AreEqual("DATECOL", map.AllObjAttrsByDataCol["DateCol"], "Column was mapped incorrectly.");
        }
    }
}
