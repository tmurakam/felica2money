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

// CSV 取り込み処理

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
    /// CSVアカウント
    /// </summary>
    public class CsvAccount : Account
    {
        private StreamReader _reader;
        private CsvRule _rule;

        /// <summary>
        /// CSVファイル読み込みを開始する
        /// </summary>
        /// <param name="path">CSVファイルパス</param>
        /// <param name="rule">CSVルール</param>
        public void StartReading(string path, CsvRule rule)
        {
            _rule = rule;

            // 銀行IDなどを設定
            //mIdent = _rule.ident;
            BankId = _rule.BankId;
            //_branchId = branchId;
            //_accountId = accountId;

            _reader = new StreamReader(path, System.Text.Encoding.Default);

        }

        // firstLine まで読み飛ばす
        private void SkipToFirstLine()
        {
            if (_rule.FirstLine == null) return;

            string line;
            while ((line = _reader.ReadLine()) != null)
            {
                if (line == _rule.FirstLine) return;
            }

            // 先頭行なし
            throw new CsvReadException(Properties.Resources.NoCsvHeaderLine);
        }

        /// <summary>
        /// CSVデータ読み込み
        /// </summary>
        public override void ReadTransactions()
        {
            SkipToFirstLine();

            var transactions = new TransactionList();
            string line;
            bool hasFormatError = false;

            while ((line = _reader.ReadLine()) != null)
            {
                // CSV カラム分割
                var row = SplitCsv(line);
                if (row.Length <= 1) continue; // ad hoc...

                // パース
                try
                {
                    var t = _rule.Parse(row);
                    transactions.Add(t);
                }
                catch (FormatException ex)
                {
                    // ignore transaction

                    // MessageBox.Show(ex.Message, Properties.Resources.Error);
                    hasFormatError = true;
                }  
            }

            if (transactions.Count == 0 && hasFormatError)
            {
                // フォーマットエラー例外をスロー
                throw new CsvReadException(Properties.Resources.CsvFormatError);
            }

            // ソート処理
            switch (_rule.SortOrder)
            {
                default:
                case CsvRule.SortOrderType.Ascent:
                    break;

                case CsvRule.SortOrderType.Descent:
                    transactions.Reverse();
                    break;

                case CsvRule.SortOrderType.Auto:
                    transactions.Sort(CompareByDate);
                    break;
            }

            Transactions = transactions;
        }

        /// <summary>
        /// CSVファイルクローズ
        /// </summary>
        public void Close()
        {
            if (_reader != null)
            {
                _reader.Close();
                _reader = null;
            }
        }

        // CSV のフィールド分割
        private string[] SplitCsv(string line)
        {
            return CsvUtil.SplitCsv(line, _rule.IsTsv);
        }

        private static int CompareByDate(Transaction x, Transaction y)
        {
            return x.Date.CompareTo(y.Date);
        }
    }
}
