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

namespace Avencia.Open.DAO
{
    /// <summary>
    /// This interface defines the most basic methods on a FastDAO, regardless of which
    /// interfaces it implements.
    /// </summary>
    /// <typeparam name="T">The type of object that can be deleted.</typeparam>
// ReSharper disable UnusedTypeParameter
    public interface IFastDaoBase<T> where T : class, new()
// ReSharper restore UnusedTypeParameter
    {
        /// <summary>
        /// The ClassMapping object representing the class-to-record mapping 
        /// to use with the data source.
        /// </summary>
        ClassMapping ClassMap { get; }

        /// <summary>
        /// The object describing how to connect to and/or interact with the data
        /// source we're reading objects from.
        /// </summary>
        ConnectionDescriptor ConnDesc { get; }
    }
}