// -*-  Mode:C++; c-basic-offset:4; tab-width:4; indent-tabs-mode:nil -*-

using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using FeliCa2Money;

namespace FeliCa2Money.test
{
    [TestFixture]
    class AgrAccountTest
    {
        private AgrAccount.Builder builder;

        [SetUp]
        public void setUp()
        {
            builder = new AgrAccount.Builder();
        }

        [TearDown]
        public void tearDown()
        {
            builder = null;
        }

        [Test]
        public void createBankAccount()
        {
            AgrAccount account = builder.NewBankAccount("\"BANK_NAME\", \"BRANCH_NAME\", \"ACCOUNT_ID\", \"-123456\", \"JPY\"");
            Assert.NotNull(account);
            Assert.False(account.IsCreditCard);
            Assert.AreEqual("BANK_NAME", account.BankId);
            Assert.AreEqual("BRANCH_NAME", account.BranchId);
            Assert.AreEqual("ACCOUNT_ID", account.AccountId);
            Assert.IsTrue(account.HasBalance);
            Assert.AreEqual(-123456, account.Balance);
        }

        [Test]
        public void createBankAccountWithoutBalance()
        {
            AgrAccount account = builder.NewBankAccount("\"BANK_NAME\", \"BRANCH_NAME\", \"ACCOUNT_ID\"");
            Assert.NotNull(account);
            Assert.False(account.IsCreditCard);
            Assert.AreEqual("BANK_NAME", account.BankId);
            Assert.AreEqual("BRANCH_NAME", account.BranchId);
            Assert.AreEqual("ACCOUNT_ID", account.AccountId);
            Assert.IsFalse(account.HasBalance);
        }

        [Test]
        public void createCreditCardAccounts()
        {
            AgrAccount account = builder.NewCreditCardAccount("\"XYZカード\", \"DUMMY\", \"123456\"");
            Assert.NotNull(account);
            Assert.True(account.IsCreditCard);
            Assert.AreEqual("CARD_XYZ1", account.AccountId);
            Assert.IsTrue(account.HasBalance);
            Assert.AreEqual(-123456, account.Balance);

            // 違うカード名
            account = builder.NewCreditCardAccount("\"XYZZカード\", \"DUMMY\", \"654321\"");
            Assert.NotNull(account);
            Assert.True(account.IsCreditCard);
            Assert.AreEqual("CARD_XYZZ1", account.AccountId);
            Assert.IsTrue(account.HasBalance);
            Assert.AreEqual(-654321, account.Balance);

            // １番目と同じカード名
            account = builder.NewCreditCardAccount("\"XYZカード\", \"DUMMY\", \"654321\"");
            Assert.NotNull(account);
            Assert.True(account.IsCreditCard);
            Assert.AreEqual("CARD_XYZ2", account.AccountId);
            Assert.IsTrue(account.HasBalance);
            Assert.AreEqual(-654321, account.Balance);
        }

        [Test]
        public void readTransaction()
        {
            AgrAccount account = builder.NewBankAccount("\"BANK_NAME\", \"BRANCH_NAME\", \"ACCOUNT_ID\"");
            Assert.True(account.ReadTransaction("\"2010/1/2\", \"DESCRIPTION\", \"100\", \"JPY\", \"--\", \"\", \"123456\", \"JPY\""));
            Assert.AreEqual(1, account.Transactions.Count);

            Transaction t = account.Transactions[0];
            Assert.AreEqual(DateTime.Parse("2010/1/2").ToString(), t.Date.ToString());
            Assert.AreEqual("DESCRIPTION", t.Desc);
            Assert.AreEqual(100, t.Value);
            Assert.AreEqual(123456, t.Balance);
        }

        [Test]
        public void readTransactionWithoutYear()
        {
            DateTime now = DateTime.Now;

            for (int i = 1; i <= 12; i++)
            {
                AgrAccount account = builder.NewBankAccount("\"BANK_NAME\", \"BRANCH_NAME\", \"ACCOUNT_ID\"");

                // 年なし(月日のみ)のフォーマットを作成
                Assert.True(account.ReadTransaction("\"" + i.ToString() + "/15\", \"DESCRIPTION\", \"100\", \"JPY\", \"\", \"\", \"123456\", \"JPY\""));
                Assert.AreEqual(1, account.Transactions.Count);

                Transaction t = account.Transactions[0];
                Assert.AreEqual(i, t.Date.Month);
                Assert.AreEqual(15, t.Date.Day);

                // 半年以上離れていないことを確認する
                TimeSpan diff = now.Subtract(t.Date);
                Assert.True(-366/2 <= diff.Days);
                Assert.True(diff.Days <= 366/2);
            }
        }

        [Test]
        public void readTransactionWithoutDay()
        {
            AgrAccount account = builder.NewBankAccount("\"BANK_NAME\", \"BRANCH_NAME\", \"ACCOUNT_ID\"");

            // 日なし(年月のみ)のフォーマットを作成
            Assert.True(account.ReadTransaction("\"2011/3\", \"DESCRIPTION\", \"100\", \"JPY\", \"\", \"\", \"123456\", \"JPY\""));
            Assert.AreEqual(1, account.Transactions.Count);

            Transaction t = account.Transactions[0];
            Assert.AreEqual(2011, t.Date.Year);
            Assert.AreEqual(3, t.Date.Month);
            Assert.AreEqual(1, t.Date.Day);
        }
    }
}
