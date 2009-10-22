using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Avencia.Open.DAO.Criteria;
using NUnit.Framework;

namespace Avencia.Open.DAO.Tests
{
    /// <exclude/>
    [TestFixture]
    public class CriteriaTests
    {
        /// <exclude/>
        [Test]
        public void TestConstructors()
        {
            // Check that we still conform to the original behavior.
            DaoCriteria sc = new DaoCriteria();
            Assert.AreEqual(-1, sc.Start, "Wrong start value.");
            Assert.AreEqual(-1, sc.Limit, "Wrong limit value.");
        }
        /// <exclude/>
        [Test]
        public void TestOrders()
        {
            DaoCriteria sc = new DaoCriteria();
            Assert.AreEqual(0, new List<DaoCriteria.SortOrder>(sc.Orders).Count, "Didn't start with none.");
            sc.AddAscSort("X");
            Assert.AreEqual(1, new List<DaoCriteria.SortOrder>(sc.Orders).Count, "Didn't add the asc one.");
            sc.AddDescSort("A");
            Assert.AreEqual(2, new List<DaoCriteria.SortOrder>(sc.Orders).Count, "Didn't add the desc one.");
            sc.AddComputedSort("Z");
            Assert.AreEqual(3, new List<DaoCriteria.SortOrder>(sc.Orders).Count, "Didn't add the computed one.");
            Assert.AreEqual("X", new List<DaoCriteria.SortOrder>(sc.Orders)[0].Property, "Wrong order first");
            Assert.AreEqual("A", new List<DaoCriteria.SortOrder>(sc.Orders)[1].Property, "Wrong order second");
            Assert.AreEqual("Z", new List<DaoCriteria.SortOrder>(sc.Orders)[2].Property, "Wrong order third");
            Assert.AreEqual(DaoCriteria.SortType.Asc, new List<DaoCriteria.SortOrder>(sc.Orders)[0].Direction, "Wrong sort dir first");
            Assert.AreEqual(DaoCriteria.SortType.Desc, new List<DaoCriteria.SortOrder>(sc.Orders)[1].Direction, "Wrong sort dir second");
            Assert.AreEqual(DaoCriteria.SortType.Computed, new List<DaoCriteria.SortOrder>(sc.Orders)[2].Direction, "Wrong sort dir third");
            sc.ClearSortOrder();
            Assert.AreEqual(0, new List<DaoCriteria.SortOrder>(sc.Orders).Count, "Didn't clear.");
            sc.AddAscSort("X");
            Assert.AreEqual(1, new List<DaoCriteria.SortOrder>(sc.Orders).Count, "Didn't add one after clearing.");
        }
        /// <exclude/>
        [Test]
        public void TestSubExpressions()
        {
            DaoCriteria sc = new DaoCriteria();
            Assert.AreEqual(0, new List<IExpression>(sc.Expressions).Count, "Didn't start with none.");
            sc.AddExpression(new CriteriaExpression(new DaoCriteria(), true));
            Assert.AreEqual(1, new List<IExpression>(sc.Expressions).Count, "Added a blank sub-expr.");
            sc.Clear();
            DaoCriteria sub1 = new DaoCriteria();
            sub1.AddExpression(new EqualsExpression("x", 5, true));
            sub1.AddExpression(new BetweenExpression("y", 1, 4, true));
            Assert.AreEqual(2, new List<IExpression>(sub1.Expressions).Count, "Sub-expr didn't have 2 exprs.");
            sc.AddExpression(new CriteriaExpression(sub1, true));
            Assert.AreEqual(1, new List<IExpression>(sc.Expressions).Count, "Should be 1 sub-expr.");
        }

        /// <exclude/>
        [Test]
        public void TestSerialize()
        {
            DaoCriteria sc = new DaoCriteria();
            sc.AddAscSort("X");
            sc.AddExpression(new EqualsExpression("X", "1", true));

            try
            {
                MemoryStream ms = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, sc);
                ms.Close();
                ms.Dispose();
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
