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

            Assert.False(mAgrFile.LoadFromFile(mTempFileName));
        }

        [Test]
        public void invalidHeader()
        {
            mSw.WriteLine("\"あぐりっぱ\",\"1.1\"");
            mSw.Close();

            Assert.False(mAgrFile.LoadFromFile(mTempFileName));
        }

        [Test]
        public void emptyFile()
        {
            writeHeader();
            mSw.Close();

            Assert.True(mAgrFile.LoadFromFile(mTempFileName));
            Assert.AreEqual(0, mAgrFile.Accounts.Count);
        }

        [Test]
        public void onlyBilling()
        {
            writeHeader();
            writeBillingAccount();
            mSw.Close();

            Assert.True(mAgrFile.LoadFromFile(mTempFileName));
            Assert.AreEqual(0, mAgrFile.Accounts.Count);
        }

        [Test]
        public void invalidBank()
        {
            writeHeader();

            // １行目の定義行がない
            mSw.WriteLine("<START_CP_XXX_ORD>");
            mSw.WriteLine("<END_CP_XXX_ORD>");

            mSw.Close();

            Assert.True(mAgrFile.LoadFromFile(mTempFileName));
            Assert.AreEqual(0, mAgrFile.Accounts.Count);
        }

        [Test]
        public void emptyBank()
        {
            writeHeader();

            mSw.WriteLine("<START_CP_XXX_ORD>");
            mSw.WriteLine("\"ABC銀行\", \"XYZ支店\", \"01234567\", \"1000\", \"JPY\"");
            mSw.WriteLine("<END_CP_XXX_ORD>");
            mSw.Close();

            Assert.True(mAgrFile.LoadFromFile(mTempFileName));
            Assert.AreEqual(1, mAgrFile.Accounts.Count);

            Account account = mAgrFile.Accounts[0];
            Assert.False(account.IsCreditCard);
            Assert.AreEqual("ABC銀行", account.BankId);
            Assert.AreEqual("XYZ支店", account.BranchId);
            Assert.AreEqual("01234567", account.AccountId);
            Assert.True(account.HasBalance);
            Assert.AreEqual(1000, account.Balance);
            Assert.IsEmpty(account.Transactions);
        }

        [Test]
        public void emptyCard()
        {
            writeHeader();

            mSw.WriteLine("<START_CP_XXX_PAY>");
            mSw.WriteLine("\"ABCカード\", \"\", \"1000\"");
            mSw.WriteLine("<END_CP_XXX_ORD>");
            mSw.Close();

            Assert.True(mAgrFile.LoadFromFile(mTempFileName));
            Assert.AreEqual(1, mAgrFile.Accounts.Count);

            Account account = mAgrFile.Accounts[0];
            Assert.True(account.IsCreditCard);
            Assert.IsEmpty(account.BankId);
            Assert.AreEqual("0", account.BranchId);
            Assert.AreEqual("CARD_ABC1", account.AccountId);
            Assert.True(account.HasBalance);
            Assert.AreEqual(-1000, account.Balance);
            Assert.IsEmpty(account.Transactions);
        }

        [Test]
        public void bankOnly()
        {
            writeHeader();
            writeBankAccount();
            mSw.Close();

            Assert.True(mAgrFile.LoadFromFile(mTempFileName));
            Assert.AreEqual(1, mAgrFile.Accounts.Count);

            Account account = mAgrFile.Accounts[0];
            Assert.AreEqual(2, account.Transactions.Count);
        }

        // internal functions
        private void writeHeader()
        {
            mSw.WriteLine("\"あぐりっぱ\",\"1.0\"");
            mSw.WriteLine("<START_HEAD>");
            mSw.WriteLine("\"全アカウント数\",\"100\"");
            mSw.WriteLine("<END_HEAD>");
            mSw.WriteLine();
        }

        private void writeBillingAccount()
        {
            mSw.WriteLine("<START_CP_XXX_BILL>");
            mSw.WriteLine("\"X月分\", \"\", \"\", \"\", \"カード会社指定\"");
            mSw.WriteLine("\"\",\"\",\"\",\"\",\"\"");
            mSw.WriteLine("<END_CP_XXX_BILL>");
            mSw.WriteLine();
        }

        private void writeBankAccount()
        {
            mSw.WriteLine("<START_CP_XXX_ORD>");
            mSw.WriteLine("\"ABC銀行\", \"XYZ支店\", \"01234567\", \"1000\", \"JPY\"");
            mSw.WriteLine("\"1970/1/1\", \"給料\", \"300000\", \"JPY\", \"--\", \"JPY\", \"300000\", \"JPY\"");
            mSw.WriteLine("\"1/2\", \"ATM\", \"*\", \"\", \"50000\", \"JPY\", \"250000\", \"JPY\"");

            // 以下は無効な行
            mSw.WriteLine("\"--\", \"ATM\", \"*\", \"\", \"50000\", \"JPY\", \"250000\", \"JPY\""); // 日付なし
            mSw.WriteLine("\"2010/1/3\", \"ATM\", \"*\", \"\", \"--\", \"JPY\", \"250000\", \"JPY\""); // 入出金なし
            mSw.WriteLine("\"2010/1/3\", \"ATM\", \"*\", \"\", \"1000\", \"JPY\""); // 残高なし
            mSw.WriteLine("<END_CP_XXX_ORD");
        }
    }
}
