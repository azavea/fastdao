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

using GeoAPI.Geometries;

namespace Azavea.Open.DAO.Criteria.Spatial
{
    /// <summary>
    /// Looks for features less than the given distance from the given shape,
    /// on a spherical surface.
    /// </summary>
    public class LesserDistanceSphereExpression : AbstractDistanceSphereExpression
    {
        /// <summary>
        /// Looks for features less than the given distance from the given shape,
        /// on a spherical surface.
        /// </summary>
        /// <param name="property">The data class' property/field being compared.
        ///                        May not be null.</param>
        /// <param name="shape">The shape we're measuring distance from.</param>
        /// <param name="distance">The distance must be less than this.</param>
        public LesserDistanceSphereExpression(string property, IGeometry shape, double distance)
            : this(property, shape, distance, true) { }

        /// <summary>
        /// Looks for features less than the given distance from the given shape,
        /// on a spherical surface.
        /// </summary>
        /// <param name="property">The data class' property/field being compared.
        ///                        May not be null.</param>
        /// <param name="shape">The shape we're measuring distance from.</param>
        /// <param name="distance">The distance must be less than this.</param>
        /// <param name="trueOrNot">True means look for matches (I.E. &lt; distance),
        ///                         False means look for non-matches (I.E. &gt;= distance)</param>
        public LesserDistanceSphereExpression(string property, IGeometry shape, double distance, bool trueOrNot)
            : base(property, shape, distance, trueOrNot) { }

        /// <summary>
        /// Produces an expression that is the exact opposite of this expression.
        /// The new expression should exclude everything this one includes, and
        /// include everything this one excludes.
        /// </summary>
        /// <returns>The inverse of this expression.</returns>
        public override IExpression Invert()
        {
            return new LesserDistanceSphereExpression(Property, Shape, Distance, !_trueOrNot);
        }
    }
}
