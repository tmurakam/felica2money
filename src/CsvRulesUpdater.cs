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
        /// CSVルールアップデートチェック間隔 (HOURS)
        /// </summary>
        private const int UPDATE_CHECK_INTERVAL_HOURS = 7*24;

        /// <summary>
        /// CSVルールアップデートリトライ間隔 (HOURS)
        /// </summary>
        private const int UPDATE_CHECK_RETRY_HOURS = 6;

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

            String remoteVersion = GetRecentMasterVersion();
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

                DialogResult result = MessageBox.Show("新しいCSV定義ファイルがあります。更新しますか？", "確認",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
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

            saveLastUpdated();
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
                //バージョン取得失敗 : エラーにはしない
                //MessageBox.Show(ex.Message, Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// 更新時刻が到来したか調べる
        /// </summary>
        /// <returns></returns>
        private bool isUpdateTime()
        {
            Properties.Settings s = Properties.Settings.Default;

            DateTime now = DateTime.Now;
            DateTime lastUpdated = s.LastCsvRuleUpdated;
            DateTime lastUpdateCheck = s.LastCsvRuleUpdateCheck;

            s.LastCsvRuleUpdateCheck = now;
            s.Save();

            TimeSpan diff1 = now.Subtract(lastUpdated);
            TimeSpan diff2 = now.Subtract(lastUpdateCheck);

            if (diff1.TotalHours > UPDATE_CHECK_INTERVAL_HOURS && diff2.TotalHours > UPDATE_CHECK_RETRY_HOURS)
            {
                return true;
            }
            return false;
        }

        private void saveLastUpdated()
        {
            Properties.Settings s = Properties.Settings.Default;
            s.LastCsvRuleUpdated = DateTime.Now;
            s.Save();
        }
    }
}
