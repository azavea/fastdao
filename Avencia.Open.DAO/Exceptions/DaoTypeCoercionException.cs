// Copyright (c) 2004-2009 Avencia, Inc.
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