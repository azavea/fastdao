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
using System.Collections;

namespace Avencia.Open.DAO.Criteria
{
    /// <summary>
    /// Represents a hand-written custom expression.  May be a chunk of SQL, or may be
    /// something else relevant to the underlying data access layer.
    /// 
    /// This is both a very powerful and very dangerous expression; it allows you
    /// to construct complicated logic, but you can easily pass invalid input (that
    /// doesn't parse), sloppy input (hard coded table / column names), or even
    /// evil input (sql injection).
    /// 
    /// MAKE SURE to sanitize any user input that makes its way into this expression!
    /// </summary>
    [Serializable]
    public class HandWrittenExpression : AbstractExpression
    {
        /// <summary>
        /// The expression text.  Can be almost anything, as long as it means
        /// something to the underlying Dao layer.
        /// </summary>
        public readonly string Expression;
        /// <summary>
        /// Parameters for the expression, if any.  If none, may be null or empty.
        /// </summary>
        public readonly IEnumerable Parameters;
        /// <summary>
        /// Represents a hand-written custom expression.  May be a chunk of SQL, or may be
        /// something else relevant to the underlying data access layer.
        /// 
        /// This is both a very powerful and very dangerous expression; it allows you
        /// to construct complicated logic, but you can easily pass invalid input (that
        /// doesn't parse), sloppy input (hard coded table / column names), or even
        /// evil input (sql injection).
        /// 
        /// MAKE SURE to sanitize any user input that makes its way into this expression!
        /// </summary>
        /// <param name="expression">The expression text.  Can be almost anything, as long as it means
        ///                          something to the underlying Dao layer.</param>
        public HandWrittenExpression(string expression) : this(expression, null) {}
        /// <summary>
        /// Represents a hand-written custom expression.  May be a chunk of SQL, or may be
        /// something else relevant to the underlying data access layer.
        /// 
        /// This is both a very powerful and very dangerous expression; it allows you
        /// to construct complicated logic, but you can easily pass invalid input (that
        /// doesn't parse), sloppy input (hard coded table / column names), or even
        /// evil input (sql injection).
        /// 
        /// MAKE SURE to sanitize any user input that makes its way into this expression!
        /// </summary>
        /// <param name="expression">The expression text.  Can be almost anything, as long as it means
        ///                          something to the underlying Dao layer.</param>
        /// <param name="parameters">Parameters for the expression, if any.  
        ///                          If none, may be null or empty.</param>
        public HandWrittenExpression(string expression, IEnumerable parameters)
            : base(true)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression", "Cannot have a null expression.");
            }
            Expression = expression;
            Parameters = parameters;
        }

        /// <summary>
        /// Since handwritten expressions are completely custom, there is no automatic
        /// way to invert them.  Therefore this is not supported.
        /// </summary>
        /// <returns>An exception.</returns>
        public override IExpression Invert()
        {
            throw new NotSupportedException("No way to invert a hand-written expression.");
        }

        /// <exclude/>
        public override string ToString()
        {
            return "Expression=" + Expression + ", " + base.ToString();
        }
    }
}