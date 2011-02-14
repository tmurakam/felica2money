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
            AgrAccount account = builder.newBankAccount("\"BANK_NAME\", \"BRANCH_NAME\", \"ACCOUNT_ID\", \"-123456\", \"JPY\"");
            Assert.NotNull(account);
            Assert.False(account.isCreditCard);
            Assert.AreEqual("BANK_NAME", account.bankId);
            Assert.AreEqual("BRANCH_NAME", account.branchId);
            Assert.AreEqual("ACCOUNT_ID", account.accountId);
            Assert.IsTrue(account.hasBalance);
            Assert.AreEqual(-123456, account.balance);
        }

        [Test]
        public void createBankAccountWithoutBalance()
        {
            AgrAccount account = builder.newBankAccount("\"BANK_NAME\", \"BRANCH_NAME\", \"ACCOUNT_ID\"");
            Assert.NotNull(account);
            Assert.False(account.isCreditCard);
            Assert.AreEqual("BANK_NAME", account.bankId);
            Assert.AreEqual("BRANCH_NAME", account.branchId);
            Assert.AreEqual("ACCOUNT_ID", account.accountId);
            Assert.IsFalse(account.hasBalance);
        }

        [Test]
        public void createCreditCardAccounts()
        {
            AgrAccount account = builder.newCreditCardAccount("\"XYZカード\", \"DUMMY\", \"123456\"");
            Assert.NotNull(account);
            Assert.True(account.isCreditCard);
            Assert.AreEqual("CARD_XYZ1", account.accountId);
            Assert.IsTrue(account.hasBalance);
            Assert.AreEqual(-123456, account.balance);

            // 違うカード名
            account = builder.newCreditCardAccount("\"XYZZカード\", \"DUMMY\", \"654321\"");
            Assert.NotNull(account);
            Assert.True(account.isCreditCard);
            Assert.AreEqual("CARD_XYZZ1", account.accountId);
            Assert.IsTrue(account.hasBalance);
            Assert.AreEqual(-654321, account.balance);

            // １番目と同じカード名
            account = builder.newCreditCardAccount("\"XYZカード\", \"DUMMY\", \"654321\"");
            Assert.NotNull(account);
            Assert.True(account.isCreditCard);
            Assert.AreEqual("CARD_XYZ2", account.accountId);
            Assert.IsTrue(account.hasBalance);
            Assert.AreEqual(-654321, account.balance);
        }

        [Test]
        public void readTransaction()
        {
            AgrAccount account = builder.newBankAccount("\"BANK_NAME\", \"BRANCH_NAME\", \"ACCOUNT_ID\"");
            Assert.True(account.readTransaction("\"2010/1/2\", \"DESCRIPTION\", \"100\", \"JPY\", \"--\", \"\", \"123456\", \"JPY\""));
            Assert.AreEqual(1, account.transactions.Count);

            Transaction t = account.transactions[0];
            Assert.AreEqual(DateTime.Parse("2010/1/2").ToString(), t.date.ToString());
            Assert.AreEqual("DESCRIPTION", t.desc);
            Assert.AreEqual(100, t.value);
            Assert.AreEqual(123456, t.balance);
        }

        [Test]
        public void readTransactionWithoutYear()
        {
            DateTime now = DateTime.Now;

            for (int i = 1; i <= 12; i++)
            {
                AgrAccount account = builder.newBankAccount("\"BANK_NAME\", \"BRANCH_NAME\", \"ACCOUNT_ID\"");

                // 年なし(月日のみ)のフォーマットを作成
                Assert.True(account.readTransaction("\"" + i.ToString() + "/15\", \"DESCRIPTION\", \"100\", \"JPY\", \"\", \"\", \"123456\", \"JPY\""));
                Assert.AreEqual(1, account.transactions.Count);

                Transaction t = account.transactions[0];
                Assert.AreEqual(i, t.date.Month);
                Assert.AreEqual(15, t.date.Day);

                // 半年(366/2日)以上離れていないことを確認する
                TimeSpan diff = now.Subtract(t.date);
                Assert.True(-366/2 <= diff.Days);
                Assert.True(diff.Days <= 366/2);
            }
        }
    }
}
