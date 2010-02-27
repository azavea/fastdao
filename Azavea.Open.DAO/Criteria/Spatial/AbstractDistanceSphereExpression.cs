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
    /// Base class for comparing curved distance along a spherical surface
    /// between the given shape and the feature, against a cutoff.
    /// </summary>
    public abstract class AbstractDistanceSphereExpression : AbstractSingleShapeExpression
    {
        /// <summary>
        /// The distance we're comparing against.
        /// </summary>
        public readonly double Distance;
        /// <summary>
        /// Base class for comparing curved distance along a spherical surface
        /// between the given shape and the feature, against a cutoff.
        /// </summary>
        /// <param name="property">The data class' property/field being compared.
        ///                        May not be null.</param>
        /// <param name="shape">The shape we're measuring distance from.</param>
        /// <param name="distance">The distance we're comparing against.</param>
        /// <param name="trueOrNot">True means look for matches,
        ///                         False means look for non-matches</param>
        protected AbstractDistanceSphereExpression(string property, IGeometry shape, double distance, bool trueOrNot)
            : base(property, shape, trueOrNot)
        {
            if (distance < 0)
            {
                throw new ArgumentException(
                    "Distance between two items is never negative, you passed: " +
                    distance, "distance");
            }
            Distance = distance;
        }
        /// <exclude/>
        public override string ToString()
        {
            return "Distance=" + Distance + ", " + base.ToString();
        }
    }
}
