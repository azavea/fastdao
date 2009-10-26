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

namespace Avencia.Open.DAO.Criteria
{
    /// <summary>
    /// On a sort parameter, indicates what order that field should be sorted in.
    /// </summary>
    [Serializable]
    public enum SortType
    {
        /// <summary>
        /// Ascending, numeric (1 - 999) or alphabetic (A - Z) or CompareTo,
        /// depending on the data type and the class using the DaoCriteria.
        /// </summary>
        Asc,
        /// <summary>
        /// Descending, numeric (999 - 1) or alphabetic (Z - A) or CompareTo,
        /// depending on the data type and the class using the DaoCriteria.
        /// </summary>
        Desc,
        /// <summary>
        /// Indicates that this is a computed parameter (such as "Field1 + field2") and
        /// it should be sorted in whatever the natural order is (up to the class using
        /// this DaoCriteria to determine).
        /// </summary>
        Computed
    }
}