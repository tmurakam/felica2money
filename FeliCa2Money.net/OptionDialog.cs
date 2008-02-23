using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FeliCa2Money
{
    public partial class OptionDialog : Form
    {
        public OptionDialog()
        {
            InitializeComponent();

            LoadProperties();
        }

        public void LoadProperties()
        {
            checkIgnoreZeroTransaction.Checked = Properties.Settings.Default.IgnoreZeroTransaction;
            checkManualOfxPath.Checked = Properties.Settings.Default.ManualOfxPath;
            checkAutoKickOfxFile.Checked = Properties.Settings.Default.AutoKickOfxFile;
            textSfcPeepPath.Text = Properties.Settings.Default.SFCPeepPath;
        }

        public void SaveProperties()
        {
            Properties.Settings.Default.IgnoreZeroTransaction = checkIgnoreZeroTransaction.Checked;
            Properties.Settings.Default.ManualOfxPath = checkManualOfxPath.Checked;
            Properties.Settings.Default.AutoKickOfxFile = checkAutoKickOfxFile.Checked;
            Properties.Settings.Default.SFCPeepPath = textSfcPeepPath.Text;

            Properties.Settings.Default.Save();
        }

        private void buttonSfcPath_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = textSfcPeepPath.Text;
            openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(textSfcPeepPath.Text);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textSfcPeepPath.Text = openFileDialog.FileName;
            }
        }
    }
}
