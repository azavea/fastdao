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

using System.Collections.Generic;

namespace Azavea.Open.DAO
{
    /// <summary>
    /// This interface defines the "update" methods of a FastDAO.
    /// </summary>
    /// <typeparam name="T">The type of object that can be written.</typeparam>
    public interface IFastDaoUpdater<T> : IFastDaoBase<T> where T : class, new()
    {
        /// <summary>
        /// Updates this object's record in the data source.
        /// </summary>
        /// <param name="obj">The object to save.</param>
        void Update(T obj);

        /// <summary>
        /// Updates a bunch of records in one transaction, hopefully faster than
        /// separate calls to Update().  Whether it is actually faster depends on
        /// the implementation.
        /// </summary>
        /// <param name="updateUs">List of objects to save.</param>
        void Update(IEnumerable<T> updateUs);
    }
}