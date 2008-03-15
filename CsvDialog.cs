using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FeliCa2Money
{
    public partial class CsvDialog : Form
    {
        // CSV 変換ルールセット
        private CsvRules rules;

        // 記憶した支店番号/口座番号
        private Hashtable branchIds;
        private Hashtable accountIds;

        public string BranchId
        {
            get { return textBranchId.Text; }
            set { textBranchId.Text = value; }
        }

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

            // リストボックスに変換定義名を並べる
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

        // ユーザ設定に書き戻す
        private void SaveAccountInfo()
        {
            Properties.Settings.Default.AccountInfo.Clear();

            int count = rules.Count;
            for (int i = 0; i < count; i++)
            {
                CsvRule rule = rules.GetRuleByIndex(i);
                string org = rule.Org;
                string x = rule.Org + ",";
                if (branchIds[org] != null) {
                    x += branchIds[org];
                }
                x += ",";
                if (accountIds[org] != null)
                {
                    x += accountIds[org];
                }
                Properties.Settings.Default.AccountInfo.Add(x);
            }

            Properties.Settings.Default.Save();
        }

        // ルール選択
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
            string org = rule.Org;
            if (branchIds[org] != null)
            {
                BranchId = (string)branchIds[org];
            }
            else
            {
                BranchId = "";
            }
            if (accountIds[org] != null)
            {
                AccountId = (string)accountIds[org];
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
            return rules.GetRuleByIndex(idx);
        }


        private void textBranchId_Leave(object sender, EventArgs e)
        {
            CsvRule rule = SelectedRule();
            string org = rule.Org;
            branchIds[org] = textBranchId.Text;
        }

        private void textAccountId_Leave(object sender, EventArgs e)
        {
            CsvRule rule = SelectedRule();
            string org = rule.Org;
            accountIds[org] = textAccountId.Text;
        }

        private void CsvDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveAccountInfo();
        }
    }
}
