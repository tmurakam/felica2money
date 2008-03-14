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
            Properties.Settings s = Properties.Settings.Default;

            checkIgnoreZeroTransaction.Checked = s.IgnoreZeroTransaction;
            checkManualOfxPath.Checked = s.ManualOfxPath;
            checkAutoKickOfxFile.Checked = s.AutoKickOfxFile;

            //textSfcPeepPath.Text = s.SFCPeepPath;

            int p = s.ShopAreaPriority;
            switch (p)
            {
                default:
                case Suica.AreaSuica: radioSuica.Checked = true; break;
                case Suica.AreaIcoca: radioIcoca.Checked = true; break;
                case Suica.AreaIruca: radioIruca.Checked = true; break;
            }
        }

        public void SaveProperties()
        {
            Properties.Settings s = Properties.Settings.Default;

            s.IgnoreZeroTransaction = checkIgnoreZeroTransaction.Checked;
            s.ManualOfxPath = checkManualOfxPath.Checked;
            s.AutoKickOfxFile = checkAutoKickOfxFile.Checked;
            //s.SFCPeepPath = textSfcPeepPath.Text;

            int p = Suica.AreaSuica;
            if (radioSuica.Checked) p = Suica.AreaSuica;
            else if (radioIcoca.Checked) p = Suica.AreaIcoca;
            else if (radioIruca.Checked) p = Suica.AreaIruca;
            s.ShopAreaPriority = p;

            s.Save();
        }

        /*
        private void buttonSfcPath_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = textSfcPeepPath.Text;
            openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(textSfcPeepPath.Text);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textSfcPeepPath.Text = openFileDialog.FileName;
            }
        }
        */
    }
}
