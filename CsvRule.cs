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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace FeliCa2Money
{
    public class CsvRule
    {
        private string ident;    // 識別子(組織名)
        private int bankId;      // 銀行ID
        private string name;     // 銀行名
        private string firstLine = null; // １行目
        private bool isAscent;    // 昇順かどうか

        // 各 CSV カラムのマッピング規則
        private Hashtable colHash = new Hashtable();

        // シリアル番号採番用
        private DateTime prevDate;
        private int idSerial;

        // プロパティ
        public string Ident
        {
            get { return ident; }
            set { ident = value; }
        }
        public int BankId {
            get { return bankId; }
            set { bankId = value; }
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
            set { SetFormat(value); }
        }
        
        // CSV 変換フォーマット文字列解析
        public void SetFormat(string format)
        {
            string[] cols = format.Split(new Char[] { ',' });

            for (int i = 0; i < cols.Length; i++)
            {
                colHash[cols[i].Trim()] = i;
            }
        }

        // シリアル番号リセット
        public void Reset()
        {
            prevDate = new DateTime(1900, 1, 1, 0, 0, 0);
            idSerial = 0;
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

        // 指定したカラムを取得 (integer)
        private int getColInt(string[] row, string key)
        {
            string v = getCol(row, key);
            if (v == null) return 0;

            try
            {
                return int.Parse(v);
            }
            catch
            {
                return int.Parse(v, System.Globalization.NumberStyles.AllowThousands);
            }
        }

        // １行解析
        // CSV の各カラムはすでに分解されているものとする
        public Transaction parse(string[] row)
        {
            Transaction t = new Transaction();

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

            // ID
            string id = getCol(row, "Id");
            if (id != null)
            {
                t.id = getColInt(row, "Id");
            }
            else
            {
                // 日付毎に ID を振る
                if (t.date == prevDate)
                {
                    idSerial++;
                }
                else
                {
                    prevDate = t.date;
                    idSerial = 0;
                }
                t.id = idSerial;
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

            // トランザクションタイプを自動設定
            t.GuessTransType(t.value >= 0);
            
            return t;
        }

        // 日付文字列の解析
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
                    throw new Exception("日付文字列解析失敗 (" + date + ")");
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
