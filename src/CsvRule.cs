/*
 * FeliCa2Money
 *
 * Copyright (C) 2001-2011 Takuya Murakami
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
        public enum SortOrder { Ascent, Descent, Auto };

        private string mIdent;    // 識別子(組織名)
        private string mBankId;      // 銀行ID
        private string mName;     // 銀行名
        private string mFirstLine = null; // １行目
        private SortOrder mSortOrder; // ソートオーダー
        private bool mIsTSV = false; // TSV かどうか

        // 各 CSV カラムのマッピング規則
        private Hashtable colHash = new Hashtable();

        // プロパティ
        public string ident
        {
            get { return mIdent; }
            set { mIdent = value; }
        }
        public string bankId {
            get { return mBankId; }
            set { mBankId = value; }
        }
        public string name {
            get { return mName; }
            set { mName = value; }
        }
        public string firstLine
        {
            get { return mFirstLine; }
            set { mFirstLine = value; }
        }
        public string order
        {
            set
            {
                if (value == "Sort") {
                    mSortOrder = SortOrder.Auto;
                } else if (value == "Descent") {
                    mSortOrder = SortOrder.Descent;
                } else {
                    mSortOrder = SortOrder.Ascent;
                }
            }
        }
        public SortOrder sortOrder
        {
            get { return mSortOrder; }
        }
        public string separator 
        {
            set
            {
                if (value == "Tab")
                {
                    mIsTSV = true;
                }
                else
                {
                    mIsTSV = false;
                }
            }
        }
        public bool isTSV
        {
            get { return mIsTSV; }
        }
            
        
        public string format
        {
            set { SetFormat(value); }
        }
        
        // CSV 変換フォーマット文字列解析
        public void SetFormat(string format)
        {
            string[] cols = format.Split(new Char[] { ',', '\t' });

            for (int i = 0; i < cols.Length; i++)
            {
                colHash[cols[i].Trim()] = i;
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

        // 指定したカラムを取得 (integer)
        private int getColInt(string[] row, string key)
        {
            string v = getCol(row, key);
            if (v == null) return 0;

            // 空フィールドの場合は 0 を返す
            if (v == "") return 0;

            // 区切り文字を抜く
            v = v.Replace(",", "");

            // 先頭に '\' があるときは抜く
            if (v.StartsWith("\\"))
            {
                v = v.Substring(1);
            }

            // 小数点が含まれることを考慮して、double でパース
            return (int)double.Parse(v);
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
                t.id = Transaction.UNASSIGNED_ID;
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

            // 年月日で区切られている場合
            string[] split = date.Split(new char[] { '年', '月', '日' });
            if (split.Length < 3)
            {
                // '/' などで区切られている場合
                split = date.Split(new Char[] { '/', '.', ' ' });
            }

            if (split.Length >= 3)
            {
                // 和暦の処理
                //   (三井住友銀行など)
                if (split[0].StartsWith("H"))
                {
                    year = int.Parse(split[0].Substring(1));
                    year += 1988;
                } else {
                    year = int.Parse(split[0]);
                }
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
                    throw new Exception(Properties.Resources.CantParseDateStr + " (" + date + ")");
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
