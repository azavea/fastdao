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

namespace Avencia.Open.DAO.SQL
{
    /// <summary>
    /// A SQL query that joins two tables, can be run by the SqlDaLayer.
    /// </summary>
    public class SqlDaJoinQuery : SqlDaQuery, IDaJoinQuery
    {
        private string _leftPrefix;
        private string _rightPrefix;

        /// <summary>
        /// Populates the prefix strings.
        /// </summary>
        /// <param name="left">Prefix for columns from the left table.</param>
        /// <param name="right">Prefix for columns from the right table.</param>
        public void SetPrefixes(string left, string right)
        {
            _leftPrefix = left;
            _rightPrefix = right;
        }

        /// <summary>
        /// The prefix that should be used to get the left table's columns out of the IDataReader
        /// when accessing them by name.
        /// </summary>
        /// <returns>The prefix for columns in the left table (I.E. "left_table.")</returns>
        public string GetLeftColumnPrefix()
        {
            return _leftPrefix;
        }

        /// <summary>
        /// The prefix that should be used to get the right table's columns out of the IDataReader
        /// when accessing them by name.
        /// </summary>
        /// <returns>The prefix for columns in the right table (I.E. "right_table.")</returns>
        public string GetRightColumnPrefix()
        {
            return _rightPrefix;
        }
    }
}