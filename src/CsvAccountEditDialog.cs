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
        private CsvAccountManager mAccountManager;

        private CsvAccount mAccount;

        // CSV 変換ルールセット
        private CsvRules mRules;

        // コンストラクタ
        public CsvAccountEditDialog(CsvAccountManager manager, CsvAccount account)
        {
            InitializeComponent();

            mAccountManager = manager;
            mAccount = account;

            mRules = manager.getRules();

            textBranchId.Text = account.branchId;
            if (textBranchId.Text == "0")
            {
                textBranchId.Text = "";
            }
            textAccountId.Text = account.accountId;
            textAccountName.Text = account.accountName;

            updateList();
        }

        private void updateList() 
        {
            // リストボックスにルール名をリストする
            listBox.Items.Clear();
            string[] names = mRules.names();

            foreach (string name in names)
            {
                listBox.Items.Add(name);
            }

            // 該当する金融機関を選択状態にする
            int i = 0;
            for (CsvRule rule in mRules)
            {
                if (rule.ident == mAccount.ident)
                {
                    listBox.SelectedIndex = i;
                    break;
                }
                i++;
            }
        }

        public CsvAccount getAccount()
        {
            int idx = listBox.SelectedIndex;
            if (idx < 0)
            {
                // 金融機関が選択されていない
                return null;
            }

            mAccount.ident = mRules.GetAt(idx).ident;
            mAccount.branchId = textBranchId.Text;
            mAccount.accountId = textAccountId.Text;
            mAccount.accountName = textAccountName.Text;

            return mAccount;
        }

        private void onUpdateCsvRules(object sender, EventArgs e)
        {
            if (CsvRules.DownloadRule())
            {
                mRules.LoadAllRules();
                updateList();
            }
        }

        private void onOkClick(object sender, EventArgs e)
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
