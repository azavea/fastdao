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

using Azavea.Open.DAO.Criteria;

namespace Azavea.Open.DAO.Unqueryable
{
    /// <summary>
    /// A query intended to be run against a data source that does not
    /// have any native query mechanism.  Therefore this class merely
    /// stores the criteria used for the query, to be later used when
    /// manually processing the contents of the data source.
    /// </summary>
    public class UnqueryableQuery : IDaQuery
    {
        /// <summary>
        /// Since the queries have to be evaluated at read time (there is no queryability
        /// in the data source itself, we have to merely read it and discard rows that don't match)
        /// there is nothing to be done to "pre-process" the criteria, we just save it
        /// for the reader to use.
        /// </summary>
        public DaoCriteria Criteria;

        /// <summary>
        /// Clears the contents of the query, allowing the object to be reused.
        /// </summary>
        public void Clear()
        {
            Criteria = null;
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
            return Criteria.ToString();
        }
    }
}