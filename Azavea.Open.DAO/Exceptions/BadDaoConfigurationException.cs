// Copyright (c) 2004-2010 Azavea, Inc.
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
using Azavea.Open.Common;

namespace Azavea.Open.DAO.Exceptions
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