using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using FeliCa2Money;

namespace FeliCa2Money.test
{
    [TestFixture]
    class AgrFileTest
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
            Assert.AreEqual("CARD_XYZ1", account.accountId);
            Assert.IsTrue(account.hasBalance);
            Assert.AreEqual(-123456, account.balance);

            // 違うカード名
            account = builder.newCreditCardAccount("\"XYZZカード\", \"DUMMY\", \"654321\"");
            Assert.NotNull(account);
            Assert.AreEqual("CARD_XYZZ1", account.accountId);
            Assert.IsTrue(account.hasBalance);
            Assert.AreEqual(-654321, account.balance);

            // １番目と同じカード名
            account = builder.newCreditCardAccount("\"XYZカード\", \"DUMMY\", \"654321\"");
            Assert.NotNull(account);
            Assert.AreEqual("CARD_XYZ2", account.accountId);
            Assert.IsTrue(account.hasBalance);
            Assert.AreEqual(-654321, account.balance);
        }
    }
}
