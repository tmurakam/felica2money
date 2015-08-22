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
        protected const int UpdateCheckIntervalHours = 7*24;

        /// <summary>
        /// チェックリトライ間隔 (HOURS)
        /// </summary>
        protected const int UpdateCheckRetryHours = 6;

        // チェックする URL を返す
        protected abstract string GetRemoteUrl();

        /// <summary>
        /// リモートファイルをダウンロードする
        /// </summary>
        /// <returns>バージョン</returns>
        protected string DownloadRemoteUrl()
        {
            var w = new WebClient {Encoding = Encoding.UTF8};
            try
            {
                var data = w.DownloadString(GetRemoteUrl());
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
        public void DownloadToFile(string path)
        {
            var w = new WebClient();
            w.DownloadFile(GetRemoteUrl(), path);

            SaveLastUpdated();
        }

        /// <summary>
        /// 更新時刻が到来したか調べる
        /// </summary>
        /// <returns></returns>
        protected bool IsUpdateTime()
        {
            var s = Properties.Settings.Default;

            var now = DateTime.Now;

            var diff1 = now.Subtract(LastUpdated);
            var diff2 = now.Subtract(LastUpdateCheck);
            LastUpdateCheck = now;

            if (diff1.TotalHours > UpdateCheckIntervalHours && diff2.TotalHours > UpdateCheckRetryHours)
            {
                return true;
            }
            //return true; // DEBUG 時のみ！
            return false;
        }

        /// <summary>
        /// 最終更新時刻を保存する
        /// </summary>
        protected void SaveLastUpdated()
        {
            LastUpdated = DateTime.Now;
        }

        abstract protected DateTime LastUpdated { get; set; }
        abstract protected DateTime LastUpdateCheck { get; set; }
    }
}
