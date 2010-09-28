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

namespace Azavea.Open.DAO.Criteria
{
    /// <summary>
    /// Property &lt; Value
    /// </summary>
    [Serializable]
    public class LesserExpression : AbstractSingleValueExpression
    {
        /// <summary>
        /// Property &lt; Value
        /// </summary>
        /// <param name="property">The data class' property/field being compared.
        ///                        May not be null.</param>
        /// <param name="value">The value to check for.  May not be null.</param>
        public LesserExpression(string property, object value)
            : this(property, value, true) {}
        /// <summary>
        /// Property &lt; Value
        /// </summary>
        /// <param name="property">The data class' property/field being compared.
        ///                        May not be null.</param>
        /// <param name="value">The value to check for.  May not be null.</param>
        /// <param name="trueOrNot">True means look for matches (I.E. &lt;),
        ///                         false means look for non-matches (I.E. &gt;=)</param>
        public LesserExpression(string property, object value, bool trueOrNot)
            : base(property, value, false, trueOrNot) {}

        /// <summary>
        /// Produces an expression that is the exact opposite of this expression.
        /// The new expression should exclude everything this one includes, and
        /// include everything this one excludes.
        /// </summary>
        /// <returns>The inverse of this expression.</returns>
        public override IExpression Invert()
        {
            return new LesserExpression(Property, Value, !_trueOrNot);
        }

        /// <exclude/>
        public override string ToString()
        {
            return Property + (TrueOrNot() ? " < " : " >= ") + Value;
        }
    }
}