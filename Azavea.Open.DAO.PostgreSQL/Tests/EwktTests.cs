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
using NUnit.Framework;

namespace Azavea.Open.DAO.PostgreSQL.Tests
{
    /// <exclude/>
    [TestFixture]
    public class EwktTests
    {
        /// <exclude/>
        [Test]
        public void TestReadPointNoSrid()
        {
            const string text = "POINT(100 100)";
            IPoint result = (IPoint)new EWKTReader().Read(text);
            Assert.AreEqual(100, result.X);
            Assert.AreEqual(100, result.Y);
            Assert.AreEqual(0, result.SRID);
        }

        /// <exclude/>
        [Test]
        public void TestReadPoint4326()
        {
            const string text = "SRID=4326;POINT(-75.137456 40.028532)";
            IPoint result = (IPoint)new EWKTReader().Read(text);
            Assert.AreEqual(-75.137456, result.X);
            Assert.AreEqual(40.028532, result.Y);
            Assert.AreEqual(4326, result.SRID);
        }

        /// <exclude />
        [Test]
        public void TestReadLineString()
        {
            const string text =
                "SRID=2272;LINESTRING(2697646.11346083 232178.576010919,2697620.861473 232005.116936741)";
            IGeometry result = new EWKTReader().Read(text);
            Assert.IsInstanceOfType(typeof(ILineString), result);
        }
    }
}