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

            // リストボックスにルール名をリストする
            listBox.Items.Clear();
            string[] names = mRules.names();

            foreach (string name in names)
            {
                listBox.Items.Add(name);
            }

            textBranchId.Text = account.branchId;
            textAccountId.Text = account.accountId;
            textAccountName.Text = account.accountName;

            // 該当する金融機関を選択状態にする
            int count = mRules.Count;
            for (int i = 0; i < count; i++)
            {
                if (mRules.GetAt(i).ident == account.ident)
                {
                    listBox.SelectedIndex = i;
                    break;
                }
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
                MessageBox.Show(Properties.Resources.UpdateCompleted);
            }
        }
    }
}
