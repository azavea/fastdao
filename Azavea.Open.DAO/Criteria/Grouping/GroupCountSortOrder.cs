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

namespace Azavea.Open.DAO.Criteria.Grouping
{
    /// <summary>
    /// This can be used to indicate you want to sort based on the count
    /// in a count where you're aggregating values.
    /// </summary>
    public class GroupCountSortOrder : SortOrder
    {
        /// <summary>
        /// Default sort order is ascending.
        /// </summary>
        public GroupCountSortOrder()
            : base("Count")
        {
        }

        /// <summary>
        /// Lets you specify the sort direction.
        /// </summary>
        /// <param name="direction">Do you want counts going up or down?</param>
        public GroupCountSortOrder(SortType direction)
            : base("Count", direction)
        {
        }
    }
}
