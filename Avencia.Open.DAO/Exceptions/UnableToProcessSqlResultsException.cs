using System;
using System.Collections;
using Avencia.Open.DAO.SQL;

namespace Avencia.Open.DAO.Exceptions
{
    /// <summary>
    /// This exception is thrown when something went wrong while dealing with
    /// what we got back from the database query, not with the database query itself.
    /// </summary>
    public class UnableToProcessSqlResultsException : ExceptionWithConnectionInfo
    {
        /// <summary>
        /// Error while processing SQL results.
        /// </summary>
        /// <param name="desc">Connection descriptor we connected with.</param>
        /// <param name="sql">SQL statement we were executed.</param>
        /// <param name="sqlParams">Parameters for the sql statement (may be null).</param>
        /// <param name="e">Exception that was thrown by the delegate.</param>
        public UnableToProcessSqlResultsException(ConnectionDescriptor desc, string sql,
                                                  IEnumerable sqlParams, Exception e)
            : base("Exception while processing results from SQL statement: " +
                   SqlUtilities.SqlParamsToString(sql, sqlParams) + ".", desc, e)
        {
        }

        /// <summary>
        /// Error while processing SQL results.
        /// </summary>
        /// <param name="message">Start of the message, if you don't want to use the default.</param>
        /// <param name="desc">Connection descriptor we connected with.</param>
        /// <param name="sql">SQL statement we executed.</param>
        /// <param name="sqlParams">Parameters for the sql statement (may be null).</param>
        /// <param name="e">Exception that was thrown by the delegate.</param>
        public UnableToProcessSqlResultsException(string message, ConnectionDescriptor desc, string sql,
                                                  IEnumerable sqlParams, Exception e)
            : base(message + " " +
                   SqlUtilities.SqlParamsToString(sql, sqlParams) + ".", desc, e)
        {
        }
    }
}