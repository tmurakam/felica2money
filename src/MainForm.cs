/*
 * FeliCa2Money
 *
 * Copyright (C) 2001-2010 Takuya Murakami
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

namespace FeliCa2Money
{
    public partial class MainForm : Form
    {
	    private const String HELP_URL = "http://felica2money.tmurakam.org/manual.html";

        public MainForm()
        {
            InitializeComponent();

            Properties.Settings s = Properties.Settings.Default;
            if (s.IsFirstRun)
            {
                s.Upgrade();
                s.IsFirstRun = false;
                s.Save();
            }
        }

        // コマンドライン処理
        private void onLoad(object sender, EventArgs e)
        {
            string[] argv = System.Environment.GetCommandLineArgs();
            if (argv.Length == 2)
            {
                string filepath = argv[1];
                if (filepath.EndsWith(".agr") || filepath.EndsWith(".AGR"))
                {
                    processAgrFile(filepath);
                }
            }
        }

        private void buttonQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void buttonEdy_Click(object sender, EventArgs e)
        {
            using (Edy edy = new Edy())
            {
                doReadAndConvert(edy);
            }
        }

        private void buttonSuica_Click(object sender, EventArgs e)
        {
            using (Suica suica = new Suica())
            {
                doReadAndConvert(suica);
            }
        }

        private void buttonNanaco_Click(object sender, EventArgs e)
        {
            using(Nanaco nanaco = new Nanaco())
            {
                doReadAndConvert(nanaco);
            }
        }

        private void buttonWaon_Click(object sender, EventArgs e)
        {
            using (Waon waon = new Waon())
            {
                doReadAndConvert(waon);
            }
        }

        private void buttonCSV_Click(object sender, EventArgs e)
        {
            CsvAccount csv = new CsvAccount();
            if (!csv.LoadAllRules()) return;

            openFileDialog.DefaultExt = "csv";
            openFileDialog.Filter = "CSVファイル|*.csv|すべてのファイル|*.*";
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;

            try
            {
                if (csv.OpenFile(openFileDialog.FileName) == false) return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Properties.Resources.Error);
                return;
            }
             
            doReadAndConvert(csv);
            csv.Close();
        }

        private void buttonAGR_Click(object sender, EventArgs e)
        {
            openFileDialog.DefaultExt = "agr";
            openFileDialog.Filter = "AGRファイル|*.agr|すべてのファイル|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                processAgrFile(openFileDialog.FileName);
            }
        }

        // AGRファイル処理
        private void processAgrFile(string filepath)
        {
            AgrFile agr = new AgrFile();

            try
            {
                if (agr.loadFromFile(filepath) == false)
                {
                    MessageBox.Show("フォーマットエラー");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Properties.Resources.Error);
                return;
            }
            doConvert(agr.accounts);
        }

        private void doReadAndConvert(Account c)
        {
            if (doRead(c))
            {
                doConvert(c);
            }
        }

        // カードを読み込む
        // 正常に読み込んだら true を返す
        private bool doRead(Account c)
        {
            try
            {
                c.ReadCard();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Properties.Resources.Error);
                return false;
            }
            if (c.transactions == null)
            {
                MessageBox.Show(Properties.Resources.CardReadError, Properties.Resources.Error);
                return false;
            }

            // 無効な取引を削除する
            c.transactions.RemoveAll(Transaction.isInvalid);

            // 0円の取引を削除する
            if (Properties.Settings.Default.IgnoreZeroTransaction)
            {
                c.transactions.RemoveAll(Transaction.isZeroTransaction);
            }

            return true;
        }

        private void doConvert(Account c)
        {
            List<Account> accounts = new List<Account>();
            accounts.Add(c);
            doConvert(accounts);
        }

        private void doConvert(List<Account> accounts)
        {
            // 明細件数をチェック
            int count = 0;
            foreach (Account account in accounts)
            {
                count += account.transactions.Count;
            }
            if (count == 0)
            {
                MessageBox.Show(Properties.Resources.NoHistory, Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // OFX ファイルパス指定
            String ofxFilePath;
            if (Properties.Settings.Default.ManualOfxPath)
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ofxFilePath = saveFileDialog.FileName;
                }
                else
                {
                    // do not save
                    return;
                }
            }
            else
            {
                ofxFilePath = System.IO.Path.GetTempPath() + "FeliCa2Money.ofx";
            }

            // OFX ファイル生成
            OfxFileBase ofx;
            if (Properties.Settings.Default.OfxVer2)
            {
                ofx = new OfxFileV2();
            }
            else
            {
                ofx = new OfxFileV1();
            }

            ofx.SetOfxFilePath(ofxFilePath);
            ofx.WriteFile(accounts);

            // Money 起動
            if (Properties.Settings.Default.AutoKickOfxFile)
            {
                ofx.Execute();
            }
        }

        // 設定ダイアログ
        private void buttonOption_Click(object sender, EventArgs e)
        {
            OptionDialog dlg = new OptionDialog();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                dlg.SaveProperties();
            }
        }

        private void buttonManual_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(HELP_URL);

	            //String helpFile = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\Felica2Money.html";
                //System.Diagnostics.Process.Start(helpFile);
            }
            catch
            {
                // do nothing
            }
        }
    }
}
