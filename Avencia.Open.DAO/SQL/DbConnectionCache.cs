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

using System;
using System.Collections.Generic;
using System.Data;
using Avencia.Open.Common.Caching;
using Avencia.Open.DAO.Exceptions;
using log4net;

namespace Avencia.Open.DAO.SQL
{
    /// <summary>
    /// Holds connections, using the connection descriptor to determine what
    /// request wants what connection.
    /// </summary>
    public class DbConnectionCache
    {
        private readonly static ILog _log = LogManager.GetLogger(
            new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().DeclaringType.Namespace);

        private readonly Dictionary<ConnectionDescriptor, TimestampedData<IDbConnection>> _cache =
            new Dictionary<ConnectionDescriptor, TimestampedData<IDbConnection>>();

        private readonly Dictionary<ConnectionDescriptor, int> _errorCount =
            new Dictionary<ConnectionDescriptor, int>();

        /// <summary>
        /// This returns a connection from the cache, or creates a new one if necessary. 
        /// 
        /// This method is thread safe.
        /// 
        /// This method improved one test's performance (the only one I measured) by about 20%.
        /// 
        /// Since some databases cannot have more than one connection at a time, we ask the
        /// connection descriptor whether we can actually cache the connection or not.
        /// </summary>
        public IDbConnection Get(AbstractSqlConnectionDescriptor connDesc)
        {
            IDbConnection conn = null;
            if (connDesc.UsePooling())
            {
                // If it is a pooled connection, check if we have one in our cache.
                lock (_cache)
                {
                    if (_cache.ContainsKey(connDesc))
                    {
                        TimestampedData<IDbConnection> connFromCache = _cache[connDesc];
                        // This pool only caches one connection for a connection string.
                        _cache.Remove(connDesc);

                        // Don't reuse a connection that was cached more than 5 minutes ago.  We use
                        // this cache to handle the case of thousands per second, if we're querying
                        // every 5 minutes we can take the time to create a new connection.
                        if (connFromCache.Time.Ticks < (DateTime.Now.Ticks - 3000000000))
                        {
                            try
                            {
                                connFromCache.Data.Close();
                            }
                            catch (Exception e)
                            {
                                _log.Debug("Exception while closing a connection that was probably expired anyway: " +
                                           connDesc, e);
                            }
                        }
                        else
                        {
                            conn = connFromCache.Data;
                        }
                    }
                }
            }
            // If it's a bad/broken/something connection, we want a new one also.
            if ((conn == null) || (conn.State != ConnectionState.Open))
            {
                // Make a new one and put it in the cache.
                conn = connDesc.CreateNewConnection();
                try
                {
                    conn.Open();
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug("Opened connection to: " + connDesc);
                    }
                    lock (_errorCount)
                    {
                        _errorCount[connDesc] = 0;
                    }
                }
                catch (Exception e)
                {
                    // Increment the number of consecutive failures.
                    int newCount = 1;
                    lock (_errorCount)
                    {
                        if (_errorCount.ContainsKey(connDesc))
                        {
                            newCount = _errorCount[connDesc] + 1;
                        }
                        _errorCount[connDesc] = newCount;
                    }
                    throw new UnableToConnectException(connDesc, newCount, e);
                }
            }
            return conn;
        }

        /// <summary>
        /// Inserts the connection into the cache IF we don't have one already (we only cache one
        /// connection per unique descriptor).
        /// 
        /// This method is thread safe.
        /// 
        /// Due to MS Access not allowing more than one connection to have the DB locked for modifying at
        /// one time, any connections to Access DBs are closed and not cached.
        /// </summary>
        public void Return(AbstractSqlConnectionDescriptor connDesc, IDbConnection conn)
        {
            bool closeConn = false;
            if (!connDesc.UsePooling())
            {
                closeConn = true;
            }
            else
            {
                lock (_cache)
                {
                    if (_cache.ContainsKey(connDesc))
                    {
                        // already have a connection for this connstr.  We're tolerant of this, just
                        // go ahead and close the second connection.
                        closeConn = true;
                    }
                    else
                    {
                        _cache[connDesc] = new TimestampedData<IDbConnection>(conn);
                    }
                }
            }
            if (closeConn)
            {
                // Not caching it, just close it.
                try
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug("Closing DB connection: " + conn.ConnectionString);
                    }
                    conn.Close();
                }
                catch (Exception e)
                {
                    // This exception is not rethrown because we don't want to mask any
                    // previous exception that might be being thrown out of "invokeMe".
                    _log.Warn("Caught an exception while trying to close a DB connection we just used.", e);
                }
            }
        }
    }
}