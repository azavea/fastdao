using System;

namespace Avencia.Open.DAO.Exceptions
{
    /// <summary>
    /// This exception is thrown when the connection you have is wrong for what
    /// you're trying to do.
    /// </summary>
    public class BadConnectionTypeException : ExceptionWithConnectionInfo
    {
        /// <summary>
        /// This connection can't do that.
        /// </summary>
        /// <param name="desc">Connection descriptor we were using to try to connect.</param>
        /// <param name="message">A message describing what is wrong with the connection type.</param>
        public BadConnectionTypeException(ConnectionDescriptor desc, string message)
            : base(message, desc, null)
        {
            // Nothing to set here.
        }
    }
}