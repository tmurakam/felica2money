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

                // アカウントIDが入っていない場合は読み飛ばす (旧バージョン対応)
                if (a[2].Length == 0) continue;

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
        /// アカウント追加
        /// </summary>
        public void AddAccount(CsvAccount account)
        {
            mAccounts.Add(account);
            SaveAccountInfo();
        }

        /// <summary>
        /// アカウント変更
        /// 注意: account は、すでに存在するアカウントである必要がある
        /// </summary>
        /// <param name="account"></param>
        public void ModifyAccount(CsvAccount account)
        {
            SaveAccountInfo();
        }

        /// <summary>
        /// アカウント削除
        /// 注意: account は、すでに存在するアカウントである必要がある
        /// </summary>
        public void DeleteAccount(CsvAccount account)
        {
            mAccounts.Remove(account);
            SaveAccountInfo();
        }

        public void UpAccount(int index)
        {
            if (index <= 0) return; // do nothing

            CsvAccount account = mAccounts[index];
            mAccounts.RemoveAt(index);
            mAccounts.Insert(index - 1, account);
            SaveAccountInfo();
        }

        public void DownAccount(int index)
        {
            if (index >= mAccounts.Count - 1) return; // do nothing

            CsvAccount account = mAccounts[index];
            mAccounts.RemoveAt(index);
            mAccounts.Insert(index + 1, account);
            SaveAccountInfo();
        }

        /// <summary>
        /// アカウント名一覧を返す
        /// </summary>
        /// <returns></returns>
        public string[] getNames()
        {
            string[] names = new string[mAccounts.Count];
            int i = 0;
            foreach (CsvAccount account in mAccounts)
            {
                string name = getBankName(account);
                if (account.accountName != "")
                {
                    name += " " + account.accountName;
                }
                names[i] = name;
                i++;
            }
            return names;
        }

        private string getBankName(CsvAccount account)
        {
            int count = mRules.Count;
            for (int i = 0; i < count; i++) {
                CsvRule rule = mRules.GetAt(i);
                if (rule.ident == account.ident) {
                    return rule.name;
                }
            }
            return "金融機関不明";
        }

        public CsvAccount GetAt(int index)
        {
            return mAccounts[index];
        }

        public int IndexOf(CsvAccount account)
        {
            return mAccounts.IndexOf(account);
        }

        public CsvRules getRules()
        {
            return mRules;
        }

        /// <summary>
        /// CSVアカウントを選択
        /// </summary>
        /// <param name="path">CSVファイルパス</param>
        /// <returns>CSVアカウント</returns>
        public CsvAccount SelectAccount(string path)
        {
            // CSVファイルに合致するルール⇒アカウントを探す
            CsvRule rule = findMatchingRuleForCsv(path);
            CsvAccount account = null;
            if (rule != null)
            {
                foreach (CsvAccount acc in mAccounts)
                {
                    if (acc.ident == rule.ident)
                    {
                        account = acc;
                        break;
                    }
                }
            }

            // 資産選択ダイアログを出す
            CsvAccountDialog dlg = new CsvAccountDialog(this);
            dlg.SelectAccount(account);
            if (dlg.ShowDialog() == DialogResult.Cancel)
            {
                return null;
            }

            // 選択されたアカウントを取り出す
            account = dlg.SelectedAccount();
            if (account == null)
            {
                MessageBox.Show(Properties.Resources.NoCsvAccountSelected, Properties.Resources.Error);
                return null;
            }

            // アカウントに対応するルールを選択する
            rule = mRules.FindRuleWithIdent(account.ident);
            if (rule == null)
            {
                MessageBox.Show(Properties.Resources.NoMatchingCsvRule, Properties.Resources.Error);
                return null;
            }

            account.startReading(path, rule);
            return account;
        }

        /// <summary>
        /// マッチする CSV ルールを探す
        /// </summary>
        /// <param name="path">CSVファイルパス</param>
        /// <returns>ルール</returns>
        public CsvRule findMatchingRuleForCsv(string path)
        {
            // TODO: とりあえず SJIS で開く (UTF-8 とかあるかも?)
            StreamReader sr = new StreamReader(path, System.Text.Encoding.Default);
            string firstLine = sr.ReadLine();
            sr.Close();

            // 合致するルールを探す
            return mRules.FindRuleForFirstLine(firstLine);
        }
    }
}
