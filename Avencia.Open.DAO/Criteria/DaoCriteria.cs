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

using System;
using System.Collections.Generic;

namespace Avencia.Open.DAO.Criteria
{
    /// <summary>
    /// This class represents data query criteria.  Originally designed for use in the NHibernate
    /// helper, it is also now used in the Avencia.Database assemblies, and in theory could be
    /// used for any organized-data-processing (DataSets, feature classes, etc).
    /// 
    /// Expressions are assumed to be added in the order you want them.
    /// AddEqual("x", 5);
    /// AddEqual("y", 4);
    /// AddEqual("z", 3);
    /// will produce "x = 5 AND y = 4 AND z = 3".
    /// 
    /// Orders are considered in the order they are added, I.E. results will be sorted
    /// first by the first order, then the second, etc.
    /// 
    /// Not every consumer of SerializableCriteria is required to support every combination of
    /// expressions, is it up to the consumer to throw NotSupportedExceptions.
    /// </summary>
    [Serializable]
    public class DaoCriteria
    {
        /// <summary>
        /// On a sort parameter, indicates what order that field should be sorted in.
        /// </summary>
        [Serializable]
        public enum SortType
        {
            /// <summary>
            /// Ascending, numeric (1 - 999) or alphabetic (A - Z) or CompareTo,
            /// depending on the data type and the class using the SerializableCriteria.
            /// </summary>
            Asc,
            /// <summary>
            /// Descending, numeric (999 - 1) or alphabetic (Z - A) or CompareTo,
            /// depending on the data type and the class using the SerializableCriteria.
            /// </summary>
            Desc,
            /// <summary>
            /// Indicates that this is a computed parameter (such as "Field1 + field2") and
            /// it should be sorted in whatever the natural order is (up to the class using
            /// this SerializableCritera to determine).
            /// </summary>
            Computed
        }

        /// <summary>
        /// A simple class that holds a single sort criterion.
        /// </summary>
        [Serializable]
        public class SortOrder
        {
            /// <summary>
            /// The property to sort on.
            /// </summary>
            public string Property;
            /// <summary>
            /// The direction to sort based on the Property.
            /// </summary>
            public SortType Direction;

            ///<summary>
            ///
            ///                    Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
            ///                
            ///</summary>
            ///
            ///<returns>
            ///
            ///                    A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
            ///                
            ///</returns>
            ///<filterpriority>2</filterpriority>
            public override string ToString()
            {
                return Property + " " + Direction;
            }
        }

        /// <summary>
        /// Indicates whether the given criteria is an AND or and OR.
        /// For example:
        /// Height &gt; 5 OR Width &gt; 5 =
        /// "Height [MatchType.Greater] 5 [BooleanType.Or]" and
        /// "Width [Matchtype.Greater] 5 [BooleanType.Or]"
        /// 
        /// Height &gt; 5 AND Width &gt; 5 =
        /// "Height [MatchType.Greater] 5 [BooleanType.AND]" and
        /// "Width [Matchtype.Greater] 5 [BooleanType.AND]"
        /// 
        /// To represent complicated criteria groups such as 
        /// "Height &gt; 5 AND (Weight &gt; 5 OR Weight == 0)", you need to use
        /// the CriteriaExpression to nest the OR expression inside another,
        /// AND criteria:
        /// SerializableCriteria(heightExp AND SerializableCriteria(weightExp1 OR weightExp2)).
        /// </summary>
        [Serializable]
        public enum BooleanType
        {
            /// <summary>
            /// Indicates both are required.
            /// </summary>
            And,
            /// <summary>
            /// Indicates either one or the other is required.
            /// </summary>
            Or
        }

        /// <summary>
        /// Whether this is an "AND" criteria or an "OR" criteria.
        /// </summary>
        public BooleanType BoolType;

        /// <summary>
        /// This is private so no one can edit the expressions collection directly.
        /// </summary>
        private readonly List<IExpression> _expressions = new List<IExpression>();
        /// <summary>
        /// The individual expressions that make up this criteria, allowing the class using this Criteria
        /// to know what the criteria are.  Adding expressions should be done with the AddExpression method.
        /// </summary>
        public readonly IEnumerable<IExpression> Expressions;

        /// <summary>
        /// This is private so no one can edit the orders collection directly.
        /// </summary>
        private readonly List<SortOrder> _orders = new List<SortOrder>();
        /// <summary>
        /// Returns a list of properties to sort on, allowing the class using this Criteria
        /// to know what the sort order should be.  Adding sorts should be done with the AddXXXSort
        /// methods.
        /// </summary>
        public readonly IEnumerable<SortOrder> Orders;

        /// <summary>
        /// Used to limit the data returned, only data rows Start to Start + Limit will be returned.
        /// A value of -1 means ignore this parameter.
        /// </summary>
        public int Start = -1;
        /// <summary>
        /// Used to limit the data returned, only data rows Start to Start + Limit will be returned.
        /// A value of -1 means ignore this parameter.
        /// </summary>
        public int Limit = -1;

        /// <summary>
        /// Constructs a blank criteria, which will return all records unless you customize it.
        /// All expressions added to it will be ANDed together.
        /// </summary>
        public DaoCriteria() : this(BooleanType.And) {}

        /// <summary>
        /// Constructs a blank criteria, which will return all records unless you customize it.
        /// </summary>
        /// <param name="howToAddExpressions">How expressions will be added together.  Determines
        ///                                   if we do exp1 AND exp2 AND exp3, or if we do
        ///                                   exp1 OR exp2 OR exp3.</param>
        public DaoCriteria(BooleanType howToAddExpressions)
            : this(null, howToAddExpressions) {}

        /// <summary>
        /// Constructs a criteria with one expression.  May be handy for cases
        /// where you only need one expression.
        /// </summary>
        /// <param name="firstExpr">The first expression to add.</param>
        public DaoCriteria(IExpression firstExpr)
            : this(firstExpr, BooleanType.And) {}

        /// <summary>
        /// Constructs a criteria with one expression.
        /// </summary>
        /// <param name="firstExpr">The first expression to add.</param>
        /// <param name="howToAddExpressions">How expressions will be added together.  Determines
        ///                                   if we do exp1 AND exp2 AND exp3, or if we do
        ///                                   exp1 OR exp2 OR exp3.</param>
        public DaoCriteria(IExpression firstExpr, BooleanType howToAddExpressions)
        {
            // Set these once here, rather than using Properties, because Properties are actually
            // function calls and we want this to be fast as possible.
            Orders = _orders;
            Expressions = _expressions;

            BoolType = howToAddExpressions;
            if (firstExpr != null)
            {
                _expressions.Add(firstExpr);
            }
        }

        /// <summary>
        /// Adds any expression to the criteria.
        /// NOTE: Not all data access layers support all expressions.
        /// </summary>
        /// <param name="expr">Anything that implements the IExpression interface.</param>
        public void AddExpression(IExpression expr)
        {
            if (expr == null)
            {
                throw new ArgumentNullException("expr", "Cannot add a null expression!");
            }
            _expressions.Add(expr);
        }

        /// <summary>
        /// Removes all expressions from this SerializableCriteria.
        /// </summary>
        public void ClearExpressions()
        {
            _expressions.Clear();
        }

        /// <summary>
        /// Helper method called by the public AddXXXSort methods.
        /// </summary>
        /// <param name="property">Data field to sort on.</param>
        /// <param name="sortType">How to sort.</param>
        private void AddSort(string property, SortType sortType)
        {
            SortOrder order = new SortOrder();
            order.Property = property;
            order.Direction = sortType;
            _orders.Add(order);
        }

        /// <summary>
        /// Adds an ascending sort (see SortType.Asc).
        /// </summary>
        /// <param name="property">Data field to sort on.</param>
        public void AddAscSort(string property)
        {
            AddSort(property, SortType.Asc);
        }

        /// <summary>
        /// Adds a descending sort (see SortType.Desc).
        /// </summary>
        /// <param name="property">Data field to sort on.</param>
        public void AddDescSort(string property)
        {
            AddSort(property, SortType.Desc);
        }

        /// <summary>
        /// Adds a sort based on a computed expression (see SortType.Computer).
        /// </summary>
        /// <param name="expression">Expression to sort on.</param>
        public void AddComputedSort(string expression)
        {
            AddSort(expression, SortType.Computed);
        }

        /// <summary>
        /// Removes all existing sort orders.
        /// </summary>
        public void ClearSortOrder()
        {
            _orders.Clear();
        }

        /// <summary>
        /// Completely clears the object so that it may be used over again.
        /// </summary>
        public void Clear()
        {
            BoolType = BooleanType.And;
            _expressions.Clear();
            _orders.Clear();
            Start = -1;
            Limit = -1;
        }

        /// <summary>
        /// Makes this SerializableCriteria into a copy of the other one.  Any existing
        /// orders, expressions, etc. on this one are lost.
        /// </summary>
        /// <param name="other">Criteria to copy everything from.</param>
        public void CopyFrom(DaoCriteria other)
        {
            if (other == null)
            {
                Clear();
            }
            else
            {
                BoolType = other.BoolType;
                _expressions.Clear();
                _expressions.AddRange(other.Expressions);
                _orders.Clear();
                _orders.AddRange(other.Orders);
                Start = other.Start;
                Limit = other.Limit;
            }
        }
    }
}
