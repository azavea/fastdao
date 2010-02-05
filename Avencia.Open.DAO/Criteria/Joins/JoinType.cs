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
    /// This enumeration defines the possible types of join.
    /// </summary>
    public enum JoinType
    {
        /// <summary>
        /// Inner join, only include when records exist in both the left DAO and the right DAO.
        /// </summary>
        Inner,
        /// <summary>
        /// Left outer join, include when records exist in the left DAO, even if they
        /// don't exist in the right DAO.
        /// </summary>
        LeftOuter,
        /// <summary>
        /// Right outer join, include when records exist in the right DAO, even if they
        /// don't exist in the left DAO.
        /// </summary>
        RightOuter,
        /// <summary>
        /// Outer join, include when records exist in either the left or right DAO, even
        /// if they don't exist in the other DAO.
        /// </summary>
        Outer,
        //// <summary>
        //// TODO: Not sure how to support this... the idea is it's not just a join but
        //// something like "select from a where a.field = (select max(bfield) from b)"
        //// </summary>
        //NestedSelect
    }
}