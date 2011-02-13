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
using System.Runtime.InteropServices;

namespace FeliCa2Money
{
    public partial class OptionDialog : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

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

            // Show shield icons
            const int BCM_SETSHIELD = 0x160c;
            SendMessage(buttonAssoc.Handle, BCM_SETSHIELD, IntPtr.Zero, new IntPtr(1));
            SendMessage(buttonDeAssoc.Handle, BCM_SETSHIELD, IntPtr.Zero, new IntPtr(1));
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
            catch (UnauthorizedAccessException)
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
            catch (UnauthorizedAccessException)
            {
                runasAdmin();
            }
            catch (ArgumentException)
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
