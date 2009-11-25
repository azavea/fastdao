using System;
using Avencia.Open.Common;

namespace Avencia.Open.DAO.Exceptions
{
    /// <summary>
    /// This is the type of exception thrown by CoerceType.
    /// </summary>
    public class DaoTypeCoercionException : LoggingException
    {
        /// <summary>
        /// Type we were trying to coerce the value to.
        /// </summary>
        public readonly Type DesiredType;
        /// <summary>
        /// The value that was trying to be coerced.
        /// </summary>
        public readonly object Value;

        /// <summary>
        /// Creates a coercion exception.
        /// </summary>
        /// <param name="desiredType">Type we were trying to coerce the value to.</param>
        /// <param name="value">The value that was trying to be coerced.</param>
        /// <param name="innerException">Exception that was thrown (if any, may be null).</param>
        public DaoTypeCoercionException(Type desiredType, object value, Exception innerException)
            : this(desiredType, value, "an exception occurred.", innerException) { }
        /// <summary>
        /// Creates a coercion exception.
        /// </summary>
        /// <param name="desiredType">Type we were trying to coerce the value to.</param>
        /// <param name="value">The value that was trying to be coerced.</param>
        /// <param name="reason">Why it did not work.</param>
        /// <param name="innerException">Exception that was thrown (if any, may be null).</param>
        public DaoTypeCoercionException(Type desiredType, object value, string reason, Exception innerException)
            : base("Cannot coerce input '" + (value ?? "<null>") + "' to type '" +
                   desiredType + "' because " + reason, innerException)
        {
            DesiredType = desiredType;
            Value = value;
        }
    }
}