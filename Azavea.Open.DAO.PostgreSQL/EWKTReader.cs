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
    /// WKTReader class doesn't allow you to extend and override its methods
    /// (and many are static anyway) so we offer this alternate implementation
    /// as a WKT reader that can read PostGIS WKT.
    /// </summary>
    public class EWKTReader
    {
        /// <summary>
        /// This is used to read the normal part of the WKT.
        /// </summary>
        private readonly WKTReader _basicReader = new WKTReader();
        /// <summary>
        /// Reads EWKT and produces a geometry.
        /// </summary>
        /// <param name="extendedWellKnownText">EWKT to parse.</param>
        /// <returns>The geometry represented by the EWKT.</returns>
        public IGeometry Read(string extendedWellKnownText)
        {
            if (extendedWellKnownText == null)
            {
                throw new ArgumentNullException("extendedWellKnownText", "Cannot read null text.");
            }
            try
            {
                // strip off the SRID part
                string[] parts = extendedWellKnownText.Split(';');
                IGeometry theGeom = _basicReader.Read(parts[parts.Length-1]);
                // If there was no SRID part, then there is no SRID.  This is allowed: SRID is -1 in PostGIS.
                // Otherwise, add the SRID.
                if (parts.Length > 1)
                {
                    theGeom.SRID = Int32.Parse(parts[0].Split('=')[1]);
                }
                return theGeom;
            }
            catch(Exception ex)
            {
                throw new LoggingException("Unable to parse EWKT: " + extendedWellKnownText, ex);
            }
        }
    }
}