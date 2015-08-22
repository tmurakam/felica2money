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
        private readonly List<CsvAccount> _accounts = new List<CsvAccount>();
        private readonly CsvRules _rules = new CsvRules();

        public CsvAccountManager()
        {
            LoadAccounts();
        }

        public int Count()
        {
            return _accounts.Count;
        }

        /// <summary>
        /// CSVルールを読み込む
        /// </summary>
        /// <returns></returns>
        public bool LoadAllRules()
        {
            return _rules.LoadAllRules();
        }

        /// <summary>
        /// ルールを追加する (単体テスト用)
        /// </summary>
        /// <param name="rule">ルール</param>
        public void AddRule(CsvRule rule)
        {
            _rules.Add(rule);
        }

        /// <summary>
        /// アカウント情報をユーザ設定から読み出す
        /// </summary>
        private void LoadAccounts()
        {
            _accounts.Clear();

            foreach (var line in Properties.Settings.Default.AccountInfo)
            {
                // 各行には、Ident, BranchId, AccountId, Nickname が入っているものとする
                var a = line.Split(',');

                // アカウントIDが入っていない場合は読み飛ばす (旧バージョン対応)
                if (a[2].Length == 0) continue;

                var account = new CsvAccount
                {
                    Ident = a[0],
                    BranchId = a[1],
                    AccountId = a[2]
                };
                if (a.Length > 3) // backword compat.
                {
                    account.AccountName = a[3];
                }

                _accounts.Add(account);
            }
        }

        /// <summary>
        /// 支店番号/口座番号をユーザ設定に書き戻す
        /// </summary>
        private void SaveAccountInfo()
        {
            var s = Properties.Settings.Default;

            s.AccountInfo.Clear();

            foreach (var account in _accounts) {
                var line = account.Ident;
                line += "," + account.BranchId;
                line += "," + account.AccountId;
                line += "," + account.AccountName;
                s.AccountInfo.Add(line);
            }

            s.Save();
        }

        /// <summary>
        /// アカウント追加
        /// </summary>
        public void AddAccount(CsvAccount account)
        {
            _accounts.Add(account);
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
            _accounts.Remove(account);
            SaveAccountInfo();
        }

        public void UpAccount(int index)
        {
            if (index <= 0) return; // do nothing

            var account = _accounts[index];
            _accounts.RemoveAt(index);
            _accounts.Insert(index - 1, account);
            SaveAccountInfo();
        }

        public void DownAccount(int index)
        {
            if (index >= _accounts.Count - 1) return; // do nothing

            var account = _accounts[index];
            _accounts.RemoveAt(index);
            _accounts.Insert(index + 1, account);
            SaveAccountInfo();
        }

        /// <summary>
        /// アカウント名一覧を返す
        /// </summary>
        /// <returns></returns>
        public string[] GetNames()
        {
            var names = new string[_accounts.Count];
            var i = 0;
            foreach (var account in _accounts)
            {
                var name = GetBankName(account);
                if (account.AccountName != "")
                {
                    name += " " + account.AccountName;
                }
                names[i] = name;
                i++;
            }
            return names;
        }

        private string GetBankName(CsvAccount account)
        {
            foreach (var rule in _rules) {
                if (rule.Ident == account.Ident) {
                    return rule.Name;
                }
            }
            return "金融機関不明";
        }

        public CsvAccount GetAt(int index)
        {
            return _accounts[index];
        }

        public int IndexOf(CsvAccount account)
        {
            return _accounts.IndexOf(account);
        }

        public CsvRules GetRules()
        {
            return _rules;
        }

        /// <summary>
        /// CSVアカウントを選択
        /// </summary>
        /// <param name="path">CSVファイルパス</param>
        /// <returns>CSVアカウント</returns>
        public CsvAccount SelectAccount(string path)
        {
            // CSVファイルに合致するルール⇒アカウントを探す
            var rule = FindMatchingRuleForCsv(path);
            CsvAccount account = null;
            if (rule != null)
            {
                foreach (var acc in _accounts)
                {
                    if (acc.Ident == rule.Ident)
                    {
                        account = acc;
                        break;
                    }
                }
            }

            // 資産選択ダイアログを出す
            var dlg = new CsvAccountDialog(this);
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
            rule = _rules.FindRuleWithIdent(account.Ident);
            if (rule == null)
            {
                MessageBox.Show(Properties.Resources.NoMatchingCsvRule, Properties.Resources.Error);
                return null;
            }

            account.StartReading(path, rule);
            return account;
        }

        /// <summary>
        /// マッチする CSV ルールを探す
        /// </summary>
        /// <param name="path">CSVファイルパス</param>
        /// <returns>ルール</returns>
        public CsvRule FindMatchingRuleForCsv(string path)
        {
            // TODO: とりあえず SJIS で開く (UTF-8 とかあるかも?)
            var sr = new StreamReader(path, Encoding.Default);
            var firstLine = sr.ReadLine();
            sr.Close();

            // 合致するルールを探す
            return _rules.FindRuleForFirstLine(firstLine);
        }
    }
}
