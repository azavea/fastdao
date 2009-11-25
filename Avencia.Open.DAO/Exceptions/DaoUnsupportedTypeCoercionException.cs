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