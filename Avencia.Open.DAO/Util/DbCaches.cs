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

using System.Collections;
using System.Collections.Generic;
using Avencia.Open.Common.Caching;
using Avencia.Open.DAO.Criteria;
using Avencia.Open.DAO.SQL;

namespace Avencia.Open.DAO.Util
{
    /// <summary>
    /// We need to cache so many things in so many places, it made more sense to just put them
    /// all in one nice central location.
    /// </summary>
    public static class DbCaches
    {
        // Many times we call Get to get things from the cache that we then do not return.
        // That's OK, these caches are ok with leaking objects.  Plus, some of the returned
        // objects wind up getting put back in the cache by another class
        // in this assembly.
        /// <summary>
        /// Cache of Hashtables.
        /// </summary>
        public static readonly ClearingCache<Hashtable> Hashtables =
            new ClearingCache<Hashtable>();
        /// <summary>
        /// Cache of ArrayLists.
        /// </summary>
        public static readonly ClearingCache<ArrayList> ArrayLists =
            new ClearingCache<ArrayList>();
        /// <summary>
        /// Cache of List&lt;int&gt;s.
        /// </summary>
        public static readonly ClearingCache<List<int>> IntLists =
            new ClearingCache<List<int>>();
        /// <summary>
        /// Cache of List&lt;string&gt;s.
        /// </summary>
        public static readonly ClearingCache<List<string>> StringLists =
            new ClearingCache<List<string>>();
        /// <summary>
        /// Cache of List&lt;IEnumerable&gt;s.
        /// </summary>
        public static readonly ClearingCache<List<IEnumerable>> EnumerableLists =
            new ClearingCache<List<IEnumerable>>();
        /// <summary>
        /// Cache of List&lt;object&gt;s.
        /// </summary>
        public static readonly ClearingCache<List<object>> ObjectLists =
            new ClearingCache<List<object>>();
        /// <summary>
        /// Cache of Dictionary&lt;string, object&gt;s.
        /// </summary>
        public static readonly ClearingCache<Dictionary<string, object>> StringObjectDicts =
            new ClearingCache<Dictionary<string, object>>();
        /// <summary>
        /// Cache of Dictionary&lt;string, int&gt;s.
        /// </summary>
        public static readonly ClearingCache<Dictionary<string, int>> StringIntDicts =
            new ClearingCache<Dictionary<string, int>>();
        /// <summary>
        /// Cache of StringBuilders.
        /// </summary>
        public readonly static StringBuilderCache StringBuilders = new StringBuilderCache();
        /// <summary>
        /// Cache of connections to all the databases we've connected to.
        /// </summary>
        public readonly static DbConnectionCache Connections = new DbConnectionCache();
        /// <summary>
        /// Cache of all the commands we've run.
        /// </summary>
        public readonly static DbCommandCache Commands = new DbCommandCache();
        /// <summary>
        /// Cache of "param" + nums.
        /// </summary>
        public readonly static ParamNameCache ParamNames = new ParamNameCache();
        /// <summary>
        /// A cache of DaoCriterias so we aren't constructing zillions of 'em.
        /// </summary>
        public readonly static ClearingCache<DaoCriteria> Criteria =
            new ClearingCache<DaoCriteria>();
        /// <summary>
        /// Cache of SQL clauses so we can reuse them.
        /// </summary>
        public static readonly ClearingCache<SqlClauseWithValue> Clauses =
            new ClearingCache<SqlClauseWithValue>();
    }
}