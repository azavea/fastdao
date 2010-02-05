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

namespace Avencia.Open.DAO.Criteria
{
    /// <summary>
    /// Min &lt;= Property &lt;= Max
    /// </summary>
    [Serializable]
    public class BetweenExpression : AbstractSinglePropertyExpression
    {
        /// <summary>
        /// The minimum acceptable value.
        /// May not be null.
        /// </summary>
        public readonly object Min;
        /// <summary>
        /// The maximum acceptable value.
        /// May not be null.
        /// </summary>
        public readonly object Max;

        /// <summary>
        /// PropertyValue &lt;= Property &lt;= PropertyValue2
        /// </summary>
        /// <param name="property">The data class' property/field being compared.
        ///                        May not be null.</param>
        /// <param name="min">The minimum acceptable value. May not be null.</param>
        /// <param name="max">The maximum acceptable value. May not be null.</param>
        public BetweenExpression(string property, object min, object max)
            : this(property, min, max, true) {}
        /// <summary>
        /// PropertyValue &lt;= Property &lt;= PropertyValue2
        /// </summary>
        /// <param name="property">The data class' property/field being compared.
        ///                        May not be null.</param>
        /// <param name="min">The minimum acceptable value. May not be null.</param>
        /// <param name="max">The maximum acceptable value. May not be null.</param>
        /// <param name="trueOrNot">True means look for matches (I.E. ==),
        ///                         false means look for non-matches (I.E. !=)</param>
        public BetweenExpression(string property, object min, object max, bool trueOrNot)
            : base(property, trueOrNot)
        {
            if (min == null)
            {
                throw new ArgumentNullException("min", "Min parameter cannot be null.");
            }
            Min = min;
            if (max == null)
            {
                throw new ArgumentNullException("max", "Max parameter cannot be null.");
            }
            Max = max;
        }

        /// <summary>
        /// Produces an expression that is the exact opposite of this expression.
        /// The new expression should exclude everything this one includes, and
        /// include everything this one excludes.
        /// </summary>
        /// <returns>The inverse of this expression.</returns>
        public override IExpression Invert()
        {
            return new BetweenExpression(Property, Min, Max, !_trueOrNot);
        }

        /// <exclude/>
        public override string ToString()
        {
            return "Min=" + Min + ", Max=" + Max + ", " + base.ToString();
        }
    }
}