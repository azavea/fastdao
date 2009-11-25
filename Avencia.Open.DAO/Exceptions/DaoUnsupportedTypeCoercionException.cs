using System;

namespace Avencia.Open.DAO.Exceptions
{
    /// <summary>
    /// This is a specialized subtype of the coerce exception, thrown when you attempt to convert
    /// a value to an unsupported type.
    /// </summary>
    public class DaoUnsupportedTypeCoercionException : DaoTypeCoercionException
    {
        /// <summary>
        /// Creates a coercion exception for when the type being coerced to is
        /// not supported.
        /// </summary>
        /// <param name="desiredType">Type we were trying to coerce the value to.</param>
        /// <param name="value">The value that was trying to be coerced.</param>
        public DaoUnsupportedTypeCoercionException(Type desiredType, object value)
            : base(desiredType, value, "that type is not yet supported.", null) { }
    }
}