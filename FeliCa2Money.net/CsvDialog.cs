using System;
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
        private CsvRules rules;

        public CsvDialog(CsvRules r)
        {
            InitializeComponent();

            rules = r;

            listBox.Items.Clear();
            string[] names = rules.names();

            foreach (string name in names)
            {
                listBox.Items.Add(name);
            }
        }

        private void CsvDialog_Load(object sender, EventArgs e)
        {
        }

        public void SelectRule(CsvRule selRule)
        {
            if (selRule != null)
            {
                int idx = rules.IndexOf(selRule);
                listBox.SelectedIndex = idx;
            }
        }

        public CsvRule SelectedRule()
        {
            int idx = listBox.SelectedIndex;
            return rules.GetRuleByIndex(idx);
        }

        public int BranchId
        {
            get {
                try
                {
                    return int.Parse(textBranchId.Text);
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
        }

        public string AccountId
        {
            get { return (textAccountId.Text); }
        }
    }
}
