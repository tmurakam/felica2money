using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using FeliCa2Money;

namespace FeliCa2Money.test
{
    [TestFixture]
    class AgrFileTest
    {
        AgrFile mAgrFile;
        string mTempFileName;
        StreamWriter mSw;

        [SetUp]
        public void setUp()
        {
            mAgrFile = new AgrFile();

            mTempFileName = Path.GetTempFileName();
            mSw = new StreamWriter(mTempFileName, false, System.Text.Encoding.Default);
        }

        [TearDown]
        public void tearDown()
        {
            mSw.Close();
            File.Delete(mTempFileName);
        }

        [Test]
        public void noHeader()
        {
            mSw.Close();

            Assert.False(mAgrFile.loadFromFile(mTempFileName));
        }

        [Test]
        public void invalidHeader()
        {
            mSw.WriteLine("\"あぐりっぱ\",\"1.1\"");
            mSw.Close();

            Assert.False(mAgrFile.loadFromFile(mTempFileName));
        }

        [Test]
        public void emptyFile()
        {
            writeHeader();
            mSw.Close();

            Assert.True(mAgrFile.loadFromFile(mTempFileName));
            Assert.AreEqual(0, mAgrFile.accounts.Count);
        }

        // internal functions
        private void writeHeader()
        {
            mSw.WriteLine("\"あぐりっぱ\",\"1.0\"");
            mSw.WriteLine("<START_HEAD>");
            mSw.WriteLine("\"全アカウント数\",\"100\"");
            mSw.WriteLine("<END_HEAD>");
        }
    }
}
