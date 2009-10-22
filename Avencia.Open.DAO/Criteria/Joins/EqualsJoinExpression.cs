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

namespace Avencia.Open.DAO.Criteria.Joins
{
    /// <summary>
    /// LeftProperty == RightProperty.  Does not accept wild cards.
    /// </summary>
    [Serializable]
    public class EqualsJoinExpression : AbstractOnePropertyEachJoinExpression
    {
        /// <summary>
        /// LeftProperty == RightProperty.  Does not accept wild cards.
        /// </summary>
        /// <param name="leftProperty">The name of the property on the object returned by the
        ///                            left DAO that we are comparing.</param>
        /// <param name="rightProperty">The name of the property on the object returned by the
        ///                             right DAO that we are comparing.</param>
        public EqualsJoinExpression(string leftProperty, string rightProperty)
            : this(leftProperty, rightProperty, true) { }
        /// <summary>
        /// LeftProperty == RightProperty.  Does not accept wild cards.
        /// </summary>
        /// <param name="leftProperty">The name of the property on the object returned by the
        ///                            left DAO that we are comparing.</param>
        /// <param name="rightProperty">The name of the property on the object returned by the
        ///                             right DAO that we are comparing.</param>
        /// <param name="trueOrNot">True means look for matches (I.E. ==),
        ///                         false means look for non-matches (I.E. !=)</param>
        public EqualsJoinExpression(string leftProperty,
                                   string rightProperty, bool trueOrNot)
            : base(leftProperty, rightProperty, trueOrNot) { }

        /// <summary>
        /// Produces an expression that is the exact opposite of this expression.
        /// The new expression should exclude everything this one includes, and
        /// include everything this one excludes.
        /// </summary>
        /// <returns>The inverse of this expression.</returns>
        public override IExpression Invert()
        {
            return new EqualsJoinExpression(LeftProperty, RightProperty, !_trueOrNot);
        }

        /// <summary>
        /// For some implementations, it's simpler to only implement one-sided joins.  So
        /// to handle right (or left) joins, you want to flip the criteria so the right
        /// and left daos are swapped and you can do a left (or right) join instead.
        /// </summary>
        /// <returns>A copy of this expression with the left and right orientation swapped.</returns>
        public override IJoinExpression Flip()
        {
            return new EqualsJoinExpression(RightProperty, LeftProperty, _trueOrNot);
        }
    }
}