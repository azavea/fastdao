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

namespace Azavea.Open.DAO.Util
{
    /// <summary>
    /// Sorts the results of a join according to the orders specified on the criteria.
    /// </summary>
    /// <typeparam name="L">The type of object returned by the left DAO.</typeparam>
    /// <typeparam name="R">The type of object returned by the right DAO.</typeparam>
    public class PseudoJoinSorter<L, R> : IComparer<JoinResult<L, R>>
        where L : class, new() where R : class, new()
    {
        private readonly List<Criteria.Joins.JoinSortOrder> _orders;
        private readonly IFastDaoReader<L> _leftDao;
        private readonly IFastDaoReader<R> _rightDao;

        /// <summary>
        /// Constructs the sorter with the things to sort on.
        /// </summary>
        /// <param name="orders">Orders from the join criteria.</param>
        /// <param name="leftDao">The "left" DAO we are joining.</param>
        /// <param name="rightDao">The "right" DAO we are joining.</param>
        public PseudoJoinSorter(List<Criteria.Joins.JoinSortOrder> orders, IFastDaoReader<L> leftDao, IFastDaoReader<R> rightDao)
        {
            _orders = orders;
            _leftDao = leftDao;
            _rightDao = rightDao;
        }

        /// <summary>
        ///                     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        ///                     Value 
        ///                     Condition 
        ///                     Less than zero
        ///                 <paramref name="x" /> is less than <paramref name="y" />.
        ///                     Zero
        ///                 <paramref name="x" /> equals <paramref name="y" />.
        ///                     Greater than zero
        ///                 <paramref name="x" /> is greater than <paramref name="y" />.
        /// </returns>
        /// <param name="x">
        ///                     The first object to compare.
        ///                 </param>
        /// <param name="y">
        ///                     The second object to compare.
        ///                 </param>
        public int Compare(JoinResult<L, R> x, JoinResult<L, R> y)
        {
            if (x == null)
            {
                throw new ArgumentNullException("x", "Cannot compare null x value with y: " + y);
            }
            if (y == null)
            {
                throw new ArgumentNullException("y", "Cannot compare null y value with x: " + x);
            }
            // If we're here, neither was null.
            foreach (Criteria.Joins.JoinSortOrder order in _orders)
            {
                int thisCompareValue;
                object xResult = order.IsForLeftDao ? (object)x.Left : x.Right;
                object yResult = order.IsForLeftDao ? (object)y.Left : y.Right;
                // Compare the possibility of null records (I.E. outer joins).
                // We are currently sorting nulls AFTER normal values.
                // The SQL spec does not specify whether nulls should be sorted
                // before or after, and databases vary.
                // 
                // DBs that sort nulls BEFORE values (null, 1, 2, 3):
                //     SQL Server
                //     Access
                //     Firebird
                // 
                // DBs that sort nulls AFTER values (1, 2, 3, null):
                //     Oracle
                //     SQLite
                if (xResult == null)
                {
                    thisCompareValue = yResult == null ? 0 : 1;
                }
                else if (yResult == null)
                {
                    thisCompareValue = -1;
                }
                else
                {
                    object xVal = order.IsForLeftDao
                                      ? _leftDao.GetValueFromObject(x.Left, order.Property)
                                      : _rightDao.GetValueFromObject(x.Right, order.Property);
                    object yVal = order.IsForLeftDao
                                      ? _leftDao.GetValueFromObject(y.Left, order.Property)
                                      : _rightDao.GetValueFromObject(y.Right, order.Property);
                    if (xVal == null)
                    {
                        thisCompareValue = yVal == null ? 0 : 1;
                    }
                    else if (yVal == null)
                    {
                        thisCompareValue = -1;
                    }
                    else
                    {
                        // If we're here, neither value was null.
                        if (!(xVal is IComparable))
                        {
                            throw new NotSupportedException(
                                "Cannot sort on values that are not IComparable! xVal: " +
                                xVal + ", yVal: " + yVal);
                        }
                        thisCompareValue = ((IComparable)xVal).CompareTo(yVal);
                    }
                }
                if (thisCompareValue != 0)
                {
                    if (order.Direction == SortType.Desc)
                    {
                        thisCompareValue *= -1;
                    }
                    return thisCompareValue;
                }
            }
            return 0;
        }
    }
}