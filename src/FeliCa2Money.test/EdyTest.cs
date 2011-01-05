// -*-  Mode:C++; c-basic-offset:4; tab-width:4; indent-tabs-mode:t -*-

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

            Assert.AreEqual(c.accountId, "456789ABCDEF0123");
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

        [Test]
        public void ReadCard2()
        {
            f.SetTestDataFromStrings(0x170f, new string[] {
                "170F:0000 20 00 00 67 18 04 23 ED 00 00 03 E8 00 00 2A 3E",
                "170F:0001 20 00 00 66 17 FA 20 61 00 00 07 D0 00 00 2E 26",
                "170F:0002 20 00 00 65 17 E8 04 86 00 00 03 E8 00 00 35 F6",
                "170F:0003 20 00 00 64 17 E2 B4 CA 00 00 86 C4 00 00 39 DE",
                "170F:0004 04 00 00 63 17 DB 08 97 00 00 1F 80 00 00 C0 A2",
                "170F:0005 20 00 00 62 17 D9 3B 97 00 00 07 08 00 00 A1 22"
            });

            //2008/06/01 02:33:17 支払い   金額:1000  残高:10814 連番:103
            //2008/05/27 02:18:09 支払い   金額:2000  残高:11814 連番:102
            //2008/05/18 00:19:18 支払い   金額:1000  残高:13814 連番:101
            //2008/05/15 12:51:22 支払い   金額:34500 残高:14814 連番:100
            //2008/05/11 18:48:55 ギフト   金額:8064  残高:49314 連番:99
            //2008/05/10 22:26:31 支払い   金額:1800  残高:41250 連番:98

            FeliCa2Money.Edy c = new FeliCa2Money.Edy();
            List<Transaction> tlist = c.ReadCard(f);
            Assert.AreEqual(6, tlist.Count);
            Transaction t;

            t = tlist[0];
            Assert.AreEqual(98, t.id);
            Assert.AreEqual("2008/05/10 22:26:31", t.date.ToString());
            Assert.AreEqual(-1800, t.value);
            Assert.AreEqual(41250, t.balance);
            Assert.AreEqual("支払 98", t.desc);

            t = tlist[1];
            Assert.AreEqual(99, t.id);
            Assert.AreEqual("2008/05/11 18:48:55", t.date.ToString());
            Assert.AreEqual(8064, t.value);
            Assert.AreEqual(49314, t.balance);
            Assert.AreEqual("Edyギフト", t.desc);
        }
    }
}
