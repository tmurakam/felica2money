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

// CSV 変換ルール

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace FeliCa2Money
{
    class CsvRules
    {
        private List<CsvRule> ruleList;

        // 定義ファイルを読み込む
        public void LoadFromFile(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlElement root = doc.DocumentElement;

            XmlNodeList list = root.GetElementsByTagName("Rule");

            ruleList = new List<CsvRule>();
            for (int i = 0; i < list.Count; i++)
            {
                CsvRule rule = new CsvRule();

                foreach (XmlElement e in list[i].ChildNodes)
                {
                    string value = e.FirstChild.Value;

                    switch (e.Name)
                    {
                        case "BankId":
                            rule.Ident = value;
                            break;
                        case "Name":
                            rule.Name = value;
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
        
    class CsvRule
    {
        private int ident; // 銀行ID
        private string name;  // 銀行名
        private string firstLine; // １行目
        private bool isAscent;    // 昇順かどうか

        private System.Collections.Hashtable colHash = new System.Collections.Hashtable(); // カラムのマッピング


        public string Ident {
            get { return ident.ToString(); }
            set { ident = int.Parse(value); }
        }
        public string Name {
            get { return name; }
            set { name = value; }
        }
        public string FirstLine
        {
            get { return firstLine; }
            set { firstLine = value; }
        }
        public string Order
        {
            set
            {
                if (value == "Descent") isAscent = false;
                else isAscent = true;
            }
        }
        public bool IsAscent
        {
            get { return isAscent; }
        }
        public string Format
        {
            set
            {
                SetFormat(value);
            }
        }
        
        // フォーマット解析
        public void SetFormat(string format)
        {
            string[] cols = format.Split(new Char[] { ',' });

            for (int i = 0; i < cols.Length; i++)
            {
                colHash[cols[i]] = i;
            }
        }

        // 指定したカラムを取得
        private string getCol(string[] row, string key)
        {
            if (colHash[key] == null)
            {
                return null;
            }
            int col = (int)colHash[key];
            return row[col];
        }

        private int getColInt(string[] row, string key)
        {
            string v = getCol(row, key);
            if (v == null) return 0;

            //return int.Parse(v, System.Globalization.NumberStyles.AllowThousands);
            return int.Parse(v);
        }

        // データ解析
        public Transaction parse(string[] row)
        {
            Transaction t = new Transaction();

            // TODO
            // quote を削除する
            
            // ID
            t.id = getColInt(row, "Id");

            // 日付
            string date = getCol(row, "Date");
            if (date != null)
            {
                t.date = parseDate(date);
            }
            else {
                int year = getColInt(row, "Year");
                int month = getColInt(row, "Month");
                int day = getColInt(row, "Day");

                if (year == 0 || month == 0 || day == 0) {
                    return null;
                }

                if (year < 100)
                {
                    year += 2000;
                }
                t.date = new DateTime(year, month, day, 0, 0, 0);
            }

            // 金額
            t.value = getColInt(row, "Income");
            t.value -= getColInt(row, "Outgo");

            // 残高
            t.balance = getColInt(row, "Balance");
            
            // 適用
            t.desc = getCol(row, "Desc");

            // 備考
            t.memo = getCol(row, "Memo");

            t.GuessTransType(t.value >= 0);
            
            return t;
        }

        private DateTime parseDate(string date)
        {
            int year, month, day;

            // '/' で区切られている場合
            string[] split = date.Split(new Char[] { '/', '.', ' ' });
            if (split.Length >= 3)
            {
                year = int.Parse(split[0]);
                month = int.Parse(split[1]);
                day = int.Parse(split[2]);
            }
            else
            {
                date = split[0];

                if (date.Length != 6 && date.Length != 8)
                {
                    // パース不可能
                    // TBD
                    throw new Exception("不明なフォーマット");
                }

                int n = int.Parse(date);
                year = n / 10000;
                month = (n / 100) % 100;
                day = n % 100;
            }

            if (year < 100) {
                year += 2000;
            }
            return new DateTime(year, month, day, 0, 0, 0);
        }
    }

}
