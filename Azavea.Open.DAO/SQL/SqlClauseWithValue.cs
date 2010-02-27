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

namespace Azavea.Open.DAO.SQL
{
    /// <summary>
    /// Many SQL clauses look something like this: "(colName > 5)".  However, on
    /// some DBs, that same clause may actually look like this: "LT_OR_EQ(5, colName)".
    /// To make things more complicated, you may want to not put 5 in the string, but
    /// use a parameter instead.  So this allows you to represent it, in the first case
    /// PartBeforeValue would be "(colName > " and PartAfterValue would be ")", and in
    /// the second case PartBeforeValue would be "LT_OR_EQ(" and PartAfterValue would
    /// be ", colName)".
    /// </summary>
    public class SqlClauseWithValue
    {
        /// <summary>
        /// The part of the clause that comes before the value.  May be null.
        /// </summary>
        public string PartBeforeValue;
        /// <summary>
        /// The part of the clause that comes after the value.  May be null.
        /// </summary>
        public string PartAfterValue;
        /// <summary>
        /// Create a blank clause.
        /// </summary>
        public SqlClauseWithValue()
        {
            // Nothing to do here.
        }

        /// <summary>
        /// Create a clause by specifying the before and after parts.
        /// </summary>
        /// <param name="before">The part of the clause that comes before the value.</param>
        /// <param name="after">The part of the clause that comes after the value.</param>
        public SqlClauseWithValue(string before, string after)
        {
            PartBeforeValue = before;
            PartAfterValue = after;
        }

        /// <summary>
        /// Sets both parts back to null.
        /// </summary>
        public void Clear()
        {
            PartBeforeValue = null;
            PartAfterValue = null;
        }
    }
}