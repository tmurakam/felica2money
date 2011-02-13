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
    /// CSVアカウントマネージャ
    /// </summary>
    public class CsvAccountManager
    {
        private List<CsvAccount> mAccounts = new List<CsvAccount>();
        private CsvRules mRules = new CsvRules();

        public CsvAccountManager()
        {
            LoadAccounts();
        }

        public int Count()
        {
            return mAccounts.Count;
        }

        /// <summary>
        /// CSVルールを読み込む
        /// </summary>
        /// <returns></returns>
        public bool LoadAllRules()
        {
            return mRules.LoadAllRules();
        }

        /// <summary>
        /// ルールを追加する (単体テスト用)
        /// </summary>
        /// <param name="rule">ルール</param>
        public void addRule(CsvRule rule)
        {
            mRules.Add(rule);
        }

        /// <summary>
        /// アカウント情報をユーザ設定から読み出す
        /// </summary>
        private void LoadAccounts()
        {
            mAccounts.Clear();

            foreach (string line in Properties.Settings.Default.AccountInfo)
            {
                // 各行には、Ident, BranchId, AccountId, Nickname が入っているものとする
                string[] a = line.Split(new char[] { ',' });

                CsvAccount account = new CsvAccount();
                account.ident = a[0];
                account.branchId = a[1];
                account.accountId = a[2];
                if (a.Length > 3) // backword compat.
                {
                    account.accountName = a[3];
                }

                mAccounts.Add(account);
            }
        }

        /// <summary>
        /// アカウント追加
        /// </summary>
        public void AddAccount(CsvAccount account)
        {
            mAccounts.Add(account);
            SaveAccountInfo();
        }

        /// <summary>
        /// 支店番号/口座番号をユーザ設定に書き戻す
        /// </summary>
        private void SaveAccountInfo()
        {
            Properties.Settings s = Properties.Settings.Default;

            s.AccountInfo.Clear();

            foreach (CsvAccount account in mAccounts) {
                string line = account.ident;
                line += "," + account.branchId;
                line += "," + account.accountId;
                line += "," + account.accountName;
                s.AccountInfo.Add(line);
            }

            s.Save();
        }

        /// <summary>
        /// CSVファイルをオープン
        /// </summary>
        /// <param name="path">CSVファイルパス</param>
        /// <returns></returns>
        public CsvAccount OpenFile(string path)
        {
            // 合致するルールを探す
            CsvRule rule = findMatchingRule(path);

            CsvRuleDialog dlg = new CsvRuleDialog(mRules);

            // ルール/口座番号などを選択
            dlg.SelectRule(rule);
            if (dlg.ShowDialog() == DialogResult.Cancel)
            {
                return null;
            }

            // 選択されたルールを取り出す
            rule = dlg.SelectedRule();
            if (rule == null)
            {
                MessageBox.Show(Properties.Resources.NoCsvRuleSelected, Properties.Resources.Error);
                return null;
            }

            CsvAccount account = new CsvAccount();
            account.startReading(path, rule);

            return account;
        }

        /// <summary>
        /// マッチする CSV ルールを探す
        /// </summary>
        /// <param name="path">CSVファイルパス</param>
        /// <returns>ルール</returns>
        public CsvRule findMatchingRule(string path)
        {
            // TODO: とりあえず SJIS で開く (UTF-8 とかあるかも?)
            StreamReader sr = new StreamReader(path, System.Text.Encoding.Default);
            string firstLine = sr.ReadLine();
            sr.Close();

            // 合致するルールを探す
            return mRules.FindRule(firstLine);
        }
    }
}
