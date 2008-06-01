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
                0x20, // 支払
                0x01, 0x23, 0x45, // 連番
                0x0, 0x0, 0x0, 0x0, // 時刻 (2000/1/1, 0:00)
                0x0, 0x0, 0x16, 0x2e, // 金額 (5,678円)
                0x0, 0x0, 0xb2, 0x6e, // 残高 (45,678円)
            });

            FeliCa2Money.Edy c = new FeliCa2Money.Edy();
            List<Transaction> t = c.ReadCard(f);

            string d = t[0].date.ToString();
            Assert.AreEqual(0x12345, t[0].id);
            Assert.AreEqual("2000/01/01 0:00:00", t[0].date.ToString());
            Assert.AreEqual(-5678, t[0].value);
            Assert.AreEqual(45678, t[0].balance, 45678);
            Assert.AreEqual("支払 74565", t[0].desc);
        }
    }
}
