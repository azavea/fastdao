// Copyright (c) 2004-2010 Avencia, Inc.
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

namespace Avencia.Open.DAO.Exceptions
{
    /// <summary>
    /// This exception is thrown when we are unable to connect to a database.
    /// </summary>
    public class UnableToConnectException : ExceptionWithConnectionInfo
    {
        /// <summary>
        /// Unable to establish a database connection.
        /// </summary>
        /// <param name="desc">Connection descriptor we were using to try to connect.</param>
        /// <param name="numTimes">How many times in a row have we failed to connect, if
        ///                        known.  This is used in the message only if it is greater
        ///                        than 1.</param>
        /// <param name="e">Exception that was thrown by the database driver.</param>
        public UnableToConnectException(ConnectionDescriptor desc, int numTimes, Exception e)
            : base("Unable to connect to database" +
                   ((numTimes > 1) ? (" (" + numTimes + " time(s) in a row).") : "."), desc, e)
        {
        }
    }
}