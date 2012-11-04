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
        private StreamReader mSr;
        private CsvRule mRule;

        /// <summary>
        /// CSVファイル読み込みを開始する
        /// </summary>
        /// <param name="path">CSVファイルパス</param>
        /// <param name="rule">CSVルール</param>
        public void startReading(string path, CsvRule rule)
        {
            mRule = rule;

            // 銀行IDなどを設定
            //mIdent = mRule.ident;
            mBankId = mRule.bankId;
            //mBranchId = branchId;
            //mAccountId = accountId;

            mSr = new StreamReader(path, System.Text.Encoding.Default);

            // firstLine まで読み飛ばす
            if (mRule.firstLine != null)
            {
                string line;
                while ((line = mSr.ReadLine()) != null)
                {
                    if (line == mRule.firstLine) break;
                }
            }
        }

        /// <summary>
        /// CSVデータ読み込み
        /// </summary>
        public override void ReadTransactions()
        {
            TransactionList transactions = new TransactionList();
            string line;

            while ((line = mSr.ReadLine()) != null)
            {
                // CSV カラム分割
                string[] row = SplitCsv(line);
                if (row.Length <= 1) continue; // ad hoc...

                // パース
                try
                {
                    Transaction t = mRule.parse(row);
                    transactions.Add(t);
                }
                catch (FormatException ex)
                {
                    // ignore transaction

                    // MessageBox.Show(ex.Message, Properties.Resources.Error);
                }  
            }

            // ソート処理
            switch (mRule.sortOrder)
            {
                default:
                case CsvRule.SortOrder.Ascent:
                    break;

                case CsvRule.SortOrder.Descent:
                    transactions.Reverse();
                    break;

                case CsvRule.SortOrder.Auto:
                    transactions.list.Sort(compareByDate);
                    break;
            }

            mTransactions = transactions;
        }

        /// <summary>
        /// CSVファイルクローズ
        /// </summary>
        public void Close()
        {
            if (mSr != null)
            {
                mSr.Close();
                mSr = null;
            }
        }

        // CSV のフィールド分割
        private string[] SplitCsv(string line)
        {
            return CsvUtil.SplitCsv(line, mRule.isTSV);
        }

        private static int compareByDate(Transaction x, Transaction y)
        {
            return x.date.CompareTo(y.date);
        }
    }
}
