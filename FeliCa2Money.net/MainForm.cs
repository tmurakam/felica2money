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

        private void doConvert(Card c)
        {
            List<Transaction> list = c.ReadCard();

            if (list == null)
            {
                MessageBox.Show("カードを読むことができませんでした", "エラー");
                return;
            }
            if (list.Count == 0)
            {
                MessageBox.Show("履歴が一件もありません", "エラー");
                return;
            }

            // OFX ファイル生成
            OfxFile ofx = new OfxFile();
            ofx.WriteFile(c, list);

            // Money 起動
            ofx.Execute();
        }

        // 設定ダイアログ
        private void buttonOption_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = Properties.Settings.Default.SFCPeepPath;
           
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.SFCPeepPath = openFileDialog.FileName;
                Properties.Settings.Default.Save();
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