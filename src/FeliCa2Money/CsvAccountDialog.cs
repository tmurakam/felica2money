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
        private readonly CsvAccountManager _accountManager;

        public CsvAccountDialog(CsvAccountManager accountManager)
        {
            InitializeComponent();

            _accountManager = accountManager;
            UpdateList();
        }

        private void UpdateList()
        {
            listBox.Items.Clear();
            var names = _accountManager.GetNames();
            foreach (var name in names)
            {
                listBox.Items.Add(name);
            }
        }

        // 指定されたアカウントを選択状態にする
        public void SelectAccount(CsvAccount account)
        {
            if (account == null) return;

            var idx = _accountManager.IndexOf(account);
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
            return _accountManager.GetAt(idx);
        }

        private void OnAddAccount(object sender, EventArgs e)
        {
            var account = new CsvAccount();
            var dlg = new CsvAccountEditDialog(_accountManager, account);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                account = dlg.GetAccount();
                if (account != null)
                {
                    _accountManager.AddAccount(account);
                    UpdateList();
                }
            }
        }

        private void OnModifyAccount(object sender, EventArgs e)
        {
            var idx = listBox.SelectedIndex;
            if (idx >= 0)
            {
                var account = _accountManager.GetAt(idx);
                var dlg = new CsvAccountEditDialog(_accountManager, account);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    account = dlg.GetAccount();
                    if (account != null)
                    {
                        _accountManager.ModifyAccount(account);
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
                    CsvAccount account = _accountManager.GetAt(idx);
                    _accountManager.DeleteAccount(account);
                    UpdateList();
                }
            }
        }

        private void OnAccountUp(object sender, EventArgs e)
        {

            var idx = listBox.SelectedIndex;
            if (idx > 0)
            {
                _accountManager.UpAccount(idx);
                UpdateList();
                listBox.SelectedIndex = idx - 1;
            }
        }

        private void OnAccountDown(object sender, EventArgs e)
        {

            var idx = listBox.SelectedIndex;
            if (idx < _accountManager.Count() - 1)
            {
                _accountManager.DownAccount(idx);
                UpdateList();
                listBox.SelectedIndex = idx + 1;
            }
        }
    }
}
