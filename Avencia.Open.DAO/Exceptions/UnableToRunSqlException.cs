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

using System;
using System.Collections;
using Avencia.Open.DAO.SQL;

namespace Avencia.Open.DAO.Exceptions
{
    /// <summary>
    /// This exception is thrown when we are able to connect to a DB, but we get an
    /// exception executing a SQL statement.
    /// </summary>
    public class UnableToRunSqlException : ExceptionWithConnectionInfo
    {
        /// <summary>
        /// Unable to execute SQL.
        /// </summary>
        /// <param name="desc">Connection descriptor we connected with.</param>
        /// <param name="sql">SQL statement we were trying to execute.</param>
        /// <param name="sqlParams">Parameters for the sql statement (may be null).</param>
        /// <param name="e">Exception that was thrown by the database driver.</param>
        public UnableToRunSqlException(ConnectionDescriptor desc, string sql,
                                       IEnumerable sqlParams, Exception e)
            : base("Database exception while trying to execute SQL statement: " +
                   SqlUtilities.SqlParamsToString(sql, sqlParams) + ".", desc, e)
        {
        }

        /// <summary>
        /// Unable to execute SQL.
        /// </summary>
        /// <param name="message">Start of the message, if you don't want to use the default.</param>
        /// <param name="desc">Connection descriptor we connected with.</param>
        /// <param name="sql">SQL statement we were trying to execute.</param>
        /// <param name="sqlParams">Parameters for the sql statement (may be null).</param>
        /// <param name="e">Exception that was thrown by the database driver.</param>
        public UnableToRunSqlException(string message, ConnectionDescriptor desc, string sql,
                                       IEnumerable sqlParams, Exception e)
            : base(message + " " +
                   SqlUtilities.SqlParamsToString(sql, sqlParams) + ".", desc, e)
        {
        }
    }
}