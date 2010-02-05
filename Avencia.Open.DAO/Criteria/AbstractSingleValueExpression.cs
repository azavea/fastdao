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
    /// Base class for expressions that compare one property with one value.
    /// </summary>
    [Serializable]
    public abstract class AbstractSingleValueExpression : AbstractSinglePropertyExpression
    {
        /// <summary>
        /// The value to compare with.
        /// </summary>
        public readonly object Value;
        /// <summary>
        /// Base class for expressions that only check one property.
        /// </summary>
        /// <param name="property">The data class' property/field being compared.
        ///                        May not be null.</param>
        /// <param name="value">The value to check for.  May be null only if nullOk is true.</param>
        /// <param name="nullOk">Whether the value can be null or not.</param>
        /// <param name="trueOrNot">True means look for matches (I.E. ==),
        ///                         false means look for non-matches (I.E. !=)</param>
        protected AbstractSingleValueExpression(string property, object value, bool nullOk, bool trueOrNot)
            : base(property, trueOrNot)
        {
            if ((!nullOk) && (value == null))
            {
                throw new ArgumentNullException("value", "Value parameter cannot be null.");
            }
            Value = value;
        }

        /// <exclude/>
        public override string ToString()
        {
            return "Value=" + Value + ", " + base.ToString();
        }
    }
}