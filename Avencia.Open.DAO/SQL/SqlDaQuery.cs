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

using System.Collections.Generic;
using System.Text;

namespace Avencia.Open.DAO.SQL
{
    /// <summary>
    /// A "normal" SQL query, can be run by the SqlDaLayer.
    /// </summary>
    public class SqlDaQuery : IDaQuery
    {
        /// <summary>
        /// The SQL statement to run, hopefully parameterized.
        /// </summary>
        public readonly StringBuilder Sql = new StringBuilder();
        /// <summary>
        /// Any parameters for the SQL statement.
        /// </summary>
        public readonly List<object> Params = new List<object>();

        /// <summary>
        /// Clears the contents of the query, allowing the object to be reused.
        /// </summary>
        public virtual void Clear()
        {
            Params.Clear();
            Sql.Remove(0, Sql.Length);
        }

        ///<summary>
        ///Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public override string ToString()
        {
            return SqlUtilities.SqlParamsToString(Sql.ToString(), Params);
        }
    }
}