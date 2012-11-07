using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Xml;

namespace FeliCa2Money
{
    /**
     * アップデートチェッカ : 基底クラス
     */
    abstract class UpdateChecker
    {
        /// <summary>
        /// チェック間隔 (HOURS)
        /// </summary>
        protected const int UPDATE_CHECK_INTERVAL_HOURS = 7*24;

        /// <summary>
        /// チェックリトライ間隔 (HOURS)
        /// </summary>
        protected const int UPDATE_CHECK_RETRY_HOURS = 6;

        // チェックする URL を返す
        protected abstract string getRemoteUrl();

        /// <summary>
        /// リモートファイルをダウンロードする
        /// </summary>
        /// <returns>バージョン</returns>
        protected String downloadRemoteUrl()
        {
            WebClient w = new WebClient();
            w.Encoding = System.Text.Encoding.UTF8;
            try
            {
                String data = w.DownloadString(getRemoteUrl());
                return data;
            }
            catch (Exception ex)
            {
                //バージョン取得失敗 : エラーにはしない
                //MessageBox.Show(ex.Message, Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// リモートファイルを指定したファイルにダウンロードする
        /// エラー時は例外が発生する
        /// </summary>
        public void downloadToFile(string path)
        {
            WebClient w = new WebClient();
            w.DownloadFile(getRemoteUrl(), path);

            saveLastUpdated();
        }

        /// <summary>
        /// 更新時刻が到来したか調べる
        /// </summary>
        /// <returns></returns>
        protected bool isUpdateTime()
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
            //return true; // DEBUG 時のみ！
            return false;
        }

        /// <summary>
        /// 最終更新時刻を保存する
        /// </summary>
        protected void saveLastUpdated()
        {
            Properties.Settings s = Properties.Settings.Default;
            s.LastCsvRuleUpdated = DateTime.Now;
            s.Save();
        }
    }
}
