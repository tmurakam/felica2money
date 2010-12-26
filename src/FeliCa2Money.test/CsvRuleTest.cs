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
            rule.Order = "Ascent";
            Assert.AreEqual(rule.SortOrder, CsvRule.SortAscent);
            rule.Order = "Descent";
            Assert.AreEqual(rule.SortOrder, CsvRule.SortDescent);
            rule.Order = "Sort";
            Assert.AreEqual(rule.SortOrder, CsvRule.SortAuto);
        }

        [Test]
        public void Separator()
        {
            rule.Separator = "Tab";
            Assert.IsTrue(rule.IsTSV);
            rule.Separator = "Comma";
            Assert.IsFalse(rule.IsTSV);
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
