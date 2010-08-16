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
using Azavea.Open.DAO.Criteria.Joins;
using NUnit.Framework;

namespace Azavea.Open.DAO.Tests
{
    /// <summary>
    /// This is a test class you can extend to test joins on any pair of DAOs.
    /// </summary>
    public abstract class AbstractJoinTests
    {
        private readonly IFastDaoReader<JoinClass1> _dao1;
        private readonly IFastDaoReader<JoinClass2> _dao2;
        private readonly bool _expectNullsFirst;
        private readonly bool _supportsLeftOuter;
        private readonly bool _supportsRightOuter;
        private readonly bool _supportsFullOuter;

        /// <exclude/>
        protected AbstractJoinTests(IFastDaoReader<JoinClass1> dao1, IFastDaoReader<JoinClass2> dao2,
                                    bool expectNullsFirst, bool supportsLeftOuter, bool supportsRightOuter, bool supportsFullOuter)
        {
            _dao1 = dao1;
            _dao2 = dao2;
            _expectNullsFirst = expectNullsFirst;
            _supportsLeftOuter = supportsLeftOuter;
            _supportsRightOuter = supportsRightOuter;
            _supportsFullOuter = supportsFullOuter;
        }

        /// <summary>
        /// This one allows you to specify the values for each side.
        /// </summary>
        public static void AssertJoinResults<L, R>(IFastDaoReader<L> leftDao, IFastDaoReader<R> rightDao,
                                                   DaoJoinCriteria crit,
                                                   string[] expectedLeft, string[] expectedRight, string description)
            where L : JoinClass1, new() where R : JoinClass1, new()
        {
            // First, test the get itself.
            IList<JoinResult<L, R>> results = leftDao.Get(crit, rightDao);
            Assert.AreEqual(expectedLeft.Length, expectedRight.Length,
                            "Test bug: Your expected left and right lists had different lengths.");
            Console.WriteLine("Results:\n" + StringHelper.Join(results, "\n"));
            Assert.AreEqual(expectedLeft.Length, results.Count,
                            description + ": Wrong number of results from the join, expected L: " +
                            StringHelper.Join(expectedLeft) + ", R: " + StringHelper.Join(expectedRight) +
                            " but got " + StringHelper.Join(results));
            for (int count = 0; count < expectedLeft.Length; count++)
            {
                if (expectedLeft[count] == null)
                {
                    Assert.IsNull(results[count].Left, description + ": Left object " +
                                                       count + " was not null: " + results[count]);
                }
                else
                {
                    Assert.IsNotNull(results[count].Left, description + ": Left object was null.");
                    Assert.AreEqual(expectedLeft[count], results[count].Left.JoinField,
                                    description + ": Left object " + count + " had the wrong value.");
                }
                if (expectedRight[count] == null)
                {
                    Assert.IsNull(results[count].Right, description + ": Right object " +
                                                        count + " was not null: " + results[count]);
                }
                else
                {
                    Assert.IsNotNull(results[count].Right, description + ": Right object was null.");
                    Assert.AreEqual(expectedRight[count], results[count].Right.JoinField, description +
                                                                                          ": Right object " + count + " had the wrong value.");
                }
            }
            // Now we know the get works, test get count if not testing start/limit
            if (crit.Start == -1 && crit.Limit == -1)
            {
                Assert.AreEqual(expectedLeft.Length, leftDao.GetCount(crit, rightDao), "GetCount was incorrect");
            }
        }
        
        /// <exclude/>
        [Test]
        public void TestInnerJoinEquals()
        {
            DaoJoinCriteria crit = new DaoJoinCriteria(new EqualJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            AssertJoinResults(_dao1, _dao2, crit,
                              new string[] { "one", "two", "three" },
                              new string[] { "one", "two", "three" },
                              "Inner join, 1 = 2");
            AssertJoinResults(_dao2, _dao1, crit,
                              new string[] { "one", "two", "three" },
                              new string[] { "one", "two", "three" },
                              "Inner join, 2 = 1");
        }

        /// <exclude/>
        [Test]
        public void TestJoinStart()
        {
            DaoJoinCriteria crit = new DaoJoinCriteria(new EqualJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Start = 1;
            AssertJoinResults(_dao1, _dao2, crit,
                              new string[] { "two", "three" },
                              new string[] { "two", "three" },
                              "Join start only");
        }

        /// <exclude/>
        [Test]
        public void TestJoinLimit()
        {
            DaoJoinCriteria crit = new DaoJoinCriteria(new EqualJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Limit = 2;
            AssertJoinResults(_dao1, _dao2, crit,
                              new string[] { "one", "two" },
                              new string[] { "one", "two" },
                              "Join limit only");
        }

        /// <exclude/>
        [Test]
        public void TestJoinStartLimit()
        {
            DaoJoinCriteria crit = new DaoJoinCriteria(new EqualJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Start = 1;
            crit.Limit = 1;
            AssertJoinResults(_dao1, _dao2, crit,
                              new string[] { "two" },
                              new string[] { "two" },
                              "Join start and limit");
        }

        /// <exclude/>
        [Test]
        public void Test12LOuterJoinEquals()
        {
            if (!_supportsLeftOuter)
            {
                Assert.Ignore("This data source does not support left outer joins.");
            }
            DaoJoinCriteria crit = new DaoJoinCriteria(JoinType.LeftOuter,
                                                       new EqualJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            AssertJoinResults(_dao1, _dao2, crit,
                              new string[] {"one", "two", "three", "four", "five"},
                              new string[] {"one", "two", "three", null, null},
                              "Left join, 1 = 2");
        }

        /// <exclude/>
        [Test]
        public void Test21LOuterJoinEquals()
        {
            if (!_supportsLeftOuter)
            {
                Assert.Ignore("This data source does not support left outer joins.");
            }
            DaoJoinCriteria crit = new DaoJoinCriteria(JoinType.LeftOuter,
                                                       new EqualJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            AssertJoinResults(_dao2, _dao1, crit,
                              new string[] { "one", "two", "three", "4", "5" },
                              new string[] { "one", "two", "three", null, null },
                              "Left join, 2 = 1");
        }

        /// <exclude/>
        [Test]
        public void Test12ROuterJoinEquals()
        {
            if (!_supportsRightOuter)
            {
                Assert.Ignore("This data source does not support right outer joins.");
            }
            DaoJoinCriteria crit = new DaoJoinCriteria(JoinType.RightOuter,
                                                       new EqualJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Orders.Add(new JoinSortOrder("ID", false));
            if (_expectNullsFirst)
            {
                AssertJoinResults(_dao1, _dao2, crit,
                                  new string[] { null, null, "one", "two", "three" },
                                  new string[] { "4", "5", "one", "two", "three" },
                                  "Right join, 1 = 2");
            }
            else
            {
                AssertJoinResults(_dao1, _dao2, crit,
                                  new string[] { "one", "two", "three", null, null },
                                  new string[] { "one", "two", "three", "4", "5" },
                                  "Right join, 1 = 2");
            }
        }

        /// <exclude/>
        [Test]
        public void Test21ROuterJoinEquals()
        {
            if (!_supportsRightOuter)
            {
                Assert.Ignore("This data source does not support right outer joins.");
            }
            DaoJoinCriteria crit = new DaoJoinCriteria(JoinType.RightOuter,
                                                       new EqualJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Orders.Add(new JoinSortOrder("ID", false));
            if (_expectNullsFirst)
            {
                AssertJoinResults(_dao2, _dao1, crit,
                                  new string[] { null, null, "one", "two", "three" },
                                  new string[] { "four", "five", "one", "two", "three" },
                                  "Right join, 2 = 1");
            }
            else
            {
                AssertJoinResults(_dao2, _dao1, crit,
                                  new string[] { "one", "two", "three", null, null },
                                  new string[] { "one", "two", "three", "four", "five" },
                                  "Right join, 2 = 1");
            }
        }

        /// <exclude/>
        [Test]
        public void Test12OuterJoinEquals()
        {
            if (!_supportsFullOuter)
            {
                Assert.Ignore("This data source does not support full outer joins.");
            }
            DaoJoinCriteria crit = new DaoJoinCriteria(JoinType.Outer,
                                                       new EqualJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Orders.Add(new JoinSortOrder("ID", false));
            if (_expectNullsFirst)
            {
                AssertJoinResults(_dao1, _dao2, crit,
                                  new string[] { null, null, "one", "two", "three", "four", "five" },
                                  new string[] { "4", "5", "one", "two", "three", null, null },
                                  "Right join, 1 = 2");
            }
            else
            {
                AssertJoinResults(_dao1, _dao2, crit,
                                  new string[] { "one", "two", "three", "four", "five", null, null },
                                  new string[] { "one", "two", "three", null, null, "4", "5" },
                                  "Right join, 1 = 2");
            }
        }

        /// <exclude/>
        [Test]
        public void Test21OuterJoinEquals()
        {
            if (!_supportsFullOuter)
            {
                Assert.Ignore("This data source does not support full outer joins.");
            }
            DaoJoinCriteria crit = new DaoJoinCriteria(JoinType.Outer,
                                                       new EqualJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Orders.Add(new JoinSortOrder("ID", false));
            if (_expectNullsFirst)
            {
                AssertJoinResults(_dao2, _dao1, crit,
                                  new string[] { null, null, "one", "two", "three", "4", "5" },
                                  new string[] { "four", "five", "one", "two", "three", null, null },
                                  "Right join, 2 = 1");
            }
            else
            {
                AssertJoinResults(_dao2, _dao1, crit,
                                  new string[] { "one", "two", "three", "4", "5", null, null },
                                  new string[] { "one", "two", "three", null, null, "four", "five" },
                                  "Right join, 2 = 1");
            }
        }

        /// <exclude/>
        [Test]
        public void TestInnerJoinGreater()
        {
            DaoJoinCriteria crit = new DaoJoinCriteria(new GreaterJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Orders.Add(new JoinSortOrder("ID", false));
            AssertJoinResults(_dao1, _dao2, crit,
                              new string[] { "one", "one", "two", "two", "two", "two", "three", "three", "three", "four", "four", "five", "five" },
                              new string[] { "4", "5", "one", "three", "4", "5", "one", "4", "5", "4", "5", "4", "5" },
                              "Inner join, 1 > 2");
            AssertJoinResults(_dao2, _dao1, crit,
                              new string[] { "one", "one", "two", "two", "two", "two", "three", "three", "three" },
                              new string[] { "four", "five", "one", "three", "four", "five", "one", "four", "five" },
                              "Inner join, 2 > 1");
        }

        /// <exclude/>
        [Test]
        public void Test12LOuterJoinGreater()
        {
            if (!_supportsLeftOuter)
            {
                Assert.Ignore("This data source does not support left outer joins.");
            }
            DaoJoinCriteria crit = new DaoJoinCriteria(JoinType.LeftOuter,
                                                       new GreaterJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Orders.Add(new JoinSortOrder("ID", false));
            AssertJoinResults(_dao1, _dao2, crit,
                              new string[] { "one", "one", "two", "two", "two", "two", "three", "three", "three", "four", "four", "five", "five" },
                              new string[] { "4", "5", "one", "three", "4", "5", "one", "4", "5", "4", "5", "4", "5" },
                              "Left join, 1 > 2");
        }

        /// <exclude/>
        [Test]
        public void Test21LOuterJoinGreater()
        {
            if (!_supportsLeftOuter)
            {
                Assert.Ignore("This data source does not support left outer joins.");
            }
            DaoJoinCriteria crit = new DaoJoinCriteria(JoinType.LeftOuter,
                                                       new GreaterJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Orders.Add(new JoinSortOrder("ID", false));
            AssertJoinResults(_dao2, _dao1, crit,
                              new string[] { "one", "one", "two", "two", "two", "two", "three", "three", "three", "4", "5" },
                              new string[] { "four", "five", "one", "three", "four", "five", "one", "four", "five", null, null },
                              "Left join, 2 > 1");
        }

        /// <exclude/>
        [Test]
        public void Test12ROuterJoinGreater()
        {
            if (!_supportsRightOuter)
            {
                Assert.Ignore("This data source does not support right outer joins.");
            }
            DaoJoinCriteria crit = new DaoJoinCriteria(JoinType.RightOuter,
                                                       new GreaterJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Orders.Add(new JoinSortOrder("ID", false));
            if (_expectNullsFirst)
            {
                AssertJoinResults(_dao1, _dao2, crit,
                                  new string[] { null,  "one", "one", "two", "two", "two", "two", "three", "three", "three", "four", "four", "five", "five" },
                                  new string[] { "two", "4",   "5",   "one", "three", "4",   "5", "one",   "4",     "5",     "4",    "5",    "4",    "5" },
                                  "Right join, 1 > 2");
            }
            else
            {
                AssertJoinResults(_dao1, _dao2, crit,
                                  new string[] { "one", "one", "two", "two", "two", "two", "three", "three", "three", "four", "four", "five", "five", null },
                                  new string[] { "4", "5", "one", "three", "4", "5", "one", "4", "5", "4", "5", "4", "5", "two" },
                                  "Right join, 1 > 2");
            }
        }

        /// <exclude/>
        [Test]
        public void Test21ROuterJoinGreater()
        {
            if (!_supportsRightOuter)
            {
                Assert.Ignore("This data source does not support right outer joins.");
            }
            DaoJoinCriteria crit = new DaoJoinCriteria(JoinType.RightOuter,
                                                       new GreaterJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Orders.Add(new JoinSortOrder("ID", false));
            if (_expectNullsFirst)
            {
                AssertJoinResults(_dao2, _dao1, crit,
                                  new string[] { null, "one", "one", "two", "two", "two", "two", "three", "three", "three" },
                                  new string[] { "two", "four", "five", "one", "three", "four", "five", "one", "four", "five" },
                                  "Right join, 2 > 1");
            }
            else
            {
                AssertJoinResults(_dao2, _dao1, crit,
                                  new string[] { "one", "one", "two", "two", "two", "two", "three", "three", "three", null },
                                  new string[] { "four", "five", "one", "three", "four", "five", "one", "four", "five", "two" },
                                  "Right join, 2 > 1");
            }
        }

        /// <exclude/>
        [Test]
        public void Test12OuterJoinGreater()
        {
            if (!_supportsFullOuter)
            {
                Assert.Ignore("This data source does not support full outer joins.");
            }
            DaoJoinCriteria crit = new DaoJoinCriteria(JoinType.Outer,
                                                       new GreaterJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Orders.Add(new JoinSortOrder("ID", false));
            if (_expectNullsFirst)
            {
                AssertJoinResults(_dao1, _dao2, crit,
                                  new string[] { null, "one", "one", "two", "two", "two", "two", "three", "three", "three", "four", "four", "five", "five" },
                                  new string[] { "two", "4", "5", "one", "three", "4", "5", "one", "4", "5", "4", "5", "4", "5" },
                                  "Right join, 1 > 2");
            }
            else
            {
                AssertJoinResults(_dao1, _dao2, crit,
                                  new string[] { "one", "one", "two", "two", "two", "two", "three", "three", "three", "four", "four", "five", "five", null },
                                  new string[] { "4", "5", "one", "three", "4", "5", "one", "4", "5", "4", "5", "4", "5", "two" },
                                  "Right join, 1 > 2");
            }
        }

        /// <exclude/>
        [Test]
        public void Test21OuterJoinGreater()
        {
            if (!_supportsFullOuter)
            {
                Assert.Ignore("This data source does not support full outer joins.");
            }
            DaoJoinCriteria crit = new DaoJoinCriteria(JoinType.Outer,
                                                       new GreaterJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Orders.Add(new JoinSortOrder("ID", false));
            if (_expectNullsFirst)
            {
                AssertJoinResults(_dao2, _dao1, crit,
                                  new string[] { null, "one", "one", "two", "two", "two", "two", "three", "three", "three", "4", "5" },
                                  new string[] { "two", "four", "five", "one", "three", "four", "five", "one", "four", "five", null, null },
                                  "Right join, 2 > 1");
            }
            else
            {
                AssertJoinResults(_dao2, _dao1, crit,
                                  new string[] { "one", "one", "two", "two", "two", "two", "three", "three", "three", "4", "5", null },
                                  new string[] { "four", "five", "one", "three", "four", "five", "one", "four", "five", null, null, "two" },
                                  "Right join, 2 > 1");
            }
        }

        /// <exclude/>
        [Test]
        public void TestInnerJoinLesser()
        {
            DaoJoinCriteria crit = new DaoJoinCriteria(new LesserJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Orders.Add(new JoinSortOrder("ID", false));
            AssertJoinResults(_dao1, _dao2, crit,
                              new string[] { "one", "one", "three", "four", "four", "four", "five", "five", "five" },
                              new string[] { "two", "three", "two",   "one",  "two",  "three", "one",  "two",  "three" },
                              "Inner join, 1 < 2");
            AssertJoinResults(_dao2, _dao1, crit,
                              new string[] { "one", "one", "three", "4", "4", "4", "4", "4", "5", "5", "5", "5", "5" },
                              new string[] { "two", "three", "two",   "one", "two", "three", "four", "five", "one", "two", "three", "four", "five" },
                              "Inner join, 2 < 1");
        }

        /// <exclude/>
        [Test]
        public void Test12LOuterJoinLesser()
        {
            if (!_supportsLeftOuter)
            {
                Assert.Ignore("This data source does not support left outer joins.");
            }
            DaoJoinCriteria crit = new DaoJoinCriteria(JoinType.LeftOuter,
                                                       new LesserJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Orders.Add(new JoinSortOrder("ID", false));
            AssertJoinResults(_dao1, _dao2, crit,
                              new string[] { "one", "one", "two", "three", "four", "four", "four", "five", "five", "five" },
                              new string[] { "two", "three", null, "two", "one", "two", "three", "one", "two", "three" },
                              "Left join, 1 < 2");
        }

        /// <exclude/>
        [Test]
        public void Test21LOuterJoinLesser()
        {
            if (!_supportsLeftOuter)
            {
                Assert.Ignore("This data source does not support left outer joins.");
            }
            DaoJoinCriteria crit = new DaoJoinCriteria(JoinType.LeftOuter,
                                                       new LesserJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Orders.Add(new JoinSortOrder("ID", false));
            AssertJoinResults(_dao2, _dao1, crit,
                              new string[] { "one", "one", "two", "three", "4", "4", "4", "4", "4", "5", "5", "5", "5", "5" },
                              new string[] { "two", "three", null, "two", "one", "two", "three", "four", "five", "one", "two", "three", "four", "five" },
                              "Left join, 2 < 1");
        }

        /// <exclude/>
        [Test]
        public void Test12ROuterJoinLesser()
        {
            if (!_supportsRightOuter)
            {
                Assert.Ignore("This data source does not support right outer joins.");
            }
            DaoJoinCriteria crit = new DaoJoinCriteria(JoinType.RightOuter,
                                                       new LesserJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Orders.Add(new JoinSortOrder("ID", false));
            if (_expectNullsFirst)
            {
                AssertJoinResults(_dao1, _dao2, crit,
                                  new string[] { null, null, "one", "one",  "three", "four", "four", "four", "five", "five", "five" },
                                  new string[] { "4", "5", "two", "three", "two", "one", "two", "three", "one", "two", "three" },
                                  "Right join, 1 < 2");
            }
            else
            {
                AssertJoinResults(_dao1, _dao2, crit,
                                  new string[] { "one", "one", "three", "four", "four", "four", "five", "five", "five", null, null },
                                  new string[] { "two", "three", "two", "one", "two", "three", "one", "two", "three", "4", "5" },
                                  "Right join, 1 < 2");
            }
        }

        /// <exclude/>
        [Test]
        public void Test21ROuterJoinLesser()
        {
            if (!_supportsRightOuter)
            {
                Assert.Ignore("This data source does not support right outer joins.");
            }
            DaoJoinCriteria crit = new DaoJoinCriteria(JoinType.RightOuter,
                                                       new LesserJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Orders.Add(new JoinSortOrder("ID", false));
            AssertJoinResults(_dao2, _dao1, crit,
                              new string[] { "one", "one", "three", "4", "4", "4", "4", "4", "5", "5", "5", "5", "5" },
                              new string[] { "two", "three", "two", "one", "two", "three", "four", "five", "one", "two", "three", "four", "five" },
                              "Right join, 2 < 1");
        }

        /// <exclude/>
        [Test]
        public void Test12OuterJoinLesser()
        {
            if (!_supportsFullOuter)
            {
                Assert.Ignore("This data source does not support full outer joins.");
            }
            DaoJoinCriteria crit = new DaoJoinCriteria(JoinType.Outer,
                                                       new LesserJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Orders.Add(new JoinSortOrder("ID", false));
            if (_expectNullsFirst)
            {
                AssertJoinResults(_dao1, _dao2, crit,
                                  new string[] { null, null, "one", "one", "two", "three", "four", "four", "four", "five", "five", "five" },
                                  new string[] { "4", "5", "two", "three", null, "two", "one", "two", "three", "one", "two", "three" },
                                  "Right join, 1 < 2");
            }
            else
            {
                AssertJoinResults(_dao1, _dao2, crit,
                                  new string[] { "one", "one", "two", "three", "four", "four", "four", "five", "five", "five", null, null },
                                  new string[] { "two", "three", null, "two", "one", "two", "three", "one", "two", "three", "4", "5" },
                                  "Right join, 1 < 2");
            }
        }

        /// <exclude/>
        [Test]
        public void Test21OuterJoinLesser()
        {
            if (!_supportsFullOuter)
            {
                Assert.Ignore("This data source does not support full outer joins.");
            }
            DaoJoinCriteria crit = new DaoJoinCriteria(JoinType.Outer,
                                                       new LesserJoinExpression("JoinField", "JoinField"));
            crit.Orders.Add(new JoinSortOrder("ID", true));
            crit.Orders.Add(new JoinSortOrder("ID", false));
            AssertJoinResults(_dao2, _dao1, crit,
                              new string[] { "one", "one", "two", "three", "4", "4", "4", "4", "4", "5", "5", "5", "5", "5" },
                              new string[] { "two", "three", null, "two", "one", "two", "three", "four", "five", "one", "two", "three", "four", "five" },
                              "Right join, 2 < 1");
        }
    }

    /// <exclude/>
    public class JoinClass1
    {
        /// <exclude/>
        public int ID;
        /// <exclude/>
        public string JoinField;
        /// <exclude/>
        public string Name;
        /// <exclude/>
        public JoinClass1()
        {
        }
        /// <exclude/>
        public JoinClass1(string joinField)
        {
            JoinField = joinField;
        }
        /// <exclude/>
        public override string ToString()
        {
            return ID + "-" + JoinField;
        }
    }

    /// <exclude/>
    public class JoinClass2 : JoinClass1
    {
        /// <exclude/>
        public JoinClass2()
        {
        }
        /// <exclude/>
        public JoinClass2(string joinField)
        {
            JoinField = joinField;
        }
    }
}