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