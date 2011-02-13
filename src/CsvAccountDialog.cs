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
            updateList();
        }

        private void updateList()
        {
            listBox.Items.Clear();
            string[] names = mManager.getNames();
            foreach (string name in names)
            {
                listBox.Items.Add(name);
            }
        }

        // 指定されたアカウントを選択状態にする
        public void SelectAccount(CsvAccount account)
        {
            if (account == null) return;

            int idx = mManager.IndexOf(account);
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

        private void onAddAccount(object sender, EventArgs e)
        {
            CsvAccount account = new CsvAccount();
            CsvAccountEditDialog dlg = new CsvAccountEditDialog(mManager, account);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                account = dlg.getAccount();
                if (account != null)
                {
                    mManager.AddAccount(account);
                    updateList();
                }
            }
        }

        private void onModifyAccount(object sender, EventArgs e)
        {
            int idx = listBox.SelectedIndex;
            if (idx >= 0)
            {
                CsvAccount account = mManager.GetAt(idx);
                CsvAccountEditDialog dlg = new CsvAccountEditDialog(mManager, account);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    account = dlg.getAccount();
                    if (account != null)
                    {
                        mManager.ModifyAccount(account);
                        updateList();
                    }
                }
            }
        }

        private void onDeleteAccount(object sender, EventArgs e)
        {
            int idx = listBox.SelectedIndex;
            if (idx >= 0)
            {
                if (MessageBox.Show(Properties.Resources.ConfirmDeleteAccount, Properties.Resources.Confirm, 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    == DialogResult.Yes)
                {
                    CsvAccount account = mManager.GetAt(idx);
                    mManager.DeleteAccount(account);
                    updateList();
                }
            }
        }

        private void onAccountUp(object sender, EventArgs e)
        {

            int idx = listBox.SelectedIndex;
            if (idx > 0)
            {
                mManager.UpAccount(idx);
                updateList();
                listBox.SelectedIndex = idx - 1;
            }
        }

        private void onAccountDown(object sender, EventArgs e)
        {

            int idx = listBox.SelectedIndex;
            if (idx < mManager.Count() - 1)
            {
                mManager.DownAccount(idx);
                updateList();
                listBox.SelectedIndex = idx + 1;
            }
        }
    }
}
