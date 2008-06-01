using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using FeliCa2Money;
using FelicaLib;

namespace FeliCa2Money.test
{
    [TestFixture]
    public class EdyTest
    {
        DummyFelica f;

        [SetUp]
        public void Setup()
        {
            f = new DummyFelica();
            f.SetSystemCode((int)SystemCode.Edy);

            // set card id
            f.SetTestData(0x110b, new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef });
        }

        [TearDown]
        public void TearDown()
        {
            f = null;
        }

        [Test]
        public void analyzeCardId()
        {
            FeliCa2Money.Edy c = new FeliCa2Money.Edy();
            c.analyzeCardId(f);

            Assert.AreEqual(c.AccountId, "456789ABCDEF0123");
        }

        [Test]
        public void ReadCard()
        {
            // set dummy data
            f.SetTestData(0x170f, new byte[] {
                0x04, // ギフト
                0x01, 0x23, 0x47, // 連番
                0x0, 0x0, 0x0, 0x0, // 時刻 (2000/1/1, 0:00)
                0x0, 0x0, 0x03, 0xe8, // 金額 (1000円)
                0x0, 0x0, 0xba, 0x3e, // 残高 (47,678円)

                0x02, // チャージ
                0x01, 0x23, 0x46, // 連番
                0x0, 0x0, 0x0, 0x0, // 時刻 (2000/1/1, 0:00)
                0x0, 0x0, 0x03, 0xe8, // 金額 (1000円)
                0x0, 0x0, 0xb6, 0x56, // 残高 (46,678円)

                0x20, // 支払
                0x01, 0x23, 0x45, // 連番
                0x0, 0x0, 0x0, 0x0, // 時刻 (2000/1/1, 0:00)
                0x0, 0x0, 0x16, 0x2e, // 金額 (5,678円)
                0x0, 0x0, 0xb2, 0x6e, // 残高 (45,678円)
            });

            FeliCa2Money.Edy c = new FeliCa2Money.Edy();
            List<Transaction> tlist = c.ReadCard(f);
            Assert.AreEqual(3, tlist.Count);
            Transaction t;

            // 支払
            t = tlist[0];
            //string d = t[0].date.ToString();
            Assert.AreEqual(0x12345, t.id);
            Assert.AreEqual("2000/01/01 0:00:00", t.date.ToString());
            Assert.AreEqual(-5678, t.value);
            Assert.AreEqual(45678, t.balance);
            Assert.AreEqual("支払 74565", t.desc);

            // チャージ
            t = tlist[1];
            Assert.AreEqual(0x12346, t.id);
            Assert.AreEqual("2000/01/01 0:00:00", t.date.ToString());
            Assert.AreEqual(1000, t.value);
            Assert.AreEqual(46678, t.balance);
            Assert.AreEqual("Edyチャージ", t.desc);

            // ギフト
            t = tlist[2];
            Assert.AreEqual(0x12347, t.id);
            Assert.AreEqual("2000/01/01 0:00:00", t.date.ToString());
            Assert.AreEqual(1000, t.value);
            Assert.AreEqual(47678, t.balance);
            Assert.AreEqual("Edyギフト", t.desc);
        }
    }
}
