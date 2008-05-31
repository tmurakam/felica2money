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
            byte[] d;
            d = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef };
            f.SetTestData(0x110b, d);
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
    }
}
