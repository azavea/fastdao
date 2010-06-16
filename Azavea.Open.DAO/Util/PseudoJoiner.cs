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
using Azavea.Open.DAO.Criteria;
using Azavea.Open.DAO.Criteria.Joins;
using log4net;

namespace Azavea.Open.DAO.Util
{
    /// <summary>
    /// This class figures out how to perform a "join" across two DAOs when a native join
    /// is not possible (either there is no support at the data source level, or the DAOs
    /// are accessing different data sources, etc).
    /// 
    /// Inner and Left join performance is adequate, consisting of 1 query to the left DAO
    /// and n queries to the right, where n is the number of rows returned by the left query.
    /// 
    /// Right join performance is marginally slower than left just because we invert all
    /// the parameters, do a left join, and then invert the results (yes it produces correct
    /// output, but the inverting will slow it down slightly).
    /// 
    /// Outer join performance can be bad, consisting of 1 query to the left DAO, 1 query
    /// to the right DAO, and then a massive in-memory comparison of all the results
    /// from the two queries to determine which ones match up.  The number of comparisons
    /// is O(n*m) where n is the number of rows from the left DAO and m is the number from
    /// the right DAO.
    /// </summary>
    public static class PseudoJoiner
    {
        /// <summary>
        /// log4net logger for logging any appropriate messages.
        /// </summary>
        private static readonly ILog _log = LogManager.GetLogger(
            new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().DeclaringType.Namespace);

        /// <summary>
        /// This does a "fake" join, where we query the two DAOs separately and do the
        /// figuring out of the join ourselves in code.
        /// </summary>
        /// <typeparam name="L">The type of object returned by the left DAO.</typeparam>
        /// <typeparam name="R">The type of object returned by the right DAO.</typeparam>
        /// <param name="crit">An object describing how to join the two DAOs.</param>
        /// <param name="leftDao">The "left" DAO we are joining.</param>
        /// <param name="rightDao">The "right" DAO we are joining.</param>
        public static List<JoinResult<L, R>> Join<L, R>(DaoJoinCriteria crit, IFastDaoReader<L> leftDao, IFastDaoReader<R> rightDao)
            where L : class, new() where R : class, new()
        {
            switch (crit.TypeOfJoin)
            {
                case JoinType.Inner:
                case JoinType.LeftOuter:
                    return InnerOrLeftJoin(crit, leftDao, rightDao);
                case JoinType.RightOuter:
                    // We're not implementing right joins, instead we flip the right to a
                    // left and call the method that's already written.
                    DaoJoinCriteria flippedCrit = crit.Flip();
                    List<JoinResult<R, L>> flippedResults =
                        InnerOrLeftJoin(flippedCrit, rightDao, leftDao);
                    List<JoinResult<L,R>> retVal = new List<JoinResult<L, R>>();
                    foreach (JoinResult<R,L> flippedResult in flippedResults)
                    {
                        retVal.Add(flippedResult.Flip());
                    }
                    return retVal;
                case JoinType.Outer:
                    return OuterJoin(crit, leftDao, rightDao);
                default:
                    throw new ArgumentOutOfRangeException("crit", crit.TypeOfJoin,
                                                          "Join type " + crit.TypeOfJoin + " is not supported implemented yet.");
            }
        }

        /// <summary>
        /// Inner and left joins are pretty similar, except that we return left,null if it was
        /// left and we leave those records out if it was inner.
        /// </summary>
        /// <typeparam name="L">The type of object returned by the left DAO.</typeparam>
        /// <typeparam name="R">The type of object returned by the right DAO.</typeparam>
        /// <param name="crit">An object describing how to join the two DAOs.</param>
        /// <param name="leftDao">The "left" DAO we are joining.</param>
        /// <param name="rightDao">The "right" DAO we are joining.</param>
        private static List<JoinResult<L, R>> InnerOrLeftJoin<L, R>(
            DaoJoinCriteria crit, IFastDaoReader<L> leftDao, IFastDaoReader<R> rightDao)
            where L : class, new() where R : class, new()
        {
            List<JoinResult<L, R>> retVal = new List<JoinResult<L, R>>();
            IList<L> leftObjs = leftDao.Get(crit.LeftCriteria);
            foreach (L leftObj in leftObjs)
            {
                DaoCriteria rightCrit = DbCaches.Criteria.Get();
                rightCrit.CopyFrom(crit.RightCriteria);
                foreach (IJoinExpression expr in crit.Expressions)
                {
                    if (expr is EqualJoinExpression)
                    {
                        EqualJoinExpression eje = (EqualJoinExpression)expr;
                        rightCrit.Expressions.Add(new EqualExpression(eje.RightProperty,
                                                                      leftDao.GetValueFromObject(leftObj, eje.LeftProperty), eje.TrueOrNot()));
                    }
                    else if (expr is GreaterJoinExpression)
                    {
                        GreaterJoinExpression gje = (GreaterJoinExpression)expr;
                        // This means LEFT GREATER THAN RIGHT but we're putting the expression
                        // on the right which means we need a LesserExpression.
                        rightCrit.Expressions.Add(new LesserExpression(gje.RightProperty,
                                                                       leftDao.GetValueFromObject(leftObj, gje.LeftProperty), gje.TrueOrNot()));
                    }
                    else if (expr is LesserJoinExpression)
                    {
                        LesserJoinExpression lje = (LesserJoinExpression)expr;
                        // This means LEFT LESS THAN RIGHT but we're putting the expression
                        // on the right which means we need a GreaterExpression.
                        rightCrit.Expressions.Add(new GreaterExpression(lje.RightProperty,
                                                                        leftDao.GetValueFromObject(leftObj, lje.LeftProperty), lje.TrueOrNot()));
                    }
                    else
                    {
                        throw new NotSupportedException("Expression " + expr + " is an unsupported type.");
                    }
                }
                bool foundRight = false;
                foreach (R rightObj in rightDao.Get(rightCrit))
                {
                    retVal.Add(new JoinResult<L, R>(leftObj, rightObj));
                    foundRight = true;
                }
                if ((!foundRight) && (crit.TypeOfJoin == JoinType.LeftOuter))
                {
                    // If there were no matches, but this is an outer join, add one
                    // with a null right object.
                    retVal.Add(new JoinResult<L, R>(leftObj, default(R)));
                }
            }
            // Now sort them per the orders.
            if (crit.Orders.Count > 0)
            {
                retVal.Sort(new PseudoJoinSorter<L, R>(crit.Orders, leftDao, rightDao));
            }
            if (crit.Start == -1 && crit.Limit == -1)
            {
                return retVal;
            }
            int start = 0, count = retVal.Count;
            if(crit.Limit != -1)
            {
                count = crit.Limit;
            }
            if(crit.Start != -1)
            {
                start = crit.Start;
                if (count == retVal.Count)
                {
                    count -= start;
                }
            }
            return retVal.GetRange(start, count);
        }

        /// <summary>
        /// Perform an outer join, meaning query both DAOs independently and figure out
        /// which records match up, including all records that don't match as well.
        /// </summary>
        /// <typeparam name="L">The type of object returned by the left DAO.</typeparam>
        /// <typeparam name="R">The type of object returned by the right DAO.</typeparam>
        /// <param name="crit">An object describing how to join the two DAOs.</param>
        /// <param name="leftDao">The "left" DAO we are joining.</param>
        /// <param name="rightDao">The "right" DAO we are joining.</param>
        private static List<JoinResult<L, R>> OuterJoin<L,R>(
            DaoJoinCriteria crit, IFastDaoReader<L> leftDao, IFastDaoReader<R> rightDao)
            where L : class, new() where R : class, new()
        {
            _log.Warn("Performing an outer join using the PseudoJoiner.  The current\n" +
                      "implementation has almost comically bad performance, due to running\n" +
                      "L*R*C comparisons where L is the number of records returned by the\n" +
                      "left DAO, R is the number returned by the right DAO, and C is the\n" +
                      "number of join expressions.  In other words, if 1000 records from\n" +
                      "each DAO meet the criteria, and since there are " + crit.Expressions.Count + " join\n" +
                      "expression(s), this will perform " + crit.Expressions.Count + "000000 comparisons to\n" +
                      "determine the correct results.  This will work but will be extremely\n" +
                      "slow for any non-trivial amounts of data.  Criteria:\n" + crit);
            List<JoinResult<L, R>> retVal = new List<JoinResult<L, R>>();
            IList<L> leftObjs = leftDao.Get(crit.LeftCriteria);
            _log.Debug("Found " + leftObjs.Count + " objects from left DAO.");
            IList<R> rightObjs = rightDao.Get(crit.RightCriteria);
            _log.Debug("Found " + rightObjs.Count + " objects from right DAO.");
            // Throw a long cast in here to make sure it does long math.
            long numComparisons = leftObjs.Count*(long)rightObjs.Count*crit.Expressions.Count;
            _log.Info("Beginning " + numComparisons + " comparisons.");

            Dictionary<R,R> rightsThatMatched = new Dictionary<R, R>();
            foreach (L leftObj in leftObjs)
            {
                bool anyGoodResults = false;
                foreach (R rightObj in rightObjs)
                {
                    bool goodResult = true;
                    foreach (IJoinExpression expr in crit.Expressions)
                    {
                        bool matchesThisExpression = true;
                        if (expr is AbstractOnePropertyEachJoinExpression)
                        {
                            AbstractOnePropertyEachJoinExpression opeje = (AbstractOnePropertyEachJoinExpression) expr;
                            object leftVal = leftDao.GetValueFromObject(leftObj, opeje.LeftProperty);
                            object rightVal = rightDao.GetValueFromObject(rightObj, opeje.RightProperty);
                            if (expr is EqualJoinExpression)
                            {
                                if (leftVal == null)
                                {
                                    if (rightVal != null)
                                    {
                                        matchesThisExpression = false;
                                    }
                                }
                                else
                                {
                                    if (rightVal == null)
                                    {
                                        matchesThisExpression = false;
                                    }
                                    else
                                    {
                                        if (!leftVal.Equals(rightVal))
                                        {
                                            matchesThisExpression = false;
                                        }
                                    }
                                }
                            }
                            else if ((expr is LesserJoinExpression) ||
                                     (expr is GreaterJoinExpression))
                            {
                                int thisCompareValue;
                                if (leftVal == null)
                                {
                                    thisCompareValue = rightVal == null ? 0 : 1;
                                }
                                else if (rightVal == null)
                                {
                                    thisCompareValue = -1;
                                }
                                else
                                {
                                    // If we're here, neither value was null.
                                    if (!(leftVal is IComparable))
                                    {
                                        throw new NotSupportedException(
                                            "Cannot check values that are not IComparable! leftVal: " +
                                            leftVal + ", rightVal: " + rightVal + ", expression: " + expr);
                                    }
                                    thisCompareValue = ((IComparable)leftVal).CompareTo(rightVal);
                                }
                                if (expr is LesserJoinExpression)
                                {
                                    matchesThisExpression = (thisCompareValue < 0);
                                }
                                else
                                {
                                    matchesThisExpression = (thisCompareValue > 0);
                                }
                            }
                            else
                            {
                                throw new NotSupportedException("Expression " + expr + " is an unsupported type.");
                            }
                        }
                        else
                        {
                            throw new NotSupportedException("Expression " + expr + " is an unsupported type.");
                        }
                        // Take into account whether the expression was NOTed.
                        if (expr.TrueOrNot())
                        {
                            if (!matchesThisExpression)
                            {
                                goodResult = false;
                            }
                        }
                        else
                        {
                            if (matchesThisExpression)
                            {
                                goodResult = false;
                            }
                        }
                    }
                    if (goodResult)
                    {
                        retVal.Add(new JoinResult<L, R>(leftObj, rightObj));
                        rightsThatMatched[rightObj] = rightObj;
                        anyGoodResults = true;
                    }
                }
                if (!anyGoodResults)
                {
                    // This is the "left" part of the outer join.
                    retVal.Add(new JoinResult<L, R>(leftObj, default(R)));
                }
            }
            // Now handle the "right" part of the outer join.
            foreach (R rightObj in rightObjs)
            {
                if (!rightsThatMatched.ContainsKey(rightObj))
                {
                    retVal.Add(new JoinResult<L, R>(default(L), rightObj));
                }
            }
            // Now sort them per the orders.
            if (crit.Orders.Count > 0)
            {
                retVal.Sort(new PseudoJoinSorter<L, R>(crit.Orders, leftDao, rightDao));
            } 
            if (crit.Start == -1 && crit.Limit == -1)
            {
                return retVal;
            }
            int start = 0, count = retVal.Count;
            if (crit.Limit != -1)
            {
                count = crit.Limit;
            }
            if (crit.Start != -1)
            {
                start = crit.Start;
                if (count == retVal.Count)
                {
                    count -= start;
                }
            }
            return retVal.GetRange(start, count);
        }
    }
}