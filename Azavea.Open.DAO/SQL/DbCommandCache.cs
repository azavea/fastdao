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

using System.Collections.Generic;
using System.Data;
using System.Text;
using Azavea.Open.Common;
using Azavea.Open.DAO.Util;

namespace Azavea.Open.DAO.SQL
{
    /// <summary>
    /// Holds DbCommands, we save them to allow us to reuse them (saves time).
    /// </summary>
    public class DbCommandCache
    {
        private readonly static Dictionary<string, IDbCommand> _cache =
            new Dictionary<string, IDbCommand>();

        /// <summary>
        /// This returns a command from the cache, or creates a new one if necessary.  
        /// This method is thread-safe.
        /// </summary>
        public IDbCommand Get(string sql, IDbConnection conn)
        {
            StringBuilder keyBuilder = DbCaches.StringBuilders.Get();
            keyBuilder.Append(sql);
            keyBuilder.Append(conn.ConnectionString);
            string key = keyBuilder.ToString();
            // See if we have one in the cache already.
            IDbCommand cmd = null;
            lock (_cache)
            {
                if (_cache.ContainsKey(key))
                {
                    cmd = _cache[key];
                    // Remove it from the cache so we don't hand it to multiple users.
                    _cache.Remove(key);
                }
            }
            if (cmd == null)
            {
                // Make a new one.
                cmd = conn.CreateCommand();
                // Default timeout is kind of short.  There are really two kinds of queries:
                // User-caused queries, where we're responding to something the user did, and
                // system queries.  For user queries, "acceptable" query time is going to be
                // way shorter than the timeout anyway, so if the queries are taking very long
                // it's a problem way before any reasonable timeout would be reached.  System
                // queries are from things like the data processing
                // jobs, where they're dealing with lots of data and being run behind the scenes,
                // so the important thing is just that we DO timeout eventually to prevent a job
                // from hanging.  So a timeout of several minutes is fine.  The CommandTimeout
                // attribute is in seconds.
                cmd.CommandTimeout = 300;
            }
            else
            {
                cmd.Connection = conn;
            }
            Chronometer.BeginTiming(key);
            DbCaches.StringBuilders.Return(keyBuilder);
            return cmd;
        }

        /// <summary>
        /// Returns a command to the cache when it is no longer used, allowing another call
        /// to reuse it.
        /// 
        /// This method is thread-safe.
        /// </summary>
        public void Return(string sql, IDbConnection conn, IDbCommand cmd)
        {
            StringBuilder keyBuilder = DbCaches.StringBuilders.Get();
            keyBuilder.Append(sql);
            keyBuilder.Append(conn.ConnectionString);
            string key = keyBuilder.ToString();

            // Save the time for reporting purposes.
            Chronometer.EndTiming(key);

            lock (_cache)
            {
                if (!_cache.ContainsKey(key))
                {
                    // Make sure we don't keep some random set of values for the params.
                    cmd.Parameters.Clear();
                    // It may have been used on a transaction, so clear that out.
                    cmd.Transaction = null;
                    // No command for this key, go ahead and cache it.
                    _cache[key] = cmd;
                } // else just discard this one, our cache is tolerant of sloppy usage.
            }
            DbCaches.StringBuilders.Return(keyBuilder);
        }
    }
}