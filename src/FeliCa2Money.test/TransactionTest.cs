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
            t.Date = new DateTime(2012, 12, 31);
            t.Serial = 567;
            t.Value = -1234;
            t.Desc = "説明";
            t.Memo = "メモ";
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
            t.Id = 12345;
            string tid = t.TransId();
            Assert.AreEqual("201212310012345", tid);
        }

        [Test]
        public void testTransIdWithoutId()
        {
            string tid = t.TransId();
            Assert.IsNotNull(tid);
            string expected = md5("2012;12;31;567;-1234;説明;メモ");
            Assert.AreEqual(expected, tid);
        }
    }
}
