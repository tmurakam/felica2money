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

// CSV 変換ダイアログ

using System;
using System.Collections;
using System.Windows.Forms;

namespace FeliCa2Money
{
    public partial class CsvDialog : Form
    {
        // CSV 変換ルールセット
        private CsvRules rules;

        // 支店番号/口座番号のキャッシュ
        private Hashtable branchIds;
        private Hashtable accountIds;

        // 支店番号テキストボックス
        public string BranchId
        {
            get { return textBranchId.Text; }
            set { textBranchId.Text = value; }
        }

        // 口座番号テキストボックス
        public string AccountId
        {
            get { return textAccountId.Text; }
            set { textAccountId.Text = value; }
        }

        // コンストラクタ
        public CsvDialog(CsvRules r)
        {
            InitializeComponent();

            rules = r;

            listBox.Items.Clear();
            string[] names = rules.names();

            // リストボックスにルール名をリストする
            foreach (string name in names)
            {
                listBox.Items.Add(name);
            }

            // 支店番号/口座番号をユーザ設定から読み出す
            branchIds = new Hashtable();
            accountIds = new Hashtable();
            LoadAccountInfo();
        }

        // 支店番号/口座番号をユーザ設定から読み出す
        private void LoadAccountInfo()
        {
            foreach (string x in Properties.Settings.Default.AccountInfo)
            {
                // 各行には、Ident,BranchId,AccountId が入っているものとする
                string[] a = x.Split(new char[] { ',' });
                branchIds[a[0]] = a[1];
                accountIds[a[0]] = a[2];
            }
        }

        // 支店番号/口座番号をユーザ設定に書き戻す
        private void SaveAccountInfo()
        {
            Properties.Settings s = Properties.Settings.Default;

            s.AccountInfo.Clear();

            int count = rules.Count;
            for (int i = 0; i < count; i++)
            {
                CsvRule rule = rules.GetAt(i);
                string org = rule.Ident;
                string x = rule.Ident + ",";
                if (branchIds[org] != null) {
                    x += branchIds[org];
                }
                x += ",";
                if (accountIds[org] != null)
                {
                    x += accountIds[org];
                }
                s.AccountInfo.Add(x);
            }

            s.Save();
        }

        // 引数で指定したルールを選択状態にする
        public void SelectRule(CsvRule selRule)
        {
            if (selRule == null) return;

            int idx = rules.IndexOf(selRule);
            listBox.SelectedIndex = idx;
        }

        // 選択アイテムが変更されたときの処理
        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 支店番号、口座番号をテキストボックスに設定する
            CsvRule rule = SelectedRule();
            string ident = rule.Ident;
            if (branchIds[ident] != null)
            {
                BranchId = (string)branchIds[ident];
            }
            else
            {
                BranchId = "";
            }
            if (accountIds[ident] != null)
            {
                AccountId = (string)accountIds[ident];
            }
            else
            {
                AccountId = "";
            }
        }

        // 選択中のルールを返す
        public CsvRule SelectedRule()
        {
            int idx = listBox.SelectedIndex;
            if (idx < 0)
            {
                return null;
            }
            return rules.GetAt(idx);
        }


        private void textBranchId_Leave(object sender, EventArgs e)
        {
            CsvRule rule = SelectedRule();
            string org = rule.Ident;
            branchIds[org] = textBranchId.Text;
        }

        private void textAccountId_Leave(object sender, EventArgs e)
        {
            CsvRule rule = SelectedRule();
            string org = rule.Ident;
            accountIds[org] = textAccountId.Text;
        }

        private void CsvDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveAccountInfo();
        }
    }
}
