// Copyright (c) 2004-2010 Avencia, Inc.
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
using Avencia.Open.Common;
using log4net;

namespace Avencia.Open.DAO.Criteria.Joins
{
    /// <summary>
    /// This class defines how to join two FastDAOs together.
    /// </summary>
    public class DaoJoinCriteria
    {
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
        /// The individual expressions that make up this criteria, defaults to empty.
        /// </summary>
        public readonly List<IJoinExpression> Expressions = new List<IJoinExpression>();

        /// <summary>
        /// The list of properties to sort on, defaults to empty.
        /// </summary>
        public readonly List<JoinSortOrder> Orders = new List<JoinSortOrder>();

        /// <summary>
        /// Whether this is an "AND" criteria or an "OR" criteria.
        /// </summary>
        public BooleanOperator BoolType;
        /// <summary>
        /// Whether this is an inner join, left join, etc.
        /// </summary>
        public JoinType TypeOfJoin;

        /// <summary>
        /// Constructs a blank inner join criteria, which will return all records unless you customize it.
        /// All expressions added to it will be ANDed together.
        /// </summary>
        public DaoJoinCriteria()
            : this(JoinType.Inner, null, BooleanOperator.And) { }
        /// <summary>
        /// Constructs an inner join criteria with one expression.  May be handy for cases
        /// where you only need one expression.
        /// </summary>
        /// <param name="firstExpr">The first expression to add.</param>
        public DaoJoinCriteria(IJoinExpression firstExpr)
            : this(JoinType.Inner, firstExpr, BooleanOperator.And) { }
        /// <summary>
        /// Constructs a blank inner join criteria, which will return all records unless you customize it.
        /// </summary>
        /// <param name="howToAddExpressions">How expressions will be added together.  Determines
        ///                                   if we do exp1 AND exp2 AND exp3, or if we do
        ///                                   exp1 OR exp2 OR exp3.</param>
        public DaoJoinCriteria(BooleanOperator howToAddExpressions)
            : this(JoinType.Inner, null, howToAddExpressions) { }
        /// <summary>
        /// Constructs an inner join criteria with one expression.
        /// </summary>
        /// <param name="firstExpr">The first expression to add.</param>
        /// <param name="howToAddExpressions">How expressions will be added together.  Determines
        ///                                   if we do exp1 AND exp2 AND exp3, or if we do
        ///                                   exp1 OR exp2 OR exp3.</param>
        public DaoJoinCriteria(IJoinExpression firstExpr,
            BooleanOperator howToAddExpressions)
            : this(JoinType.Inner, firstExpr, howToAddExpressions) { }
        /// <summary>
        /// Constructs a blank criteria, which will return all records unless you customize it.
        /// All expressions added to it will be ANDed together.
        /// </summary>
        /// <param name="typeOfJoin">Is this an inner join, left join, etc.</param>
        public DaoJoinCriteria(JoinType typeOfJoin)
            : this(typeOfJoin, null, BooleanOperator.And) { }
        /// <summary>
        /// Constructs a criteria with one expression.  May be handy for cases
        /// where you only need one expression.
        /// </summary>
        /// <param name="firstExpr">The first expression to add.</param>
        /// <param name="typeOfJoin">Is this an inner join, left join, etc.</param>
        public DaoJoinCriteria(JoinType typeOfJoin, IJoinExpression firstExpr)
            : this(typeOfJoin, firstExpr, BooleanOperator.And) { }
        /// <summary>
        /// Constructs a blank criteria, which will return all records unless you customize it.
        /// </summary>
        /// <param name="howToAddExpressions">How expressions will be added together.  Determines
        ///                                   if we do exp1 AND exp2 AND exp3, or if we do
        ///                                   exp1 OR exp2 OR exp3.</param>
        /// <param name="typeOfJoin">Is this an inner join, left join, etc.</param>
        public DaoJoinCriteria(JoinType typeOfJoin, BooleanOperator howToAddExpressions)
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
            BooleanOperator howToAddExpressions)
        {
            TypeOfJoin = typeOfJoin;
            BoolType = howToAddExpressions;
            if (firstExpr != null)
            {
                Expressions.Add(firstExpr);
            }
        }

        /// <summary>
        /// Completely clears the object so that it may be used over again.
        /// </summary>
        public void Clear()
        {
            TypeOfJoin = JoinType.Inner;
            BoolType = BooleanOperator.And;
            Expressions.Clear();
            Orders.Clear();
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
                retVal.Expressions.Add(expr.Flip());
            }
            foreach (JoinSortOrder order in Orders)
            {
                retVal.Orders.Add(order.Flip());
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
