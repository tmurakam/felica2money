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
            rule.ident = "hoge";
            Assert.AreEqual(rule.ident, "hoge");
        }

        [Test]
        public void Order()
        {
            rule.order = "Ascent";
            Assert.AreEqual(rule.sortOrder, CsvRule.SortOrder.Ascent);
            rule.order = "Descent";
            Assert.AreEqual(rule.sortOrder, CsvRule.SortOrder.Descent);
            rule.order = "Sort";
            Assert.AreEqual(rule.sortOrder, CsvRule.SortOrder.Auto);
        }

        [Test]
        public void Separator()
        {
            rule.separator = "Tab";
            Assert.IsTrue(rule.isTSV);
            rule.separator = "Comma";
            Assert.IsFalse(rule.isTSV);
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
            t = rule.parse(splitCSV("05/9/23"));
            Assert.IsTrue(t.date.Year == 2005 && t.date.Month == 9 && t.date.Day == 23);

            t = rule.parse(splitCSV("05年9月23日"));
            Assert.IsTrue(t.date.Year == 2005 && t.date.Month == 9 && t.date.Day == 23);

            t = rule.parse(splitCSV("H17年9月23日"));
            Assert.IsTrue(t.date.Year == 2005 && t.date.Month == 9 && t.date.Day == 23);

            t = rule.parse(splitCSV("050923"));
            Assert.IsTrue(t.date.Year == 2005 && t.date.Month == 9 && t.date.Day == 23);

            t = rule.parse(splitCSV("20050923"));
            Assert.IsTrue(t.date.Year == 2005 && t.date.Month == 9 && t.date.Day == 23);
        }


        [Test]
        public void ParseYMD()
        {
            Transaction t;
            rule.SetFormat("Year,Month,Day");

            t = rule.parse(splitCSV("03, 03, 10"));
            Assert.AreEqual(t.date.Year, 2003);
            Assert.AreEqual(t.date.Month, 3);
            Assert.AreEqual(t.date.Day, 10);
        }
    }
}
