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

using Avencia.Open.DAO.Criteria.Joins;

namespace Avencia.Open.DAO
{
    /// <summary>
    /// If the layer supports joins natively (such as SQL statements in the database)
    /// it will implement this interface.
    /// </summary>
    public interface IDaJoinableLayer
    {
        /// <summary>
        /// This returns whether or not this layer can perform the requested
        /// join natively.  This lets a layer that can have native joins determine
        /// whether this particular join is able to be done natively.
        /// </summary>
        /// <typeparam name="R">The type of object returned by the other DAO.</typeparam>
        /// <param name="crit">The criteria specifying the requested join.</param>
        /// <param name="rightConn">The connection info for the other DAO we're joining with.</param>
        /// <param name="rightMapping">Class mapping for the right table we would be querying against.</param>
        /// <returns>True if we can perform the join natively, false if we cannot.</returns>
        bool CanJoin<R>(DaoJoinCriteria crit, ConnectionDescriptor rightConn, ClassMapping rightMapping) where R : new();

        /// <summary>
        /// This is not guaranteed to succeed unless CanJoin(crit, rightDao) returns true.
        /// </summary>
        /// <param name="crit">The criteria specifying the requested join.</param>
        /// <param name="leftMapping">Class mapping for the left table we're querying against.</param>
        /// <param name="rightMapping">Class mapping for the right table we're querying against.</param>
        IDaJoinQuery CreateJoinQuery(DaoJoinCriteria crit, ClassMapping leftMapping,
                                          ClassMapping rightMapping);

        /// <summary>
        /// This performs a count instead of an actual query.  Depending on the data access layer
        /// implementation, this may or may not be significantly faster than actually executing
        /// the normal query and seeing how many results you get back.  Generally it should be
        /// faster.
        /// </summary>
        /// <param name="crit">The criteria specifying the requested join.</param>
        /// <param name="leftMapping">Class mapping for the left table we're querying against.</param>
        /// <param name="rightMapping">Class mapping for the right table we're querying against.</param>
        /// <returns>The number of results that you would get if you ran the actual query.</returns>
        int GetCount(DaoJoinCriteria crit, ClassMapping leftMapping, ClassMapping rightMapping);
    }
}