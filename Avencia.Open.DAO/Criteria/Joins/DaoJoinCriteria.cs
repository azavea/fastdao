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
using log4net;
using StringHelper=Avencia.Open.Common.StringHelper;

namespace Avencia.Open.DAO.Criteria.Joins
{
    /// <summary>
    /// This class defines how to join two FastDAOs together.
    /// </summary>
    public class DaoJoinCriteria
    {
        /// <summary>
        /// This enumeration defines the possible types of join.
        /// </summary>
        public enum JoinType
        {
            /// <summary>
            /// Inner join, only include when records exist in both the left DAO and the right DAO.
            /// </summary>
            Inner,
            /// <summary>
            /// Left outer join, include when records exist in the left DAO, even if they
            /// don't exist in the right DAO.
            /// </summary>
            LeftOuter,
            /// <summary>
            /// Right outer join, include when records exist in the right DAO, even if they
            /// don't exist in the left DAO.
            /// </summary>
            RightOuter,
            /// <summary>
            /// Outer join, include when records exist in either the left or right DAO, even
            /// if they don't exist in the other DAO.
            /// </summary>
            Outer,
            //// <summary>
            //// TODO: Not sure how to support this... the idea is it's not just a join but
            //// something like "select from a where a.field = (select max(bfield) from b)"
            //// </summary>
            //NestedSelect
        }

        /// <summary>
        /// This is similar to a DaoCriteria's SortOrder, except it is necessary
        /// to specify which DAO, the left or the right, has this field we're sorting on.
        /// </summary>
        public class JoinSortOrder : DaoCriteria.SortOrder
        {
            /// <summary>
            /// If true, the property we're sorting on comes from the left
            /// DAO.  If false, it comes from the right DAO.
            /// </summary>
            public bool IsForLeftDao;

            /// <summary>
            /// For some implementations, it's simpler to only implement one-sided joins.  So
            /// to handle right (or left) joins, you want to flip the criteria so the right
            /// and left daos are swapped and you can do a left (or right) join instead.
            /// </summary>
            /// <returns>A copy of this sort order with the left/right orientation swapped.</returns>
            public JoinSortOrder Flip()
            {
                JoinSortOrder retVal = new JoinSortOrder();
                retVal.Direction = Direction;
                retVal.Property = Property;
                retVal.IsForLeftDao = !IsForLeftDao;
                return retVal;
            }

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
                return (IsForLeftDao ? "LEFT." : "RIGHT.") + base.ToString();
            }
        }
        /// <summary>
        /// log4net logger for logging any appropriate messages.
        /// </summary>
        protected static ILog _log = LogManager.GetLogger(
            new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().DeclaringType.Namespace);
        /// <summary>
        /// These are the criteria applied to the left DAO's records by themselves.
        /// I.E. only return rows where left.field == 5.
        /// </summary>
        public DaoCriteria LeftCriteria;
        /// <summary>
        /// These are the criteria applied to the right DAO's records by themselves.
        /// I.E. only return rows where right.field is not null.
        /// </summary>
        public DaoCriteria RightCriteria;

        /// <summary>
        /// This is private so no one can edit the expressions collection directly.
        /// </summary>
        private readonly List<IJoinExpression> _expressions = new List<IJoinExpression>();
        /// <summary>
        /// The individual expressions that make up this criteria, allowing the class using this Criteria
        /// to know what the criteria are.  Adding expressions should be done with the AddExpression method.
        /// </summary>
        public readonly IEnumerable<IJoinExpression> Expressions;

        /// <summary>
        /// This is private so no one can edit the orders collection directly.
        /// </summary>
        private readonly List<JoinSortOrder> _orders = new List<JoinSortOrder>();
        /// <summary>
        /// Returns a list of properties to sort on, allowing the class using this Criteria
        /// to know what the sort order should be.  Adding sorts should be done with the AddXXXSort
        /// methods.
        /// </summary>
        public readonly IEnumerable<JoinSortOrder> Orders;

        /// <summary>
        /// Whether this is an "AND" criteria or an "OR" criteria.
        /// </summary>
        public DaoCriteria.BooleanType BoolType;
        /// <summary>
        /// Whether this is an inner join, left join, etc.
        /// </summary>
        public JoinType TypeOfJoin;

        /// <summary>
        /// Constructs a blank inner join criteria, which will return all records unless you customize it.
        /// All expressions added to it will be ANDed together.
        /// </summary>
        public DaoJoinCriteria()
            : this(JoinType.Inner, null, DaoCriteria.BooleanType.And) { }
        /// <summary>
        /// Constructs an inner join criteria with one expression.  May be handy for cases
        /// where you only need one expression.
        /// </summary>
        /// <param name="firstExpr">The first expression to add.</param>
        public DaoJoinCriteria(IJoinExpression firstExpr)
            : this(JoinType.Inner, firstExpr, DaoCriteria.BooleanType.And) { }
        /// <summary>
        /// Constructs a blank inner join criteria, which will return all records unless you customize it.
        /// </summary>
        /// <param name="howToAddExpressions">How expressions will be added together.  Determines
        ///                                   if we do exp1 AND exp2 AND exp3, or if we do
        ///                                   exp1 OR exp2 OR exp3.</param>
        public DaoJoinCriteria(DaoCriteria.BooleanType howToAddExpressions)
            : this(JoinType.Inner, null, howToAddExpressions) { }
        /// <summary>
        /// Constructs an inner join criteria with one expression.
        /// </summary>
        /// <param name="firstExpr">The first expression to add.</param>
        /// <param name="howToAddExpressions">How expressions will be added together.  Determines
        ///                                   if we do exp1 AND exp2 AND exp3, or if we do
        ///                                   exp1 OR exp2 OR exp3.</param>
        public DaoJoinCriteria(IJoinExpression firstExpr,
            DaoCriteria.BooleanType howToAddExpressions)
            : this(JoinType.Inner, firstExpr, howToAddExpressions) { }
        /// <summary>
        /// Constructs a blank criteria, which will return all records unless you customize it.
        /// All expressions added to it will be ANDed together.
        /// </summary>
        /// <param name="typeOfJoin">Is this an inner join, left join, etc.</param>
        public DaoJoinCriteria(JoinType typeOfJoin)
            : this(typeOfJoin, null, DaoCriteria.BooleanType.And) { }
        /// <summary>
        /// Constructs a criteria with one expression.  May be handy for cases
        /// where you only need one expression.
        /// </summary>
        /// <param name="firstExpr">The first expression to add.</param>
        /// <param name="typeOfJoin">Is this an inner join, left join, etc.</param>
        public DaoJoinCriteria(JoinType typeOfJoin, IJoinExpression firstExpr)
            : this(typeOfJoin, firstExpr, DaoCriteria.BooleanType.And) { }
        /// <summary>
        /// Constructs a blank criteria, which will return all records unless you customize it.
        /// </summary>
        /// <param name="howToAddExpressions">How expressions will be added together.  Determines
        ///                                   if we do exp1 AND exp2 AND exp3, or if we do
        ///                                   exp1 OR exp2 OR exp3.</param>
        /// <param name="typeOfJoin">Is this an inner join, left join, etc.</param>
        public DaoJoinCriteria(JoinType typeOfJoin, DaoCriteria.BooleanType howToAddExpressions)
            : this(typeOfJoin, null, howToAddExpressions) { }
        /// <summary>
        /// Constructs a criteria with one expression.
        /// </summary>
        /// <param name="firstExpr">The first expression to add.</param>
        /// <param name="howToAddExpressions">How expressions will be added together.  Determines
        ///                                   if we do exp1 AND exp2 AND exp3, or if we do
        ///                                   exp1 OR exp2 OR exp3.</param>
        /// <param name="typeOfJoin">Is this an inner join, left join, etc.</param>
        public DaoJoinCriteria(JoinType typeOfJoin, IJoinExpression firstExpr,
            DaoCriteria.BooleanType howToAddExpressions)
        {
            // Set these once here, rather than using Properties, because Properties are actually
            // function calls and we want this to be fast as possible.
            Orders = _orders;
            Expressions = _expressions;

            TypeOfJoin = typeOfJoin;
            BoolType = howToAddExpressions;
            if (firstExpr != null)
            {
                _expressions.Add(firstExpr);
            }
        }
        /// <summary>
        /// This is the new standard way of adding expressions to a serializable criteria.
        /// </summary>
        /// <param name="expr">Anything that implements the IExpression interface.</param>
        public void AddExpression(IJoinExpression expr)
        {
            if (expr == null)
            {
                throw new ArgumentNullException("expr", "Cannot add a null expression!");
            }
            _expressions.Add(expr);
        }

        /// <summary>
        /// Helper method called by the public AddXXXSort methods.
        /// </summary>
        /// <param name="isForLeftDao">If true, this is a sort on a property on the left DAO.  
        ///                            If false, the right DAO.</param>
        /// <param name="property">Data field to sort on.</param>
        /// <param name="sortType">How to sort.</param>
        private void AddSort(bool isForLeftDao, string property, DaoCriteria.SortType sortType)
        {
            JoinSortOrder order = new JoinSortOrder();
            order.Property = property;
            order.Direction = sortType;
            order.IsForLeftDao = isForLeftDao;
            _orders.Add(order);
        }

        /// <summary>
        /// Adds an ascending sort (see SortType.Asc).
        /// </summary>
        /// <param name="isForLeftDao">If true, this is a sort on a property on the left DAO.  
        ///                            If false, the right DAO.</param>
        /// <param name="property">Data field to sort on.</param>
        public void AddAscSort(bool isForLeftDao, string property)
        {
            AddSort(isForLeftDao, property, DaoCriteria.SortType.Asc);
        }

        /// <summary>
        /// Adds a descending sort (see SortType.Desc).
        /// </summary>
        /// <param name="isForLeftDao">If true, this is a sort on a property on the left DAO.  
        ///                            If false, the right DAO.</param>
        /// <param name="property">Data field to sort on.</param>
        public void AddDescSort(bool isForLeftDao, string property)
        {
            AddSort(isForLeftDao, property, DaoCriteria.SortType.Desc);
        }

        /// <summary>
        /// Completely clears the object so that it may be used over again.
        /// </summary>
        public void Clear()
        {
            TypeOfJoin = JoinType.Inner;
            BoolType = DaoCriteria.BooleanType.And;
            _expressions.Clear();
            _orders.Clear();
        }

        /// <summary>
        /// For some implementations, it's simpler to only implement one-sided joins.  So
        /// to handle right (or left) joins, you want to flip the criteria so the right
        /// and left daos are swapped and you can do a left (or right) join instead.
        /// </summary>
        /// <returns>This criteria converted from a left to a right join, or vice versa.</returns>
        public DaoJoinCriteria Flip()
        {
            DaoJoinCriteria retVal = new DaoJoinCriteria();
            switch (TypeOfJoin)
            {
                case JoinType.LeftOuter:
                    retVal.TypeOfJoin = JoinType.RightOuter;
                    break;
                case JoinType.RightOuter:
                    retVal.TypeOfJoin = JoinType.LeftOuter;
                    break;
                default:
                    throw new NotSupportedException("Cannot flip a criteria with join type: " + TypeOfJoin);
            }
            if (_log.IsDebugEnabled)
            {
                _log.Debug("Flipping criteria: " + this +
                           ".  There is a potential performance gain if you can write your original join as type: " +
                           retVal.TypeOfJoin);
            }
            retVal.BoolType = BoolType;
            foreach (IJoinExpression expr in Expressions)
            {
                retVal._expressions.Add(expr.Flip());
            }
            foreach (JoinSortOrder order in Orders)
            {
                retVal._orders.Add(order.Flip());
            }
            return retVal;
        }

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
            return TypeOfJoin + " JOIN, expressions: " + StringHelper.Join(Expressions) + " (" +
                   BoolType + "ed), orders: " + StringHelper.Join(Orders) + ", where LEFT: " +
                   LeftCriteria + " and RIGHT: " + RightCriteria;
        }
    }
}
