/*
 * FeliCa2Money
 *
 * Copyright (C) 2001-2008 Takuya Murakami
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 */

// CSV 変換ルールセット

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace FeliCa2Money
{
    public class CsvRules
    {
        private List<CsvRule> ruleList;

        public CsvRules()
        {
            ruleList = new List<CsvRule>();
        }

        // 定義ファイルを読み込む
        public void LoadFromFile(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlElement root = doc.DocumentElement;

            XmlNodeList list = root.GetElementsByTagName("Rule");

            for (int i = 0; i < list.Count; i++)
            {
                CsvRule rule = new CsvRule();

                foreach (XmlElement e in list[i].ChildNodes)
                {
                    string value = e.FirstChild.Value;

                    switch (e.Name)
                    {
                        case "Ident":
                            rule.Org = value;
                            break;
                        case "Name":
                            rule.Name = value;
                            break;
                        case "BankId":
                            rule.BankId = int.Parse(value);
                            break;
                        case "FirstLine":
                            rule.FirstLine = value;
                            break;
                        case "Format":
                            rule.Format = value;
                            break;
                        case "Order":
                            rule.Order = value;
                            break;
                        default:
                            // ignore
                            break;
                    }
                }

                ruleList.Add(rule);
            }
        }

        // 名前一覧を返す
        public string[] names()
        {
            int size = 0;
            foreach (CsvRule rule in ruleList)
            {
                size++;
            }

            string[] names = new string[size];
            int i = 0;
            foreach (CsvRule rule in ruleList)
            {
                names[i] = rule.Name;
                i++;
            }
            return names;
        }

        // 指定したインデックスのルールを返す
        public CsvRule GetRuleByIndex(int idx)
        {
            return ruleList[idx];
        }

        // 指定したルールのインデックスを返す
        public int IndexOf(CsvRule rule)
        {
            return ruleList.IndexOf(rule);
        }

        public CsvRule FindRule(string firstLine)
        {
            foreach (CsvRule rule in ruleList)
            {
                if (rule.FirstLine == firstLine)
                {
                    return rule;
                }
            }
            return null;
        }
    }
}
