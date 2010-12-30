using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Security;
using System.Security.Principal;
using System.Diagnostics;

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
            checkOfxVer2.Checked = s.OfxVer2;

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
            s.OfxVer2 = checkOfxVer2.Checked;
            //s.SFCPeepPath = textSfcPeepPath.Text;

            int p = Suica.AreaSuica;
            if (radioSuica.Checked) p = Suica.AreaSuica;
            else if (radioIcoca.Checked) p = Suica.AreaIcoca;
            else if (radioIruca.Checked) p = Suica.AreaIruca;
            s.ShopAreaPriority = p;

            s.Save();
        }

        private void buttonCsvRulesUpdate_Click(object sender, EventArgs e)
        {
            if (CsvRules.DownloadRule())
            {
                MessageBox.Show(Properties.Resources.UpdateCompleted);
            }
        }

        // AGR関連付け
        private void onAgrAssociateClick(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("AGRファイルをFeliCa2Moneyに関連付けますか？", "確認",
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                string cmdline = "\"" + Application.ExecutablePath + "\" %1";
                string filetype = Application.ProductName;
                string description = "Agurippa電子明細";
                string iconpath = Application.ExecutablePath;

                // ファイルタイプ登録
                Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(".agr");
                regkey.SetValue("", filetype);
                regkey.Close();

                Microsoft.Win32.RegistryKey shellkey = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(filetype);
                shellkey.SetValue("", description);

                // 動詞登録
                shellkey = shellkey.CreateSubKey("shell\\open");
                shellkey.SetValue("", "FeliCa2Moneyで開く(&O)");

                // コマンドライン登録
                shellkey = shellkey.CreateSubKey("command");
                shellkey.SetValue("", cmdline);
                shellkey.Close();

                // アイコン登録
                Microsoft.Win32.RegistryKey iconkey = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(filetype + "\\DefaultIcon");
                iconkey.SetValue("", iconpath + ",0");
                iconkey.Close();
            }
            catch (UnauthorizedAccessException ex)
            {
                runasAdmin();
            }
        }

        private void onAgrUnAssociateClick(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("AGRファイルの関連付けを解除しますか？", "確認",
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            string filetype = Application.ProductName;

            try
            {
                Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(".agr");
                Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(filetype);
            }
            catch (UnauthorizedAccessException ex)
            {
                runasAdmin();
            }
            catch (ArgumentException ex)
            {
                // サブキーなし。無視。
            }
        }

        private void runasAdmin()
        {
            DialogResult result = MessageBox.Show("管理者権限がありません。管理者権限で起動しなおしますので、再度実行してください。", "確認", MessageBoxButtons.OKCancel,
                MessageBoxIcon.Exclamation);
            if (result != DialogResult.OK)
            {
                return;
            }

            // 管理者権限で起動しなおす
            ProcessStartInfo si = new ProcessStartInfo();
            si.UseShellExecute = true;
            si.WorkingDirectory = Environment.CurrentDirectory;
            si.FileName = Application.ExecutablePath;
            si.Verb = "runas";

            try
            {
                Process p = Process.Start(si);
            }
            catch
            {
                return;
            }
            Application.Exit();
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
