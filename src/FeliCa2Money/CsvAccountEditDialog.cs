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

// CSV 口座編集ダイアログ

using System;
using System.Collections;
using System.Windows.Forms;

namespace FeliCa2Money
{
    public partial class CsvAccountEditDialog : Form
    {
        private CsvAccountManager _accountManager;

        private readonly CsvAccount _account;

        // CSV 変換ルールセット
        private readonly CsvRules _csvRules;

        // コンストラクタ
        public CsvAccountEditDialog(CsvAccountManager manager, CsvAccount account)
        {
            InitializeComponent();

            _accountManager = manager;
            _account = account;

            _csvRules = manager.GetRules();

            textBranchId.Text = account.BranchId;
            if (textBranchId.Text == "0")
            {
                textBranchId.Text = "";
            }
            textAccountId.Text = account.AccountId;
            textAccountName.Text = account.AccountName;

            UpdateList();
        }

        private void UpdateList() 
        {
            // リストボックスにルール名をリストする
            listBox.Items.Clear();
            var names = _csvRules.Names();

            foreach (var name in names)
            {
                listBox.Items.Add(name);
            }

            // 該当する金融機関を選択状態にする
            var i = 0;
            foreach (var rule in _csvRules)
            {
                if (rule.Ident == _account.Ident)
                {
                    listBox.SelectedIndex = i;
                    break;
                }
                i++;
            }
        }

        public CsvAccount GetAccount()
        {
            var idx = listBox.SelectedIndex;
            if (idx < 0)
            {
                // 金融機関が選択されていない
                return null;
            }

            _account.Ident = _csvRules.GetAt(idx).Ident;
            _account.BranchId = textBranchId.Text;
            _account.AccountId = textAccountId.Text;
            _account.AccountName = textAccountName.Text;

            return _account;
        }

        private void OnUpdateCsvRules(object sender, EventArgs e)
        {
            var updater = new CsvRulesUpdater();

            if (updater.CheckUpdate(true))
            {
                _csvRules.LoadAllRules();
                UpdateList();
            }
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex < 0)
            {
                MessageBox.Show(Properties.Resources.RequireBankId, "エラー");
                return;
            }
            if (textAccountId.Text == "")
            {
                MessageBox.Show(Properties.Resources.RequireAccountName, "エラー");
                return;
            }

            this.DialogResult = DialogResult.OK;
            Close();
        }
    }
}
