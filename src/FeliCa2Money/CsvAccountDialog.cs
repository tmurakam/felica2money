using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FeliCa2Money
{
    public partial class CsvAccountDialog : Form
    {
        private CsvAccountManager mManager;

        public CsvAccountDialog(CsvAccountManager manager)
        {
            InitializeComponent();

            mManager = manager;
            UpdateList();
        }

        private void UpdateList()
        {
            listBox.Items.Clear();
            var names = mManager.GetNames();
            foreach (var name in names)
            {
                listBox.Items.Add(name);
            }
        }

        // 指定されたアカウントを選択状態にする
        public void SelectAccount(CsvAccount account)
        {
            if (account == null) return;

            var idx = mManager.IndexOf(account);
            listBox.SelectedIndex = idx;
        }
        
        // 選択中のアカウントを返す
        public CsvAccount SelectedAccount()
        {
            int idx = listBox.SelectedIndex;
            if (idx < 0)
            {
                return null;
            }
            return mManager.GetAt(idx);
        }

        private void OnAddAccount(object sender, EventArgs e)
        {
            var account = new CsvAccount();
            var dlg = new CsvAccountEditDialog(mManager, account);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                account = dlg.getAccount();
                if (account != null)
                {
                    mManager.AddAccount(account);
                    UpdateList();
                }
            }
        }

        private void OnModifyAccount(object sender, EventArgs e)
        {
            var idx = listBox.SelectedIndex;
            if (idx >= 0)
            {
                var account = mManager.GetAt(idx);
                var dlg = new CsvAccountEditDialog(mManager, account);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    account = dlg.getAccount();
                    if (account != null)
                    {
                        mManager.ModifyAccount(account);
                        UpdateList();
                    }
                }
            }
        }

        private void OnDeleteAccount(object sender, EventArgs e)
        {
            var idx = listBox.SelectedIndex;
            if (idx >= 0)
            {
                if (MessageBox.Show(Properties.Resources.ConfirmDeleteAccount, Properties.Resources.Confirm, 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    == DialogResult.Yes)
                {
                    CsvAccount account = mManager.GetAt(idx);
                    mManager.DeleteAccount(account);
                    UpdateList();
                }
            }
        }

        private void OnAccountUp(object sender, EventArgs e)
        {

            var idx = listBox.SelectedIndex;
            if (idx > 0)
            {
                mManager.UpAccount(idx);
                UpdateList();
                listBox.SelectedIndex = idx - 1;
            }
        }

        private void OnAccountDown(object sender, EventArgs e)
        {

            var idx = listBox.SelectedIndex;
            if (idx < mManager.Count() - 1)
            {
                mManager.DownAccount(idx);
                UpdateList();
                listBox.SelectedIndex = idx + 1;
            }
        }
    }
}
