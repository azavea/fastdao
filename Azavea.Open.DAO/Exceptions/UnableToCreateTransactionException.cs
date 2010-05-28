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
    /// This exception is thrown when we are unable to begin a transaction.
    /// </summary>
    public class UnableToCreateTransactionException : ExceptionWithConnectionInfo
    {
        /// <summary>
        /// Unable to begin a transaction against the database.
        /// </summary>
        /// <param name="desc">Connection descriptor we were using to try to connect.</param>
        /// <param name="e">Exception that was thrown by the database driver.</param>
        public UnableToCreateTransactionException(IConnectionDescriptor desc, Exception e)
            : base("Unable to begin a transaction.", desc, e)
        {
        }
    }
}