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