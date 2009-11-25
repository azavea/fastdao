using System;
using Avencia.Open.Common;

namespace Avencia.Open.DAO.Exceptions
{
    /// <summary>
    /// This is the type of exception thrown by DAO methods when unable to read
    /// their config (or the values in it make no sense).
    /// </summary>
    public class BadDaoConfigurationException : LoggingException
    {
        /// <summary>
        /// Creates an exception indicating the configuration was no good.
        /// </summary>
        /// <param name="message">What was wrong with the configuration.</param>
        public BadDaoConfigurationException(string message)
            : base(message, null) { }
        /// <summary>
        /// Creates an exception indicating the configuration was no good.
        /// </summary>
        /// <param name="message">What was wrong with the configuration.</param>
        /// <param name="innerException">Exception that was thrown (if any, may be null).</param>
        public BadDaoConfigurationException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}