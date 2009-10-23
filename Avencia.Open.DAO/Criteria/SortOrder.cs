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
    /// A simple class that holds a single sort criterion.
    /// </summary>
    [Serializable]
    public class SortOrder
    {
        /// <summary>
        /// The data class' property to sort on.
        /// </summary>
        public readonly string Property;
        /// <summary>
        /// The direction to sort based on the Property.
        /// </summary>
        public readonly SortType Direction;

        /// <summary>
        /// A simple class that holds a single sort criterion.
        /// This constructor constructs one with an "ascending" sort.
        /// </summary>
        /// <param name="property">The data class' property to sort on.</param>
        public SortOrder(string property) : this(property, SortType.Asc) { }

        /// <summary>
        /// A simple class that holds a single sort criterion.
        /// </summary>
        /// <param name="property">The data class' property to sort on.</param>
        /// <param name="direction">The direction to sort based on the Property.</param>
        public SortOrder(string property, SortType direction)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property",
                    "Property must be a valid property/field name from the data class.");
            }
            Property = property;
            Direction = direction;
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
            return Property + " " + Direction;
        }
    }
}