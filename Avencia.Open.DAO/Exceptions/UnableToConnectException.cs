using System;

namespace Avencia.Open.DAO.Exceptions
{
    /// <summary>
    /// This exception is thrown when we are unable to connect to a database.
    /// </summary>
    public class UnableToConnectException : ExceptionWithConnectionInfo
    {
        /// <summary>
        /// Unable to establish a database connection.
        /// </summary>
        /// <param name="desc">Connection descriptor we were using to try to connect.</param>
        /// <param name="numTimes">How many times in a row have we failed to connect, if
        ///                        known.  This is used in the message only if it is greater
        ///                        than 1.</param>
        /// <param name="e">Exception that was thrown by the database driver.</param>
        public UnableToConnectException(ConnectionDescriptor desc, int numTimes, Exception e)
            : base("Unable to connect to database" +
                   ((numTimes > 1) ? (" (" + numTimes + " time(s) in a row).") : "."), desc, e)
        {
        }
    }
}