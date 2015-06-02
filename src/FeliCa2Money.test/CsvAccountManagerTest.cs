// -*-  Mode:C++; c-basic-offset:4; tab-width:4; indent-tabs-mode:nil -*-

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using FeliCa2Money;

namespace FeliCa2Money.test
{
    [TestFixture]
    class CsvAccountManagerTest
    {
        CsvAccountManager mManager;
        CsvRule mRule;
        string mTempFileName;
        StreamWriter mSw;

        [SetUp]
        public void setUp()
        {
            mManager = new CsvAccountManager();

            mRule = new CsvRule();
            mRule.FirstLine = "FIRST_LINE";
            mRule.SetFormat("Date,Income,Balance,Desc,Memo");
            mManager.addRule(mRule);

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
        public void emptyFileNoRule()
        {
            mSw.Close();

            /// 空ファイルの場合にルールなしになること
            Assert.IsNull(mManager.findMatchingRuleForCsv(mTempFileName));
        }

        [Test]
        public void MatchRule()
        {
            mSw.WriteLine("FIRST_LINE");
            mSw.Close();

            Assert.AreEqual(mRule, mManager.findMatchingRuleForCsv(mTempFileName));
        }

        [Test]
        public void NoMatchRule()
        {
            mSw.WriteLine("NO_MATCH");
            mSw.Close();

            Assert.IsNull(mManager.findMatchingRuleForCsv(mTempFileName));
        }
    }
}

