// -*-  Mode:C++; c-basic-offset:4; tab-width:4; indent-tabs-mode:nil -*-

using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using FeliCa2Money;

namespace FeliCa2Money.test
{
    [TestFixture]
    public class CsvRuleTest
    {
        private CsvRule rule;

        [SetUp]
        public void Setup()
        {
            rule = new CsvRule();
        }

        [TearDown]
        public void TearDown()
        {
            rule = null;
        }

        [Test]
        public void Ident()
        {
            rule.Ident = "hoge";
            Assert.AreEqual(rule.Ident, "hoge");
        }

        [Test]
        public void Order()
        {
            rule.OrderString = "Ascent";
            Assert.AreEqual(rule.SortOrder, CsvRule.SortOrderType.Ascent);
            rule.OrderString = "Descent";
            Assert.AreEqual(rule.SortOrder, CsvRule.SortOrderType.Descent);
            rule.OrderString = "Sort";
            Assert.AreEqual(rule.SortOrder, CsvRule.SortOrderType.Auto);
        }

        [Test]
        public void Separator()
        {
            rule.Separator = "Tab";
            Assert.IsTrue(rule.IsTsv);
            rule.Separator = "Comma";
            Assert.IsFalse(rule.IsTsv);
        }

        private string[] splitCSV(string x)
        {
            return x.Split(new Char[] {',', '\t'});
        }

        [Test]
        public void ParseDate()
        {
            Transaction t;
            rule.SetFormat("Date");
            t = rule.Parse(splitCSV("2005/09/23"));
            Assert.IsTrue(t.date.Year == 2005 && t.date.Month == 9 && t.date.Day == 23);

            t = rule.Parse(splitCSV("05/9/23"));
            Assert.IsTrue(t.date.Year == 2005 && t.date.Month == 9 && t.date.Day == 23);

            t = rule.Parse(splitCSV("05年9月23日"));
            Assert.IsTrue(t.date.Year == 2005 && t.date.Month == 9 && t.date.Day == 23);

            t = rule.Parse(splitCSV("H17年9月23日"));
            Assert.IsTrue(t.date.Year == 2005 && t.date.Month == 9 && t.date.Day == 23);

            t = rule.Parse(splitCSV("H17/09/23"));
            Assert.IsTrue(t.date.Year == 2005 && t.date.Month == 9 && t.date.Day == 23);

            t = rule.Parse(splitCSV("050923"));
            Assert.IsTrue(t.date.Year == 2005 && t.date.Month == 9 && t.date.Day == 23);

            t = rule.Parse(splitCSV("20050923"));
            Assert.IsTrue(t.date.Year == 2005 && t.date.Month == 9 && t.date.Day == 23);

            t = rule.Parse(splitCSV("9/23/2010"));
            Assert.IsTrue(t.date.Year == 2010 && t.date.Month == 9 && t.date.Day == 23);

            t = rule.Parse(splitCSV("09232005"));
            Assert.IsTrue(t.date.Year == 2005 && t.date.Month == 9 && t.date.Day == 23);
        }


        [Test]
        public void ParseYMD()
        {
            Transaction t;
            rule.SetFormat("Year,Month,Day");

            t = rule.Parse(splitCSV("03, 03, 10"));
            Assert.AreEqual(t.date.Year, 2003);
            Assert.AreEqual(t.date.Month, 3);
            Assert.AreEqual(t.date.Day, 10);
        }

        [Test]
        public void testMultiColumn()
        {
            Transaction t;
            rule.SetFormat("Date,Desc,Memo,Desc,Memo");

            t = rule.Parse(splitCSV("20110101,A,B,C,D"));
            Assert.AreEqual(t.desc, "A C");
            Assert.AreEqual(t.memo, "B D");
        }
    }
}
