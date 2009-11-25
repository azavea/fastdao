using System;
using Avencia.Open.Common;

namespace Avencia.Open.DAO.Exceptions
{
    /// <summary>
    /// This exception type is constructed with the database connection string
    /// as part of the message.
    /// </summary>
    public class ExceptionWithConnectionInfo : LoggingException
    {
        /// <summary>
        /// An error occurred doing something with a database.
        /// </summary>
        /// <param name="message">What happened.</param>
        /// <param name="desc">The connection info will be appended to the message.</param>
        /// <param name="e">Inner exception that caused this one to be thrown.</param>
        public ExceptionWithConnectionInfo(string message, ConnectionDescriptor desc,
                                           Exception e)
            : base(message + "  Connection info: " + desc, e)
        {
        }
    }
}