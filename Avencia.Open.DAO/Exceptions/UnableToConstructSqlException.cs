using System;

namespace Avencia.Open.DAO.Exceptions
{
    /// <summary>
    /// This exception is thrown when we are unable to construct a SQL statement,
    /// possibly due to bad values, expression types that are not supported, etc.
    /// </summary>
    public class UnableToConstructSqlException : ExceptionWithConnectionInfo
    {
        /// <summary>
        /// Unable to construct a SQL statement.
        /// </summary>
        /// <param name="message">Start of the message, if you don't want to use the default.</param>
        /// <param name="desc">Connection descriptor we would have connected with.</param>
        /// <param name="e">Exception that was thrown by the database driver.</param>
        public UnableToConstructSqlException(string message, ConnectionDescriptor desc, Exception e)
            : base(message, desc, e) { }
    }
}