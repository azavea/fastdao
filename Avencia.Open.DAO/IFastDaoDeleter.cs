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
using Avencia.Open.DAO.Criteria;

namespace Avencia.Open.DAO
{
    /// <summary>
    /// This interface defines the "delete" methods of FastDAO.
    /// </summary>
    /// <typeparam name="T">The type of object that can be deleted.</typeparam>
    public interface IFastDaoDeleter<T> where T : class, new()
    {
        /// <summary>
        /// Deletes the specified object from the data source.  No error is generated
        /// if the object does not appear to be stored in the data source.
        /// </summary>
        /// <param name="dataObject">An object to delete from the data source.</param>
        void Delete(T dataObject);

        /// <summary>
        /// Deletes the specified objects from the data source.  If the objects are not in the
        /// database, no error is generated.
        /// </summary>
        void Delete(IEnumerable<T> deleteUs);

        /// <summary>
        /// Deletes objects from the data source that meet the given criteria.
        /// </summary>
        /// <param name="crit">Criteria for deletion.  NOTE: Only the expressions are observed,
        ///                    other things (like "order" or start / limit) are ignored.
        ///                    Also, null or blank (no expressions) criteria are NOT allowed.
        ///                    If you really wish to delete all rows, call DeleteAll().</param>
        /// <returns>The number of rows/objects deleted (or UNKNOWN_NUM_ROWS).</returns>
        int Delete(DaoCriteria crit);

        /// <summary>
        /// Deletes all records of this dao's type.
        /// </summary>
        /// <returns>The number of rows/objects deleted.</returns>
        int DeleteAll();

        /// <summary>
        /// Deletes every row from the data source for this DAO.  
        /// Can be faster than DeleteAll for certain types of data source
        /// and/or certain conditions, but requires greater permissions.
        /// If the data source does not support truncation, this will be
        /// exactly the same as calling DeleteAll().
        /// </summary>
        void Truncate();
    }
}