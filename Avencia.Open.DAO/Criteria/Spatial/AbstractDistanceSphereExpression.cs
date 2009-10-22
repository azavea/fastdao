using System;
using GeoAPI.Geometries;

namespace Avencia.Open.DAO.Criteria.Spatial
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
