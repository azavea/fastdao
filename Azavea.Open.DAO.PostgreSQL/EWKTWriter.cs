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
using Azavea.Open.Common;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

namespace Azavea.Open.DAO.PostgreSQL
{
    /// <summary>
    /// PostGIS doesn't use exact WKT, it uses "extended WKT".  Unfortunately the
    /// WKTWriter class doesn't allow you to extend and override its methods
    /// (and many are static anyway) so we offer this alternate implementation
    /// as a WKT writer that can write PostGIS WKT.
    /// </summary>
    public class EWKTWriter
    {
        /// <summary>
        /// This is used to write the normal part of the WKT.
        /// </summary>
        private readonly WKTWriter _basicWriter = new WKTWriter();

        /// <summary>
        /// Converts a geometry into EWKT.
        /// </summary>
        /// <param name="geometry">The geometry to write.</param>
        /// <returns>The EWKT that represents this geometry.</returns>
        public string Write(IGeometry geometry)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry", "Cannot convert null geometry to EWKT.");
            }

            // If there was no SRID on the geometry, then don't use a SRID (rather than use a SRID of 0)
            if (geometry.SRID == 0)
            {
                return _basicWriter.Write(geometry);
            }

            // Otherwise tack on the SRID.
            try
            {
                return String.Format("SRID={0};{1}", geometry.SRID, _basicWriter.Write(geometry));
            }
            catch (Exception e)
            {
                throw new LoggingException("Unable to write geometry to ewkt: " + geometry, e);
            }
        }

        /// <summary>
        /// Converts a single point into EWKT
        /// </summary>
        /// <param name="point">The point to write</param>
        /// <returns>The EWKT that represents this point.</returns>
        public static string ToPoint(IPoint point)
        {
            if (point == null)
            {
                throw new ArgumentNullException("point", "Cannot convert null point to EWKT.");
            }
            try
            {
                return String.Format("SRID={0};{1}", point.SRID, WKTWriter.ToPoint(point.Coordinate));
            }
            catch (Exception e)
            {
                throw new LoggingException("Unable to write point to ewkt: " + point, e);
            }
        }
    }
}