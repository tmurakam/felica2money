/*
 * FeliCa2Money
 *
 * Copyright (C) 2001-2015 Takuya Murakami
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
    /// <summary>
    /// CSV変換ルール
    /// </summary>
    public class CsvRule
    {
        /// <summary>
        /// ソート順種別
        /// </summary>
        public enum SortOrderType { Ascent, Descent, Auto };

        // 各 CSV カラムの定義
        private string[] _columnDefs;

        // 各 CSV カラムのマッピング規則
        private readonly Dictionary<string,int> _columnIndex = new Dictionary<string, int>();

        public CsvRule()
        {
            IsTsv = false;
            FirstLine = null;
        }

        // プロパティ
        public string Ident { get; set; }

        public string BankId { get; set; }

        public string Name { get; set; }

        public string FirstLine { get; set; }

        /// <summary>
        /// ソート順序
        /// </summary>
        public SortOrderType SortOrder { get; private set; }

        /// <summary>
        /// ソート順序を文字列識別子で設定する
        /// </summary>
        public string OrderString
        {
            set
            {
                if (value == "Sort") {
                    SortOrder = SortOrderType.Auto;
                } else if (value == "Descent") {
                    SortOrder = SortOrderType.Descent;
                } else {
                    SortOrder = SortOrderType.Ascent;
                }
            }
        }

        /// <summary>
        /// セパレータ種別設定。"Tab" を指定すると TSV。
        /// </summary>
        public string Separator 
        {
            set
            {
                if (value == "Tab")
                {
                    IsTsv = true;
                }
                else
                {
                    IsTsv = false;
                }
            }
        }

        public bool IsTsv { get; private set; }


        /// <summary>
        /// CSV変換フォーマット文字列を設定する
        /// </summary>
        /// <param name="format">フォーマット</param>
        public void SetFormat(string format)
        {
            _columnDefs = format.Split(new Char[] { ',', '\t' });

            for (int i = 0; i < _columnDefs.Length; i++)
            {
                string key = _columnDefs[i].Trim();
                _columnDefs[i] = key;
                _columnIndex[key] = i;
            }
        }

        /// <summary>
        /// 指定したキー列に対応するカラム値を取得
        /// </summary>
        /// <param name="row">行データ</param>
        /// <param name="key">キー名</param>
        /// <returns>値</returns>
        private string GetCol(string[] row, string key)
        {
            if (!_columnIndex.ContainsKey(key))
            {
                return null;
            }
            int col = _columnIndex[key];
            return row[col];
        }

        /// <summary>
        /// 指定したカラムを取得(すべて連結) 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetMultiCol(string[] row, string key)
        {
            StringBuilder val = new StringBuilder();
            for (int i = 0; i < _columnDefs.Length; i++)
            {
                if (key == _columnDefs[i] && row[i].Length > 0)
                {
                    if (val.Length > 0)
                    {
                        val.Append(" ");
                    }
                    val.Append(row[i]);
                }
            }
            return val.ToString();
        }

        // 指定したカラムを取得 (integer)
        private int GetColInt(string[] row, string key)
        {
            string v = GetCol(row, key);
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

        /// <summary>
        /// １行解析。
        /// CSV の各カラムはすでに分解されているものとする
        /// </summary>
        /// <param name="row">行データ</param>
        /// <returns>トランザクション</returns>
        public Transaction Parse(string[] row)
        {
            Transaction t = new Transaction();

            // 日付
            string date = GetCol(row, "Date");
            if (date != null)
            {
                t.Date = CsvUtil.parseDate(date);
            }
            else {
                int year = GetColInt(row, "Year");
                int month = GetColInt(row, "Month");
                int day = GetColInt(row, "Day");

                if (year == 0 || month == 0 || day == 0) {
                    return null;
                }

                if (year < 100)
                {
                    year += 2000;
                }
                t.Date = new DateTime(year, month, day, 0, 0, 0);
            }

            // ID
            string id = GetCol(row, "Id");
            if (id != null)
            {
                try
                {
                    t.Id = GetColInt(row, "Id");
                }
                catch (FormatException)
                {
                    // just ignore : do not use ID
                }
            }

            // 金額
            t.Value = GetColInt(row, "Income");
            t.Value -= GetColInt(row, "Outgo");

            // 残高
            t.Balance = GetColInt(row, "Balance");
            
            // 適用
            t.Desc = GetMultiCol(row, "Desc");

            // 備考
            t.Memo = GetMultiCol(row, "Memo");

            // トランザクションタイプを自動設定
            t.GuessTransType(t.Value >= 0);
            
            return t;
        }
    }
}
