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

            Properties.Settings.Default.Upgrade();
        }

        private void buttonQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void buttonEdy_Click(object sender, EventArgs e)
        {
            doConvert(new Edy());
        }

        private void buttonSuica_Click(object sender, EventArgs e)
        {
            doConvert(new Suica());
        }

        private void buttonNanaco_Click(object sender, EventArgs e)
        {
            doConvert(new Nanaco());
        }

	    private void doConvert(Card c)
        {
            List<Transaction> list;

	        try
	        {
		        list = c.ReadCard();
	        }
	        catch (Exception ex)
	        {
	    	    MessageBox.Show(ex.Message, "エラー");
		        return;
	        }

            if (list == null)
            {
                MessageBox.Show("カードを読むことができませんでした", "エラー");
                return;
            }

            // 0円の取引を削除する
            if (Properties.Settings.Default.IgnoreZeroTransaction)
            {
                list.RemoveAll(Transaction.isZeroTransaction);
            }

            if (list.Count == 0)
            {
                MessageBox.Show("履歴が一件もありません", "エラー");
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
            OfxFile ofx = new OfxFile(); 
            ofx.SetOfxFilePath(ofxFilePath);
            ofx.WriteFile(c, list);

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
                System.Diagnostics.Process.Start("Felica2Money.html");
            }
            catch
            {
                // do nothing
            }
        }

    }
}