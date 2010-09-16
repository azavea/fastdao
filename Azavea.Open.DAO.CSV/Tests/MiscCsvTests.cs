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
using System.IO;
using Azavea.Open.Common.Collections;
using NUnit.Framework;

namespace Azavea.Open.DAO.CSV.Tests
{
    /// <exclude/>
    [TestFixture]
    public class MiscCsvTests
    {
        /// <exclude/>
        [TestFixtureSetUp]
        public void Init()
        {
            // Reset the unit test data by copying the templates, since we don't know what any
            // other unit tests (or a previous run of this test) has done to the state of the files.
            foreach (string fileName in Directory.GetFiles("..\\..\\Tests\\", "*.csv"))
            {
                File.Delete(fileName);
            }
            foreach (string fileName in Directory.GetFiles("..\\..\\Tests\\Template\\", "*.csv"))
            {
                File.Copy(fileName, "..\\..\\Tests\\" + Path.GetFileName(fileName));
            }
        }

        /// <exclude/>
        [Test]
        public void TestWriteCsvWithQuotes()
        {
            StringWriter writer = new StringWriter();

            // This is testing to verify the CsvDescriptor constructor that takes a 
            // Writer and a CVSQuoteLevel.
            CsvTestObj testObj1 = new CsvTestObj();
            CheckedDictionary<string, object> testDict1 = new CheckedDictionary<string, object>();
            testObj1.One = 50;
            testObj1.Two = -1.0;
            testObj1.Three = "Yo";
            testObj1.Four = new DateTime(2001, 1, 1, 1, 1, 1);
            testObj1.Five = null;
            testDict1["One"] = testObj1.One;
            testDict1["Two"] = testObj1.Two;
            testDict1["Three"] = testObj1.Three;
            testDict1["Four"] = testObj1.Four;
            testDict1["Five"] = testObj1.Five;

            CsvTestObj testObj2 = new CsvTestObj();
            CheckedDictionary<string, object> testDict2 = new CheckedDictionary<string, object>();
            testObj2.One = int.MaxValue;
            testObj2.Two = double.MinValue;
            testObj2.Three = null;
            testObj2.Four = DateTime.MinValue;
            testObj2.Five = "";
            testDict2["One"] = testObj2.One;
            testDict2["Two"] = testObj2.Two;
            testDict2["Three"] = testObj2.Three;
            testDict2["Four"] = testObj2.Four;
            testDict2["Five"] = testObj2.Five;

            ClassMapping mapping1 = MakeMapping("n/a", "WriteOne", true);
            CsvDescriptor desc1 = new CsvDescriptor(writer, CsvQuoteLevel.QuoteStrings);
            DictionaryDao dao1 = new DictionaryDao(desc1, mapping1);
            dao1.Insert(testDict1);
            dao1.Insert(testDict2);

            string csv = writer.ToString();
            Assert.IsNotEmpty(csv, "The writer should not return an empty string. ");
        }

        /// <exclude/>
        [Test]
        public void TestWriteCsv()
        {
            CsvTestObj testObj1 = new CsvTestObj();
            CheckedDictionary<string, object> testDict1 = new CheckedDictionary<string, object>();
            testObj1.One = 50;
            testObj1.Two = -1.0;
            testObj1.Three = "Yo";
            testObj1.Four = new DateTime(2001, 1, 1, 1, 1, 1);
            testObj1.Five = null;
            testDict1["One"] = testObj1.One;
            testDict1["Two"] = testObj1.Two;
            testDict1["Three"] = testObj1.Three;
            testDict1["Four"] = testObj1.Four;
            testDict1["Five"] = testObj1.Five;

            CsvTestObj testObj2 = new CsvTestObj();
            CheckedDictionary<string, object> testDict2 = new CheckedDictionary<string, object>();
            testObj2.One = int.MaxValue;
            testObj2.Two = double.MinValue;
            testObj2.Three = null;
            testObj2.Four = DateTime.MinValue;
            testObj2.Five = "";
            testDict2["One"] = testObj2.One;
            testDict2["Two"] = testObj2.Two;
            testDict2["Three"] = testObj2.Three;
            testDict2["Four"] = testObj2.Four;
            testDict2["Five"] = testObj2.Five;

            ClassMapping mapping1 = MakeMapping("n/a", "WriteOne", true);
            CsvDescriptor desc1 = new CsvDescriptor("..\\..\\Tests");
            DictionaryDao dao1 = new DictionaryDao(desc1, mapping1);
            dao1.Truncate();
            dao1.Insert(testDict1);
            dao1.Insert(testDict2);

            ClassMapping mapping2 = MakeMapping("Azavea.Open.DAO.CSV.Tests.CsvTestObj,Azavea.Open.DAO.CSV", "Doesn'tMatter", true);
            CsvDescriptor desc2 = new CsvDescriptor(CsvConnectionType.FileName,
                                                    "..\\..\\Tests\\WriteTwo.csv");
            FastDAO<CsvTestObj> dao2 = new FastDAO<CsvTestObj>(desc2, mapping2);
            dao2.Truncate();
            dao2.Insert(testObj1);
            dao2.Insert(testObj2);

            ClassMapping mapping3 = MakeMapping("n/a", "AlsoDoesn'tMatter", true);
            using (StreamWriter sw = new StreamWriter("..\\..\\Tests\\WriteThree.csv", false))
            {
                CsvDescriptor desc3 = new CsvDescriptor(sw);
                DictionaryDao dao3 = new DictionaryDao(desc3, mapping3);
                // Can't truncate this one.
                dao3.Insert(testDict1);
                dao3.Insert(testDict2);
            }

            ClassMapping mapping4 = MakeMapping("n/a", "WriteFour", false);
            CsvDescriptor desc4 = new CsvDescriptor("..\\..\\Tests");
            DictionaryDao dao4 = new DictionaryDao(desc4, mapping4);
            dao4.Truncate();
            dao4.Insert(testDict1);
            dao4.Insert(testDict2);

            ClassMapping mapping5 = MakeMapping("Azavea.Open.DAO.CSV.Tests.CsvTestObj,Azavea.Open.DAO.CSV", "Doesn'tMatter", false);
            CsvDescriptor desc5 = new CsvDescriptor(CsvConnectionType.FileName,
                                                    "..\\..\\Tests\\WriteFive.csv");
            FastDAO<CsvTestObj> dao5 = new FastDAO<CsvTestObj>(desc5, mapping5);
            dao5.Truncate();
            dao5.Insert(testObj1);
            dao5.Insert(testObj2);

            ClassMapping mapping6 = MakeMapping("n/a", "AlsoDoesn'tMatter", false);
            using (StreamWriter sw = new StreamWriter("..\\..\\Tests\\WriteSix.csv", false))
            {
                CsvDescriptor desc6 = new CsvDescriptor(sw);
                DictionaryDao dao6 = new DictionaryDao(desc6, mapping6);
                // Can't truncate this one.
                dao6.Insert(testDict1);
                dao6.Insert(testDict2);
            }

            // Now, assert they are all correct.  1, 2, and 3 should be the same (they have headers)
            // and 4, 5, and 6 should be the same (without headers).
            AssertFileContentsSame("..\\..\\Tests\\WriteOne.csv", "..\\..\\Tests\\WriteTwo.csv");
            AssertFileContentsSame("..\\..\\Tests\\WriteOne.csv", "..\\..\\Tests\\WriteThree.csv");
            AssertFileGreater("..\\..\\Tests\\WriteOne.csv", "..\\..\\Tests\\WriteFour.csv");
            AssertFileContentsSame("..\\..\\Tests\\WriteFour.csv", "..\\..\\Tests\\WriteFive.csv");
            AssertFileContentsSame("..\\..\\Tests\\WriteFour.csv", "..\\..\\Tests\\WriteFive.csv");
        }

        private static void AssertFileGreater(string greaterFile, string lesserFile)
        {
            FileInfo greaterInfo = new FileInfo(greaterFile);
            FileInfo lesserInfo = new FileInfo(lesserFile);
            Assert.IsTrue(greaterInfo.Exists, greaterFile + " was not created.");
            Assert.IsTrue(lesserInfo.Exists, lesserFile + " was not created.");
            Assert.Greater(greaterInfo.Length, (double)lesserInfo.Length, 
                           greaterFile + " was not longer than " + lesserFile);
        }

        private static void AssertFileContentsSame(string file1, string file2)
        {
            FileInfo info1 = new FileInfo(file1);
            FileInfo info2 = new FileInfo(file2);
            Assert.IsTrue(info1.Exists, file1 + " was not created.");
            Assert.IsTrue(info2.Exists, file2 + " was not created.");
            Assert.AreEqual(info1.Length, info2.Length,
                            file1 + " was not the same size as " + file2);

            StreamReader reader1 = new StreamReader(file1);
            StreamReader reader2 = new StreamReader(file2);
            int lineNum = 1;
            while (!reader1.EndOfStream)
            {
                Assert.AreEqual(reader1.ReadLine(), reader2.ReadLine(), "Line " + lineNum + ": " + file1 +
                                                                        " differs from " + file2);
                lineNum++;
            }
        }

        private static ClassMapping MakeMapping(string className, string name, bool headerRow)
        {
            ClassMapping mapping = new ClassMapping(className, name,
                                                                      new ClassMapColDefinition[] {
                                                                                new ClassMapColDefinition("One", headerRow ? "col1" : "1", null),
                                                                                new ClassMapColDefinition("Two", headerRow ? "col2" : "2", null),
                                                                                new ClassMapColDefinition("Three", headerRow ? "col3" : "3", null),
                                                                                new ClassMapColDefinition("Four", headerRow ? "col4" : "4", null),
                                                                                new ClassMapColDefinition("Five", headerRow ? "col5" : "5", null),
                                                                            }, !"n/a".Equals(className));
            return mapping;
        }
    }

    /// <exclude/>
    public class CsvTestObj
    {
        /// <exclude/>
        public int One;
        /// <exclude/>
        public double Two;
        /// <exclude/>
        public string Three;
        /// <exclude/>
        public DateTime Four;
        /// <exclude/>
        public object Five;
    }
}