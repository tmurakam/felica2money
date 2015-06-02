using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Xml;

namespace FeliCa2Money
{
    class CsvRulesUpdater : UpdateChecker
    {
        /// <summary>
        /// マスタCSVルール定義ファイルの URL
        /// </summary>
        //private const string CSV_MASTER_RULE_URL = "https://github.com/tmurakam/felica2money/raw/master/defs/CsvRules.xml";
        //private const string CSV_MASTER_RULE_URL = "https://raw.githubusercontent.com/tmurakam/felica2money/master/defs/CsvRules.xml";
        private const string CSV_MASTER_RULE_URL = "http://felica2money.tmurakam.org/data/CsvRules.php";

        override protected string getRemoteUrl()
        {
            return CSV_MASTER_RULE_URL;
        }

        override protected DateTime lastUpdated
        {
            get { return Properties.Settings.Default.LastCsvRuleUpdated; }
            set
            {
                Properties.Settings.Default.LastCsvRuleUpdated = value;
                Properties.Settings.Default.Save();
            }
        }

        override protected DateTime lastUpdateCheck
        {
            get { return Properties.Settings.Default.LastCsvRuleUpdateCheck; }
            set { 
                Properties.Settings.Default.LastCsvRuleUpdateCheck = value;
                Properties.Settings.Default.Save();
            }
        }

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
        public bool CheckUpdate(bool manualUpdate = false)
        {
            if (!manualUpdate && !isUpdateTime())
            {
                return false;
            }

            var remoteVersion = GetRecentMasterVersion();
            if (remoteVersion == null)
            {
                // ネットワーク未接続など
                if (manualUpdate)
                {
                    MessageBox.Show("CSV定義情報をダウンロードできません", Properties.Resources.OnlineUpdate, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            CsvRules rules = new CsvRules();
            rules.LoadAllRules();

            if (rules.MasterVersion == null || remoteVersion.CompareTo(rules.MasterVersion) > 0)
            {
                if (manualUpdate)
                {
                    return DownloadRule();
                }

                var result = MessageBox.Show("新しいCSV定義ファイルがあります。更新しますか？", "確認",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Yes)
                {
                    return DownloadRule();
                }
            }
            else
            {
                // すでに最新版となっている
                saveLastUpdated();
                if (manualUpdate)
                {
                    MessageBox.Show("CSV定義ファイルは最新版です", Properties.Resources.OnlineUpdate, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            return false;
        }

        /// <summary>
        /// オンライン定義ファイルのバージョンを取得する
        /// </summary>
        /// <returns>バージョン</returns>
        public string GetRecentMasterVersion()
        {
            var rules = new CsvRules();

            var xml = downloadRemoteUrl();
            if (xml == null)
            {
                return null;
            }
            return rules.LoadFromString(xml);
        }

        /// <summary>
        /// 定義ファイルをダウンロードする
        /// </summary>
        /// <returns></returns>
        public bool DownloadRule()
        {
            string path = CsvRules.GetRulesPath() + "\\" + CsvRules.CSV_MASTER_RULE_FILENAME;

            try
            {
                downloadToFile(path);
                MessageBox.Show(Properties.Resources.UpdateCompleted, Properties.Resources.OnlineUpdate, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
