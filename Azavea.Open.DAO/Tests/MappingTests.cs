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
using Azavea.Open.DAO.Memory;
using NUnit.Framework;

namespace Azavea.Open.DAO.Tests
{
    /// <exclude/>
    [TestFixture]
    public class MappingTests
    {
        /// <exclude/>
        [Test]
        public void TestEasyCompositeKeys()
        {
            AssertCompositeMapping(new FastDAO<EasyCompositeKeyClass>(
                new MemoryDescriptor("Test1"), "..\\..\\Tests\\Mapping.xml").ClassMap);
        }
        /// <exclude/>
        [Test]
        public void TestNHibernateCompositeKeys()
        {
            AssertCompositeMapping(new FastDAO<NHibernateCompositeKeyClass>(
                new MemoryDescriptor("Test1"), "..\\..\\Tests\\Mapping.xml").ClassMap);
        }
        /// <exclude/>
        [Test]
        public void TestInlineMapping()
        {
            // These should work fine since they are mapped inline.
            new FastDAO<NameClass>(new Config("..\\..\\Tests\\MemoryDao.config", "MemoryDaoConfig"), "DAOInlineMapping");
            new FastDAO<EnumClass>(new Config("..\\..\\Tests\\MemoryDao.config", "MemoryDaoConfig"), "DAOInlineMapping");
            Exception ex = null;
            try
            {
                // This should fail since it isn't in the inline mapping.
                new FastDAO<BoolClass>(new Config("..\\..\\Tests\\MemoryDao.config", "MemoryDaoConfig"), "DAOInlineMapping");
            }
            catch (Exception e)
            {
                ex = e;
            }
            Assert.IsNotNull(ex, "Failed to throw an exception on class that is not in the mapping.");
            Assert.IsTrue(ex.Message.Contains("BoolClass"),
                "Doesn't mention the name of the class that isn't mapped in the exception message.");
        }

        private static void AssertCompositeMapping(ClassMapping mapping)
        {
            Assert.AreEqual(3, mapping.IdDataColsByObjAttrs.Count, "Should be three id columns.");
            Assert.AreEqual(2, mapping.NonIdDataColsByObjAttrs.Count, "Should be 2 property columns.");
            Assert.AreEqual("IDCol1", mapping.IdDataColsByObjAttrs["ID1"], "Wrong column mapped for ID 1.");
            Assert.AreEqual("IDCol2", mapping.IdDataColsByObjAttrs["ID2"], "Wrong column mapped for ID 2.");
            Assert.AreEqual("IDCol3", mapping.IdDataColsByObjAttrs["ID3"], "Wrong column mapped for ID 3.");
            Assert.AreEqual("PropCol1", mapping.NonIdDataColsByObjAttrs["Prop1"], "Wrong column mapped for Property 1.");
            Assert.AreEqual("PropCol2", mapping.NonIdDataColsByObjAttrs["Prop2"], "Wrong column mapped for Property 2.");
        }
    }
    /// <exclude/>
    public abstract class BaseCompositeKeyClass
    {
        /// <exclude/>
        public string ID1;
        /// <exclude/>
        public string ID2;
        /// <exclude/>
        public string ID3;
        /// <exclude/>
        public string Prop1;
        /// <exclude/>
        public string Prop2;
    }
    /// <exclude/>
    public class EasyCompositeKeyClass : BaseCompositeKeyClass { }
    /// <exclude/>
    public class NHibernateCompositeKeyClass : BaseCompositeKeyClass { }
}
