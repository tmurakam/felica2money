// -*-  Mode:C++; c-basic-offset:4; tab-width:4; indent-tabs-mode:nil -*-

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using NUnit.Framework;
using FeliCa2Money;

namespace FeliCa2Money.test
{
    class TestAccount : Account
    {
        public TestAccount()
        {
            mTransactions = new List<Transaction>();
        }

        public override void ReadCard()
        {
            // do nothing
        }
    }

    [TestFixture]
    class OfxTest
    {
        private Ofx ofx;
        private Transaction T1, T2;
        private List<Account> accounts;

        [SetUp]
        public void setUp()
        {
            ofx = new Ofx();

            accounts = new List<Account>();

            T1 = new Transaction();
            T1.date = new DateTime(2000, 1, 1);
            T1.desc = "T1";
            T1.type = TransType.Payment;
            T1.value = 1000;
            T1.balance = 10000;

            T2 = new Transaction();
            T2.date = new DateTime(2010, 12, 31);
            T2.desc = "T2";
            T2.type = TransType.Payment;
            T2.value = 2000;
            T2.balance = 12000;
        }

        [TearDown]
        public void tearDown()
        {
        }

        [Test]
        public void noEntry()
        {
            TestAccount account = new TestAccount();
            accounts.Add(account);

            Assert.Throws<InvalidOperationException>(() => ofx.genOfx(accounts));
        }

        [Test]
        public void bankOnly()
        {
            TestAccount bank = new TestAccount();
            bank.isCreditCard = false;
            bank.transactions.Add(T1);
            bank.transactions.Add(T2);
            accounts.Add(bank);

            TestAccount card = new TestAccount();
            card.isCreditCard = true;
            accounts.Add(card);

            ofx.genOfx(accounts);
            XmlDocument doc = ofx.doc;

            Assert.NotNull(doc);
            Assert.NotNull(doc.SelectSingleNode("/OFX/BANKMSGSRSV1"));
            Assert.Null(doc.SelectSingleNode("/OFX/CREDITCARDMSGSRSV1"));
        }

        [Test]
        public void cardOnly()
        {
            TestAccount bank = new TestAccount();
            bank.isCreditCard = false;
            accounts.Add(bank);

            TestAccount card = new TestAccount();
            card.isCreditCard = true;
            card.transactions.Add(T1);
            card.transactions.Add(T2);
            accounts.Add(card);

            ofx.genOfx(accounts);
            XmlDocument doc = ofx.doc;

            Assert.NotNull(doc);
            Assert.Null(doc.SelectSingleNode("/OFX/BANKMSGSRSV1"));
            Assert.NotNull(doc.SelectSingleNode("/OFX/CREDITCARDMSGSRSV1"));
        }

        [Test]
        public void normalTest()
        {
            TestAccount bank = new TestAccount();
            bank.isCreditCard = false;
            bank.transactions.Add(T1);
            accounts.Add(bank);

            TestAccount card = new TestAccount();
            card.isCreditCard = true;
            card.transactions.Add(T2);
            accounts.Add(card);

            ofx.genOfx(accounts);
            XmlDocument doc = ofx.doc;

            Assert.NotNull(doc);
            Assert.NotNull(doc.SelectSingleNode("/OFX/BANKMSGSRSV1"));
            Assert.NotNull(doc.SelectSingleNode("/OFX/CREDITCARDMSGSRSV1"));

            // 各エントリチェック
            assertNodeText(doc, "/OFX/SIGNONMSGSRSV1/SONRS/DTSERVER", "20101231000000[+9:JST]");

            XmlNode stmtrs = doc.SelectSingleNode("/OFX/BANKMSGSRSV1/STMTTRNRS/STMTRS");

            assertNodeText(stmtrs, "BANKTRANLIST/DTSTART", "20000101000000[+9:JST]");
            assertNodeText(stmtrs, "LEDGERBAL/DTASOF", "20000101000000[+9:JST]");
            assertNodeText(stmtrs, "LEDGERBAL/BALAMT", "10000");

            XmlNode ccstmtrs = doc.SelectSingleNode("/OFX/CREDITCARDMSGSRSV1/CCSTMTTRNRS/CCSTMTRS");
            assertNodeText(ccstmtrs, "BANKTRANLIST/DTSTART", "20101231000000[+9:JST]");
            assertNodeText(ccstmtrs, "LEDGERBAL/DTASOF", "20101231000000[+9:JST]");
            assertNodeText(ccstmtrs, "LEDGERBAL/BALAMT", "12000");
        }

        [Test]
        public void lastBalanceTest()
        {
            TestAccount acc = new TestAccount();
            acc.isCreditCard = false;
            accounts.Add(acc);

            Transaction t;
        
            t = new Transaction();
            t.date = new DateTime(2010, 4, 1);
            t.desc = "t1";
            t.type = TransType.Payment;
            t.value = 100;
            t.balance = 10000;
            acc.transactions.Add(t);

            t = new Transaction();
            t.date = new DateTime(2010, 4, 1);
            t.desc = "t2";
            t.type = TransType.Payment;
            t.value = 200;
            t.balance = 20000;
            acc.transactions.Add(t);

            t = new Transaction();
            t.date = new DateTime(2010, 1, 1); // 逆順
            t.desc = "t3";
            t.type = TransType.Payment;
            t.value = 300;
            t.balance = 30000;
            acc.transactions.Add(t);
        
            ofx.genOfx(accounts);
            XmlDocument doc = ofx.doc;

            // 最も新しい日付の最後の取引(t2)の値になっているかどうか確認
            XmlNode ledgerBal = doc.SelectSingleNode("/OFX/BANKMSGSRSV1/STMTTRNRS/STMTRS/LEDGERBAL");
            assertNodeText(ledgerBal, "DTASOF", "20100401000000[+9:JST]");
            assertNodeText(ledgerBal, "BALAMT", "20000");
        }

        private void assertNodeText(XmlNode node, string path, string expected)
        {
            XmlNode n = node.SelectSingleNode(path);
            Assert.NotNull(n);
            Assert.AreEqual(expected, n.InnerText);
        }
    }
}
