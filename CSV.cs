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

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FeliCa2Money
{
    class CsvFile
    {
        public void test()
        {
            TextFieldParser parser = new TextFieldParser("test.csv",
                System.Text.Encoding.GetEncoding("Shift_JIS"));
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");

            while (!parser.EndOfData)
            {
                string[] row = parser.ReadFields();
                Transaction t = rule.parse(row);
            }
        }
    }

    class CsvRule
    {
        private string ident; // 銀行ID
        private string name;  // 銀行名
        private string firstLine; // １行目
        private bool isAscent;    // 昇順かどうか

        private int col_id, col_date, col_year, col_month, col_day,
            col_income, col_outgo, col_balance, col_desc, col_memo;

        
        // フォーマット解析
        public SetFormat(string format)
        {
            // TBD
            
        }
        
        // データ解析
        public Transaction parse(string[] row)
        {
            Transaction t = new Transaction();

            // TODO
            // quote を削除する
            
            // ID
            if (col_id >= 0) {
                t.id = Int.Parse(row[col_id]);
            }

            // 日付
            if (col_date >= 0) {
                t.date = parseDate(row[col_date]);
            }
            else {
                int year, month, day;

                if (col_year >= 0) {
                    year = int.Parse(row[col_year]);
                    if (year < 100) {
                        year += 2000;
                    }
                }
                if (col_month >= 0) {
                    month = int.Parse(row[col_month]);
                }
                if (col_day >= 0) {
                    day = int.Parse(row[col_day]);
                }
                t.date = new DateTime(year, month, day, 0, 0, 0);
            }

            // 金額
            if (col_income >= 0) {
                t.value = int.Parse(row[col_income]);
            }
            if (col_outgo >= 0) {
                t.value -= int.Parse(row[col_outgo]);
            }

            // 残高
            if (col_balance >= 0) {
                t.balance = int.Parse(row[col_balance]);
            }
            
            // 適用
            if (col_desc >= 0) {
                t.desc = row[col_desc];
            }
            // 備考
            if (col_memo >= 0) {
                t.memo = row[col_memo];
            }

            t.GuessTransType(t.value >= 0);
            
            return t;
        }

        private DateTime parseDate(string date)
        {
            // 日付の区切り文字を抜く
            date.Replace("/", "");
            date.Replace(".", "");

            if (date.Length != 6 && date.Length != 8) {
                // パース不可能
                return null;
            }

            int n = int.Parse(date);
            int year = n / 1000;
            if (year < 100) {
                year += 2000;
            }
            int month = (n / 100) % 100;
            int day = n % 100;

            return new DateTime(year, month, day, 0, 0, 0);
        }
    }

}
