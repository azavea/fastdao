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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Azavea.Open.DAO.Criteria;
using NUnit.Framework;

namespace Azavea.Open.DAO.Tests
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
            Assert.AreEqual(0, sc.Orders.Count, "Didn't start with none.");
            sc.Orders.Add(new SortOrder("X", SortType.Asc));
            Assert.AreEqual(1, sc.Orders.Count, "Didn't add the asc one.");
            sc.Orders.Add(new SortOrder("A", SortType.Desc));
            Assert.AreEqual(2, sc.Orders.Count, "Didn't add the desc one.");
            sc.Orders.Add(new SortOrder("Z", SortType.Computed));
            Assert.AreEqual(3, sc.Orders.Count, "Didn't add the computed one.");
            Assert.AreEqual("X", sc.Orders[0].Property, "Wrong order first");
            Assert.AreEqual("A", sc.Orders[1].Property, "Wrong order second");
            Assert.AreEqual("Z", sc.Orders[2].Property, "Wrong order third");
            Assert.AreEqual(SortType.Asc, sc.Orders[0].Direction, "Wrong sort dir first");
            Assert.AreEqual(SortType.Desc, sc.Orders[1].Direction, "Wrong sort dir second");
            Assert.AreEqual(SortType.Computed, sc.Orders[2].Direction, "Wrong sort dir third");
            sc.Orders.Clear();
            Assert.AreEqual(0, sc.Orders.Count, "Didn't clear.");
            sc.Orders.Add(new SortOrder("X", SortType.Asc));
            Assert.AreEqual(1, sc.Orders.Count, "Didn't add one after clearing.");
        }
        /// <exclude/>
        [Test]
        public void TestSubExpressions()
        {
            DaoCriteria sc = new DaoCriteria();
            Assert.AreEqual(0, new List<IExpression>(sc.Expressions).Count, "Didn't start with none.");
            sc.Expressions.Add(new CriteriaExpression(new DaoCriteria(), true));
            Assert.AreEqual(1, new List<IExpression>(sc.Expressions).Count, "Added a blank sub-expr.");
            sc.Clear();
            DaoCriteria sub1 = new DaoCriteria();
            sub1.Expressions.Add(new EqualExpression("x", 5, true));
            sub1.Expressions.Add(new BetweenExpression("y", 1, 4, true));
            Assert.AreEqual(2, new List<IExpression>(sub1.Expressions).Count, "Sub-expr didn't have 2 exprs.");
            sc.Expressions.Add(new CriteriaExpression(sub1, true));
            Assert.AreEqual(1, new List<IExpression>(sc.Expressions).Count, "Should be 1 sub-expr.");
        }

        /// <exclude/>
        [Test]
        public void TestSerialize()
        {
            DaoCriteria sc = new DaoCriteria();
            sc.Orders.Add(new SortOrder("X", SortType.Asc));
            sc.Expressions.Add(new EqualExpression("X", "1", true));

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
