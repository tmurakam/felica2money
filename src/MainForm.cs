/*
 * FeliCa2Money
 *
 * Copyright (C) 2001-2008 Takuya Murakami
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
            CsvCard csv = new CsvCard();
            if (!csv.LoadAllRules()) return;

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
            // AGR ファイル読み込み
            AgrFile agr = new AgrFile();

            openFileDialog.Filter = "AGRファイル|*.agr|すべてのファイル|*.*";
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;

            try
            {
                if (agr.loadFromFile(openFileDialog.FileName) == false) return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Properties.Resources.Error);
                return;
            }

            doConvert(agr.cards);
        }

        private void doReadAndConvert(Card c)
        {
            if (doRead(c))
            {
                doConvert(c);
            }
        }

        // カードを読み込む
        // カードが正常に読み取られ、１件以上有効な取引があれば、true を返す。
        private bool doRead(Card c)
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

            if (c.transactions.Count == 0)
            {
                MessageBox.Show(Properties.Resources.NoHistory, Properties.Resources.Error);
                return false;
            }
            return true;
        }

        private void doConvert(Card c)
        {
            List<Card> cards = new List<Card>();
            cards.Add(c);
            doConvert(cards);
        }

        private void doConvert(List<Card> cards)
        {
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
            OfxFile ofx;
            if (Properties.Settings.Default.OfxVer2)
            {
                ofx = new OfxFile2();
            }
            else
            {
                ofx = new OfxFile();
            }

            ofx.SetOfxFilePath(ofxFilePath);
            ofx.WriteFile(cards);

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
                String helpFile = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\Felica2Money.html";
                System.Diagnostics.Process.Start(helpFile);
            }
            catch
            {
                // do nothing
            }
        }
    }
}
