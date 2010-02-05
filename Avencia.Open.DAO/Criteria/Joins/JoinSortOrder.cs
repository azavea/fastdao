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

namespace Avencia.Open.DAO.Criteria.Joins
{
    /// <summary>
    /// This is similar to a DaoCriteria's SortOrder, except it is necessary
    /// to specify which DAO, the left or the right, has this field we're sorting on.
    /// </summary>
    public class JoinSortOrder : SortOrder
    {
        /// <summary>
        /// If true, the property we're sorting on comes from the left
        /// DAO.  If false, it comes from the right DAO.
        /// </summary>
        public readonly bool IsForLeftDao;

        /// <summary>
        /// A simple class that holds a sort criterion for a property from the right or left DAO.
        /// This constructor creates an "ascending" sort order.
        /// </summary>
        /// <param name="property">The data class' property to sort on.</param>
        /// <param name="isForLeftDao">If true, the property we're sorting on comes from the left
        ///                            DAO.  If false, it comes from the right DAO.</param>
        public JoinSortOrder(string property, bool isForLeftDao)
            : this(property, SortType.Asc, isForLeftDao) { }
        /// <summary>
        /// A simple class that holds a sort criterion for a property from the right or left DAO.
        /// </summary>
        /// <param name="property">The data class' property to sort on.</param>
        /// <param name="direction">The direction to sort based on the Property.</param>
        /// <param name="isForLeftDao">If true, the property we're sorting on comes from the left
        ///                            DAO.  If false, it comes from the right DAO.</param>
        public JoinSortOrder(string property, SortType direction, bool isForLeftDao)
            : base(property, direction)
        {
            IsForLeftDao = isForLeftDao;
        }

        /// <summary>
        /// For some implementations, it's simpler to only implement one-sided joins.  So
        /// to handle right (or left) joins, you want to flip the criteria so the right
        /// and left daos are swapped and you can do a left (or right) join instead.
        /// </summary>
        /// <returns>A copy of this sort order with the left/right orientation swapped.</returns>
        public JoinSortOrder Flip()
        {
            return new JoinSortOrder(Property, Direction, !IsForLeftDao);
        }

        ///<summary>
        ///
        ///                    Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        ///                
        ///</summary>
        ///
        ///<returns>
        ///
        ///                    A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        ///                
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public override string ToString()
        {
            return (IsForLeftDao ? "LEFT." : "RIGHT.") + base.ToString();
        }
    }
}