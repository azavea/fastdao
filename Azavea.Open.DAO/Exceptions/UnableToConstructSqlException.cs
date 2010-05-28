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

namespace Azavea.Open.DAO.Exceptions
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
        public UnableToConstructSqlException(string message, IConnectionDescriptor desc, Exception e)
            : base(message, desc, e) { }
    }
}