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
    /// Not every consumer of DaoCriteria is required to support every combination of
    /// expressions, is it up to the consumer to throw NotSupportedExceptions.
    /// </summary>
    [Serializable]
    public class DaoCriteria
    {
        /// <summary>
        /// Whether this is an "AND" criteria or an "OR" criteria.
        /// </summary>
        public BooleanOperator BoolType;

        /// <summary>
        /// The individual expressions that make up this criteria.  Defaults to empty.
        /// Expressions can be added, cleared, etc.
        /// </summary>
        public readonly List<IExpression> Expressions = new List<IExpression>();

        /// <summary>
        /// The list of properties to sort on.  Defaults to empty, you may add, clear, reorder, etc.
        /// </summary>
        public readonly List<SortOrder> Orders = new List<SortOrder>();

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
        public DaoCriteria() : this(BooleanOperator.And) {}

        /// <summary>
        /// Constructs a blank criteria, which will return all records unless you customize it.
        /// </summary>
        /// <param name="howToAddExpressions">How expressions will be added together.  Determines
        ///                                   if we do exp1 AND exp2 AND exp3, or if we do
        ///                                   exp1 OR exp2 OR exp3.</param>
        public DaoCriteria(BooleanOperator howToAddExpressions)
            : this(null, howToAddExpressions) {}

        /// <summary>
        /// Constructs a criteria with one expression.  May be handy for cases
        /// where you only need one expression.
        /// </summary>
        /// <param name="firstExpr">The first expression to add.</param>
        public DaoCriteria(IExpression firstExpr)
            : this(firstExpr, BooleanOperator.And) {}

        /// <summary>
        /// Constructs a criteria with one expression.
        /// </summary>
        /// <param name="firstExpr">The first expression to add.</param>
        /// <param name="howToAddExpressions">How expressions will be added together.  Determines
        ///                                   if we do exp1 AND exp2 AND exp3, or if we do
        ///                                   exp1 OR exp2 OR exp3.</param>
        public DaoCriteria(IExpression firstExpr, BooleanOperator howToAddExpressions)
        {
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
            BoolType = BooleanOperator.And;
            Expressions.Clear();
            Orders.Clear();
            Start = -1;
            Limit = -1;
        }

        /// <summary>
        /// Makes this DaoCriteria into a copy of the other one.  Any existing
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
                Expressions.Clear();
                Expressions.AddRange(other.Expressions);
                Orders.Clear();
                Orders.AddRange(other.Orders);
                Start = other.Start;
                Limit = other.Limit;
            }
        }
    }
}
