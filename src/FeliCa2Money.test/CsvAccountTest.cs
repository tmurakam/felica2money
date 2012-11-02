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
    class CsvAccountTest
    {
        CsvAccount mAccount;
        CsvRule mRule;
        string mTempFileName;
        StreamWriter mSw;

        [SetUp]
        public void setUp()
        {
            mAccount = new CsvAccount();
            mAccount.branchId = "0";
            mAccount.accountId = "0";

            mRule = new CsvRule();
            mRule.firstLine = "FIRST_LINE";
            mRule.SetFormat("Date,Income,Balance,Desc,Memo");

            mTempFileName = Path.GetTempFileName();
            mSw = new StreamWriter(mTempFileName, false, System.Text.Encoding.Default);
        }

        [TearDown]
        public void tearDown()
        {
            mSw.Close();
            mAccount.Close();

            File.Delete(mTempFileName);
        }

        // 空ファイル読み込み
        [Test]
        public void loadEmptyFile()
        {
            mSw.Close();

            mAccount.startReading(mTempFileName, mRule);
            mAccount.ReadTransactions();
            Assert.AreEqual(0, mAccount.transactions.Count);
        }

        // FirstLine のみのファイル読み込み
        [Test]
        public void loadOnlyFirstLineFile()
        {
            mSw.WriteLine("FIRST_LINE");
            mSw.Close();

            mAccount.startReading(mTempFileName, mRule);
            mAccount.ReadTransactions();
            Assert.AreEqual(0, mAccount.transactions.Count);
        }

        // FirstLine がないファイルの読み込み
        [Test]
        public void loadNoFirstLineFile()
        {
            mSw.WriteLine("2011/1/1, 50000, 50000, Desc, Memo");
            mSw.Close();

            mAccount.startReading(mTempFileName, mRule);
            mAccount.ReadTransactions();
            Assert.AreEqual(0, mAccount.transactions.Count);
        }

        // 通常読み込み
        [Test]
        public void loadNormalFile()
        {
            mSw.WriteLine("FIRST_LINE");
            mSw.WriteLine("2011/1/2, 500, 50000, Desc, Memo");
            mSw.Close();

            mAccount.startReading(mTempFileName, mRule);
            mAccount.ReadTransactions();
            Assert.AreEqual(1, mAccount.transactions.Count);
            Transaction t = mAccount.transactions.getAt(0);

            Assert.AreEqual(t.date.Year, 2011);
            Assert.AreEqual(t.date.Month, 1);
            Assert.AreEqual(t.date.Day, 2);
            Assert.AreEqual(t.value, 500);
            Assert.AreEqual(t.balance, 50000);
            Assert.AreEqual(t.desc, "Desc");
        }

        // Ascent テスト
        [Test]
        public void ascentOrder()
        {
            mSw.WriteLine("FIRST_LINE");
            mSw.WriteLine("2011/1/1, 100, 10000, Desc, Memo");
            mSw.WriteLine("2011/1/2, 200, 10200, Desc, Memo");
            mSw.Close();

            mRule.order = "Ascent";
            mAccount.startReading(mTempFileName, mRule);
            mAccount.ReadTransactions();
            Assert.AreEqual(2, mAccount.transactions.Count);
            Transaction t = mAccount.transactions.getAt(0);

            Assert.AreEqual(t.date.Day, 1);
            Assert.AreEqual(t.value, 100);
        }

        // Descent テスト
        [Test]
        public void descentOrder()
        {
            mSw.WriteLine("FIRST_LINE");
            mSw.WriteLine("2011/1/2, 200, 10200, Desc, Memo");
            mSw.WriteLine("2011/1/1, 100, 10000, Desc, Memo");
            mSw.Close();

            mRule.order = "Descent";
            mAccount.startReading(mTempFileName, mRule);
            mAccount.ReadTransactions();
            Assert.AreEqual(2, mAccount.transactions.Count);
            Transaction t = mAccount.transactions.getAt(0);

            Assert.AreEqual(t.date.Day, 1);
            Assert.AreEqual(t.value, 100);
        }

        // 自動 Order テスト
        [Test]
        public void autoOrder()
        {
            mSw.WriteLine("FIRST_LINE");
            mSw.WriteLine("2011/1/2, 200, 10200, Desc, Memo");
            mSw.WriteLine("2011/1/1, 100, 10000, Desc, Memo");
            mSw.Close();

            mRule.order = "Sort";
            mAccount.startReading(mTempFileName, mRule);
            mAccount.ReadTransactions();
            Assert.AreEqual(2, mAccount.transactions.Count);
            Transaction t = mAccount.transactions.getAt(0);

            Assert.AreEqual(t.date.Day, 1);
            Assert.AreEqual(t.value, 100);
        }

        // ID自動採番テスト
        [Test]
        public void idSerialTest()
        {
            mSw.WriteLine("FIRST_LINE");
            mSw.WriteLine("2011/1/1, 100, 10000, Desc, Memo");
            mSw.WriteLine("2011/1/2, 100, 10600, Desc, Memo");
            mSw.WriteLine("2011/1/1, 200, 10200, Desc, Memo");
            mSw.WriteLine("2011/1/2, 200, 10800, Desc, Memo");
            mSw.WriteLine("2011/1/1, 300, 10500, Desc, Memo");
            mSw.WriteLine("2011/1/2, 300, 11100, Desc, Memo");
            mSw.Close();

            mRule.order = "Sort";

            mAccount.startReading(mTempFileName, mRule);
            mAccount.ReadTransactions();
            Assert.AreEqual(6, mAccount.transactions.Count);

            Assert.AreEqual(0, mAccount.transactions.getAt(0).id);
            Assert.AreEqual(1, mAccount.transactions.getAt(1).id);
            Assert.AreEqual(2, mAccount.transactions.getAt(2).id);
            Assert.AreEqual(0, mAccount.transactions.getAt(3).id);
            Assert.AreEqual(1, mAccount.transactions.getAt(4).id);
            Assert.AreEqual(2, mAccount.transactions.getAt(5).id);
        }
    }
}

