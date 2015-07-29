// -*-  Mode:C++; c-basic-offset:4; tab-width:4; indent-tabs-mode:nil -*-
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

// CSV ユーティリティ

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FeliCa2Money
{
    /// <summary>
    /// CSVユーティリティ
    /// </summary>
    class CsvUtil
    {
        // コンストラクタなし
        private CsvUtil()
        {
        }

        /// <summary>
        /// CSVフィールド分割
        /// </summary>
        /// <param name="line">CSV行</param>
        /// <param name="isTsv">区切りがタブの場合は true, カンマの場合は false</param>
        /// <returns>分割されたカラム</returns>
        public static string[] SplitCsv(string line, bool isTsv)
        {
            var fields = new List<string>();
            Regex regCsv;

            if (isTsv)
            {
                regCsv = new Regex("([^\\t]*)\\t");
                line = line + "\t";
            }
            else
            {
                regCsv = new Regex("\\s*(\"(?:[^\"]|\"\")*\"|[^,]*)\\s*,", RegexOptions.None);
                line = line + ",";
            }

            var m = regCsv.Match(line);
            var count = 0;
            while (m.Success)
            {
                var field = m.Groups[1].Value;

                // 前後の空白を削除
                field = field.Trim();

                // ダブルクォートを抜く
                if (field.StartsWith("\"") && field.EndsWith("\""))
                {
                    field = field.Substring(1, field.Length - 2);
                }
                // "" を " に変換
                field = field.Replace("\"\"", "\"");

                // もう一度前後の空白を削除
                field = field.Trim();

                fields.Add(field);
                count++;
                m = m.NextMatch();
            }

            return fields.ToArray() as string[];
        }

        /// <summary>
        /// 日付文字列をヒューリスティックに解析する。
        /// </summary>
        /// <remarks>
        /// 以下のようなフォーマットをサポートする。<br/>
        ///   yyyy年mm月dd日
        ///   Hyy年mm月dd日   (和暦、平成のみ対応)
        ///   yyyy/mm/dd
        ///   yy/mm/dd (年が下２桁)
        ///   mm/dd/yyyy
        ///   yyyymmdd
        ///   yymmdd
        ///   mmddyyyy
        /// なお、以下の形式はサポートしない
        ///   mm/dd/yy (yy/mm/dd と区別できない)
        /// </remarks>
        /// <param name="date">日付文字列</param>
        /// <returns>DateTime型の日付。時分秒は0。</returns>
        public static DateTime ParseDate(string date)
        {
            int year, month, day;

            // 年月日で区切られている場合
            var split = date.Split(new char[] { '年', '月', '日' });
            if (split.Length < 3)
            {
                // '/' などで区切られている場合
                split = date.Split(new Char[] { '/', '.', ' ', '-' });
            }

            if (split.Length >= 3)
            {
                // 和暦の処理
                //   (三井住友銀行など)
                if (split[0].StartsWith("H"))
                {
                    year = int.Parse(split[0].Substring(1));
                    year += 1988;
                    month = int.Parse(split[1]);
                    day = int.Parse(split[2]);
                } else {
                    var n0 = int.Parse(split[0]);
                    var n1 = int.Parse(split[1]);
                    var n2 = int.Parse(split[2]);

                    if (n2 > 1970)
                    {
                        // mm/dd/yyyy 形式
                        year = n2;
                        month = n0;
                        day = n1;
                    }
                    else
                    {
                        // yyyy/mm/dd 形式
                        year = n0;
                        month = n1;
                        day = n2;
                    }
                }
            }
            else
            {
                date = split[0];

                if (date.Length != 6 && date.Length != 8)
                {
                    // パース不可能
                    // TBD
                    throw new FormatException(Properties.Resources.CantParseDateStr + " (" + date + ")");
                }

                var n = int.Parse(date);
                year = n / 10000;
                month = (n / 100) % 100;
                day = n % 100;

                if (date.Length == 8 && year < 1900)
                {
                    // mmddyyyy
                    year = n % 10000;
                    month = n / 1000000;
                    day = (n / 10000) % 100;
                }
            }

            if (year < 100) {
                year += 2000;
            }
            return new DateTime(year, month, day, 0, 0, 0);
        }
    }
}
