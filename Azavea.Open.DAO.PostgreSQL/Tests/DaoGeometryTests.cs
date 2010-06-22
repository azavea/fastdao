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
using System.Collections.Generic;
using Azavea.Open.Common;
using Azavea.Open.DAO.Criteria;
using Azavea.Open.DAO.Criteria.Spatial;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Azavea.Open.DAO.PostgreSQL.Tests
{
    /// <exclude/>
    [TestFixture]
    public class DaoGeometryTests
    {
        private FastDAO<PointClass> _pointDao;
        private FastDAO<LineClass> _lineDao;
        private FastDAO<PolyClass> _polyDao;

        /// <exclude/>
        [TestFixtureSetUp]
        public void SetUp()
        {
            try
            {
                _polyDao = new FastDAO<PolyClass>(new Config("..\\..\\Tests\\PostgreSqlDao.config", "PostgreSqlDaoConfig"), "DAO");
                _lineDao = new FastDAO<LineClass>(new Config("..\\..\\Tests\\PostgreSqlDao.config", "PostgreSqlDaoConfig"), "DAO");
                _pointDao = new FastDAO<PointClass>(new Config("..\\..\\Tests\\PostgreSqlDao.config", "PostgreSqlDaoConfig"), "DAO");
                SetupPoints();
                SetupLines();
                SetupPolys();
            }
            catch (Exception e)
            {
                string message = "ERROR: Exception while setting up PostGIS DAO tests: " + e;
                Console.WriteLine(message);
                Assert.Fail(message);
            }
        }

        /// <exclude/>
        [Test]
        public void TestGetAllPoints()
        {
            IList<PointClass> points = _pointDao.Get();
            Assert.AreEqual(10, points.Count, "Wrong number of points.");
        }

        /// <exclude/>
        [Test]
        public void TestGetAllLines()
        {
            IList<LineClass> lines = _lineDao.Get();
            Assert.AreEqual(6, lines.Count, "Wrong number of lines.");
        }

        /// <exclude/>
        [Test]
        public void TestGetAllPolys()
        {
            IList<PolyClass> polys = _polyDao.Get();
            Assert.AreEqual(6, polys.Count, "Wrong number of polygons.");
        }

        /// <exclude/>
        [Test]
        public void TestGetMultipleCriteria()
        {
            DaoCriteria crit = new DaoCriteria(BooleanOperator.Or);
            crit.Expressions.Add(new EqualExpression("Double", 10000));
            crit.Expressions.Add(new GreaterExpression("Int", 300));
            IList<PointClass> points = _pointDao.Get(crit);
            Assert.AreEqual(5, points.Count, "Wrong number of points.");
            IList<LineClass> lines = _lineDao.Get(crit);
            Assert.AreEqual(4, lines.Count, "Wrong number of lines.");
            IList<PolyClass> polys = _polyDao.Get(crit);
            Assert.AreEqual(4, polys.Count, "Wrong number of polygons.");
        }

        /// <exclude/>
        [Test]
        public void TestGetIntersects()
        {
            DaoCriteria crit = new DaoCriteria();
            ICoordinate[] coords = new ICoordinate[5];
            coords[0] = new Coordinate(100, 100);
            coords[1] = new Coordinate(200, 100);
            coords[2] = new Coordinate(200, 150);
            coords[3] = new Coordinate(100, 150);
            coords[4] = new Coordinate(100, 100);
            IGeometry poly = new Polygon(new LinearRing(coords));
            crit.Expressions.Add(new IntersectsExpression("Shape", poly));
            IList<PointClass> points = _pointDao.Get(crit);
            Assert.AreEqual(6, points.Count, "Wrong number of points.");
            IList<LineClass> lines = _lineDao.Get(crit);
            Assert.AreEqual(2, lines.Count, "Wrong number of lines.");
            IList<PolyClass> polys = _polyDao.Get(crit);
            Assert.AreEqual(2, polys.Count, "Wrong number of polygons.");
        }

        /// <exclude/>
        [Test]
        public void TestGetContainedBy()
        {
            DaoCriteria crit = new DaoCriteria();
            ICoordinate[] coords = new ICoordinate[5];
            coords[0] = new Coordinate(100, 100);
            coords[1] = new Coordinate(200, 100);
            coords[2] = new Coordinate(200, 150);
            coords[3] = new Coordinate(100, 150);
            coords[4] = new Coordinate(100, 100);
            IGeometry poly = new Polygon(new LinearRing(coords));
            crit.Expressions.Add(new WithinExpression("Shape", poly));
            IList<PointClass> points = _pointDao.Get(crit);
            Assert.AreEqual(4, points.Count, "Wrong number of points.");
            IList<LineClass> lines = _lineDao.Get(crit);
            Assert.AreEqual(1, lines.Count, "Wrong number of lines.");
            IList<PolyClass> polys = _polyDao.Get(crit);
            Assert.AreEqual(1, polys.Count, "Wrong number of polygons.");
        }

        /// <exclude/>
        [Test]
        public void TestBatchInsert()
        {
            try
            {
                IList<PointClass> points = new List<PointClass>();
                PointClass pc1 = new PointClass();
                pc1.Shape = new Point(10, 10);
                PointClass pc2 = new PointClass();
                points.Add(pc1);
                pc1.Shape = new Point(20, 20);
                points.Add(pc2);
                _pointDao.Insert(points);
            }
            finally
            {
                // Reset the points to the "normal" state so other tests don't fail.
                SetupPoints();
            }
        }

        private void SetupPoints()
        {
            _pointDao.Truncate();
            _pointDao.Insert(MakePoint(100, 100));
            _pointDao.Insert(MakePoint(110, 110));
            _pointDao.Insert(MakePoint(120, 120));
            _pointDao.Insert(MakePoint(130, 130));
            _pointDao.Insert(MakePoint(140, 140));
            _pointDao.Insert(MakePoint(150, 150));
            _pointDao.Insert(MakePoint(160, 160));
            _pointDao.Insert(MakePoint(170, 170));
            _pointDao.Insert(MakePoint(180, 180));
            _pointDao.Insert(MakePoint(190, 190));
        }

        private void SetupLines()
        {
            _lineDao.Truncate();
            _lineDao.Insert(MakeLine(100, 100));
            _lineDao.Insert(MakeLine(100, 200));
            _lineDao.Insert(MakeLine(100, 300));
            _lineDao.Insert(MakeLine(200, 100));
            _lineDao.Insert(MakeLine(200, 200));
            _lineDao.Insert(MakeLine(200, 300));
        }

        private void SetupPolys()
        {
            _polyDao.Truncate();
            _polyDao.Insert(MakePoly(100, 100));
            _polyDao.Insert(MakePoly(100, 200));
            _polyDao.Insert(MakePoly(100, 300));
            _polyDao.Insert(MakePoly(200, 100));
            _polyDao.Insert(MakePoly(200, 200));
            _polyDao.Insert(MakePoly(200, 300));
        }

        /// <exclude/>
        public PointClass MakePoint(int x, int y)
        {
            PointClass retVal = new PointClass();
            retVal.Int = x + y;
            retVal.Double = x * y;
            retVal.Shape = new Point(x, y);
            retVal.Shape.SRID = -1;
            return retVal;
        }
        /// <exclude/>
        public LineClass MakeLine(int x, int y)
        {
            LineClass retVal = new LineClass();
            retVal.Int = x + y;
            retVal.Double = x * y;
            ICoordinate[] coords = new ICoordinate[4];
            coords[0] = new Coordinate(x, y);
            coords[1] = new Coordinate(x + 10, y);
            coords[2] = new Coordinate(x + 10, y + 20);
            coords[3] = new Coordinate(x + 20, y + 20);
            retVal.Shape = new LineString(coords);
            retVal.Shape.SRID = -1;
            return retVal;
        }
        /// <exclude/>
        public PolyClass MakePoly(int x, int y)
        {
            PolyClass retVal = new PolyClass();
            retVal.Int = x + y;
            retVal.Double = x * y;
            retVal.Date = DateTime.Now;
            ICoordinate[] coords = new ICoordinate[5];
            coords[0] = new Coordinate(x, y);
            coords[1] = new Coordinate(x + 10, y);
            coords[2] = new Coordinate(x + 10, y + 20);
            coords[3] = new Coordinate(x, y + 20);
            coords[4] = new Coordinate(x, y);
            retVal.Shape = new Polygon(new LinearRing(coords));
            retVal.Shape.SRID = -1;
            return retVal;
        }
    }

    /// <exclude/>
    public class PointClass
    {
        /// <exclude/>
        public int ObjectID;
        /// <exclude/>
        public int Int;
        /// <exclude/>
        public string String;
        /// <exclude/>
        public double Double;
        /// <exclude/>
        public IGeometry Shape;
    }

    /// <exclude/>
    public class LineClass
    {
        /// <exclude/>
        public int ObjectID;
        /// <exclude/>
        public int Int;
        /// <exclude/>
        public string String;
        /// <exclude/>
        public double Double;
        /// <exclude/>
        public IGeometry Shape;
    }

    /// <exclude/>
    public class PolyClass
    {
        /// <exclude/>
        public int ObjectID;
        /// <exclude/>
        public int Int;
        /// <exclude/>
        public string String;
        /// <exclude/>
        public double Double;
        /// <exclude/>
        public DateTime? Date;
        /// <exclude/>
        public IGeometry Shape;
    }
}