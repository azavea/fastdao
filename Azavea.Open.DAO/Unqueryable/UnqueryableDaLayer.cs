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
    /// A base class for data access layers that have no native query support
    /// and instead have to manually implement querying.
    /// </summary>
    public abstract class UnqueryableDaLayer : AbstractDaLayer
    {
        /// <summary>
        /// Cache used to store the queries for reuse.
        /// </summary>
        protected readonly Common.Caching.ClearingCache<UnqueryableQuery> _queryCache =
            new Common.Caching.ClearingCache<UnqueryableQuery>();

        /// <summary>
        /// Instantiates the data access layer with the connection descriptor for the data source.
        /// </summary>
        /// <param name="connDesc">The connection descriptor that is being used by this FastDaoLayer.</param>
        /// <param name="supportsNumRecords">If true, methods that return numbers of records affected will be
        ///                                 returning accurate numbers.  If false, they will probably return
        ///                                 FastDAO.UNKNOWN_NUM_ROWS.</param>
        protected UnqueryableDaLayer(IConnectionDescriptor connDesc, bool supportsNumRecords)
            : base(connDesc, supportsNumRecords)
        {
        }

        /// <summary>
        /// Builds the query based on a serializable criteria.  The Query object is particular to
        /// the implementation, but may contain things like the parameters parsed out, or whatever
        /// makes sense to this FastDaoLayer.  You can think of this method as a method to convert
        /// from the generic DaoCriteria into the specific details necessary for querying.
        /// </summary>
        /// <param name="crit">The criteria to use to find the desired objects.</param>
        /// <param name="mapping">The mapping of the table for which to build the query string.</param>
        /// <returns>A query that can be run by ExecureQuery.</returns>
        public override IDaQuery CreateQuery(ClassMapping mapping, DaoCriteria crit)
        {
            UnqueryableQuery retVal = _queryCache.Get();
            retVal.Criteria = crit;
            return retVal;
        }

        /// <summary>
        /// Should be called when you're done with the query.  Allows us to cache the
        /// objects for reuse.
        /// </summary>
        /// <param name="query">Query you're done using.</param>
        public override void DisposeOfQuery(IDaQuery query)
        {
            _queryCache.Return((UnqueryableQuery)query);
        }

    }
}
