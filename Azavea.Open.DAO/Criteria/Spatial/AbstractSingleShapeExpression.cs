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
using GeoAPI.Geometries;

namespace Azavea.Open.DAO.Criteria.Spatial
{
    /// <summary>
    /// Base class for expressions that have a single geometry criteria.
    /// </summary>
    public abstract class AbstractSingleShapeExpression : AbstractSinglePropertyExpression
    {
        /// <summary>
        /// This is what you want records' shapes to relate to.
        /// </summary>
        public readonly IGeometry Shape;

        /// <summary>
        /// Create an expression on a single property relating to a single shape.
        /// </summary>
        /// <param name="property">The data class' property/field being compared.
        ///                        May not be null.</param>
        /// <param name="shape">This is what you want records' shapes to relate to.</param>
        /// <param name="trueOrNot">True means look for matches (I.E. does relate),
        ///                         False means look for non-matches (I.E. does not relate)</param>
        protected AbstractSingleShapeExpression(string property, IGeometry shape, bool trueOrNot)
            : base(property, trueOrNot)
        {
            if (shape == null)
            {
                throw new ArgumentNullException("shape",
                    "Cannot check for records that interact with a null shape.");
            }
            Shape = shape;
        }
        /// <exclude/>
        public override string ToString()
        {
            return "Shape=" + Shape + ", " + base.ToString();
        }
    }
}
