using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Xml;

namespace FeliCa2Money
{
    class CsvRulesUpdater
    {
        /// <summary>
        /// マスタCSVルール定義ファイルの URL
        /// </summary>
        private const String CSV_MASTER_RULE_URL = "https://github.com/tmurakam/felica2money/raw/master/defs/CsvRules.xml";

        /// <summary>
        /// 定義ファイルダウンロードを確認する
        /// </summary>
        /// <returns></returns>
        public bool ConfirmDownloadRule()
        {
            if (MessageBox.Show(Properties.Resources.QueryCsvRuleDownload, Properties.Resources.Confirm,
                MessageBoxButtons.OKCancel) != DialogResult.OK) return false;

            return DownloadRule();
        }

        /// <summary>
        /// アップデートチェックを行う
        /// </summary>
        /// <returns>アップデートされた場合は true</returns>
        public bool CheckUpdate()
        {
            CsvRules rules = new CsvRules();
            rules.LoadAllRules();

            String remoteVersion = GetRecentMasterVersion();
            if (rules.MasterVersion == null || remoteVersion.CompareTo(rules.MasterVersion) > 0)
            {
                DialogResult result = MessageBox.Show("新しいCSV定義ファイルがあります。更新しますか？", "確認", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return DownloadRule();
                }
            }
            return false;
        }

        /// <summary>
        /// 定義ファイルをダウンロードする
        /// </summary>
        /// <returns></returns>
        public bool DownloadRule()
        {
            string path = CsvRules.getRulesPath() + "\\" + CsvRules.CSV_MASTER_RULE_FILENAME;

            WebClient w = new WebClient();
            try
            {
                w.DownloadFile(CSV_MASTER_RULE_URL, path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            MessageBox.Show(Properties.Resources.UpdateCompleted, Properties.Resources.OnlineUpdate, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }

        /// <summary>
        /// オンライン定義ファイルのバージョンを取得する
        /// </summary>
        /// <returns>バージョン</returns>
        public String GetRecentMasterVersion()
        {
            CsvRules rules = new CsvRules();

            WebClient w = new WebClient();
            w.Encoding = System.Text.Encoding.UTF8;
            try
            {
                String xml = w.DownloadString(CSV_MASTER_RULE_URL);
                return rules.LoadFromString(xml);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}
