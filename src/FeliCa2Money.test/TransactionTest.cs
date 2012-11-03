using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace FeliCa2Money.test
{
    [TestFixture]
    class TransactionTest
    {
        Transaction t;

        [SetUp]
        public void SetUp()
        {
            t = new Transaction();
            t.date = new DateTime(2012, 12, 31);
            t.serial = 567;
            t.value = -1234;
            t.desc = "説明";
            t.memo = "メモ";
        }

        // MD5計算。文字列の MD5 を計算して 16進数で返す。
        private string md5(string s)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(s));

            StringBuilder result = new StringBuilder();
            foreach (byte b in hash)
            {
                result.Append(b.ToString("x2"));
            }
            return result.ToString();
        }

        [Test]
        public void testTransIdWithId()
        {
            t.id = 12345;
            string tid = t.transId();
            Assert.AreEqual("201212310012345", tid);
        }

        [Test]
        public void testTransIdWithoutId()
        {
            string tid = t.transId();
            Assert.IsNotNull(tid);
            string expected = md5("2012;12;31;567;-1234;説明;メモ");
            Assert.AreEqual(expected, tid);
        }
    }
}
