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
            mAccount.BranchId = "0";
            mAccount.AccountId = "0";

            mRule = new CsvRule();
            mRule.FirstLine = "FIRST_LINE";
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
        public void LoadEmptyFile()
        {
            mSw.Close();

            mAccount.StartReading(mTempFileName, mRule);
            try
            {
                mAccount.ReadTransactions();
                Assert.Fail();
            }
            catch (CsvReadException e)
            {
                // ok
            }
        }

        // FirstLine のみのファイル読み込み
        [Test]
        public void loadOnlyFirstLineFile()
        {
            mSw.WriteLine("FIRST_LINE");
            mSw.Close();

            mAccount.StartReading(mTempFileName, mRule);
            mAccount.ReadTransactions();
            Assert.AreEqual(0, mAccount.Transactions.Count);
        }

        // FirstLine がないファイルの読み込み
        [Test]
        public void loadNoFirstLineFile()
        {
            mSw.WriteLine("2011/1/1, 50000, 50000, Desc, Memo");
            mSw.Close();

            mAccount.StartReading(mTempFileName, mRule);
            try
            {
                mAccount.ReadTransactions();
                Assert.Fail();
            }
            catch (CsvReadException e)
            {
                // ok
            }
        }

        // 通常読み込み
        [Test]
        public void loadNormalFile()
        {
            mSw.WriteLine("FIRST_LINE");
            mSw.WriteLine("2011/1/2, 500, 50000, Desc, Memo");
            mSw.Close();

            mAccount.StartReading(mTempFileName, mRule);
            mAccount.ReadTransactions();
            Assert.AreEqual(1, mAccount.Transactions.Count);
            Transaction t = mAccount.Transactions[0];

            Assert.AreEqual(t.Date.Year, 2011);
            Assert.AreEqual(t.Date.Month, 1);
            Assert.AreEqual(t.Date.Day, 2);
            Assert.AreEqual(t.Value, 500);
            Assert.AreEqual(t.Balance, 50000);
            Assert.AreEqual(t.Desc, "Desc");
        }

        // Ascent テスト
        [Test]
        public void ascentOrder()
        {
            mSw.WriteLine("FIRST_LINE");
            mSw.WriteLine("2011/1/1, 100, 10000, Desc, Memo");
            mSw.WriteLine("2011/1/2, 200, 10200, Desc, Memo");
            mSw.Close();

            mRule.OrderString = "Ascent";
            mAccount.StartReading(mTempFileName, mRule);
            mAccount.ReadTransactions();
            Assert.AreEqual(2, mAccount.Transactions.Count);
            Transaction t = mAccount.Transactions[0];

            Assert.AreEqual(t.Date.Day, 1);
            Assert.AreEqual(t.Value, 100);
        }

        // Descent テスト
        [Test]
        public void descentOrder()
        {
            mSw.WriteLine("FIRST_LINE");
            mSw.WriteLine("2011/1/2, 200, 10200, Desc, Memo");
            mSw.WriteLine("2011/1/1, 100, 10000, Desc, Memo");
            mSw.Close();

            mRule.OrderString = "Descent";
            mAccount.StartReading(mTempFileName, mRule);
            mAccount.ReadTransactions();
            Assert.AreEqual(2, mAccount.Transactions.Count);
            Transaction t = mAccount.Transactions[0];

            Assert.AreEqual(t.Date.Day, 1);
            Assert.AreEqual(t.Value, 100);
        }

        // 自動 Order テスト
        [Test]
        public void autoOrder()
        {
            mSw.WriteLine("FIRST_LINE");
            mSw.WriteLine("2011/1/2, 200, 10200, Desc, Memo");
            mSw.WriteLine("2011/1/1, 100, 10000, Desc, Memo");
            mSw.Close();

            mRule.OrderString = "Sort";
            mAccount.StartReading(mTempFileName, mRule);
            mAccount.ReadTransactions();
            Assert.AreEqual(2, mAccount.Transactions.Count);
            Transaction t = mAccount.Transactions[0];

            Assert.AreEqual(t.Date.Day, 1);
            Assert.AreEqual(t.Value, 100);
        }

        // ID自動採番テスト
        [Test]
        public void IdSerialTest()
        {
            mSw.WriteLine("FIRST_LINE");
            mSw.WriteLine("2011/1/1, 100, 10000, Desc, Memo");
            mSw.WriteLine("2011/1/2, 100, 10600, Desc, Memo");
            mSw.WriteLine("2011/1/1, 200, 10200, Desc, Memo");
            mSw.WriteLine("2011/1/2, 200, 10800, Desc, Memo");
            mSw.WriteLine("2011/1/1, 300, 10500, Desc, Memo");
            mSw.WriteLine("2011/1/2, 300, 11100, Desc, Memo");
            mSw.Close();

            mRule.OrderString = "Sort";

            mAccount.StartReading(mTempFileName, mRule);
            mAccount.ReadTransactions();
            Assert.AreEqual(6, mAccount.Transactions.Count);

            // ID自動採番。本来は CsvAccount ではなく TransactionList
            // でテストすべき。
            mAccount.Transactions.AssignSerials();

            Assert.AreEqual(0, mAccount.Transactions[0].Serial);
            Assert.AreEqual(1, mAccount.Transactions[1].Serial);
            Assert.AreEqual(2, mAccount.Transactions[2].Serial);
            Assert.AreEqual(0, mAccount.Transactions[3].Serial);
            Assert.AreEqual(1, mAccount.Transactions[4].Serial);
            Assert.AreEqual(2, mAccount.Transactions[5].Serial);
        }
    }
}

