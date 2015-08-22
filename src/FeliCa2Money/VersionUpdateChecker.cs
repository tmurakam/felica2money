using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace FeliCa2Money
{
    class VersionUpdateChecker : UpdateChecker
    {
        private const string SiteUrl = "http://felica2money.tmurakam.org";

        protected override string GetRemoteUrl()
        {
            //return "https://github.com/tmurakam/felica2money/raw/master/defs/RecentVersion.txt";
            //return "https://raw.githubusercontent.com/tmurakam/felica2money/master/defs/RecentVersion.txt";
            return "http://felica2money.tmurakam.org/data/RecentVersion.php";
        }

        override protected DateTime LastUpdated
        {
            get { return Properties.Settings.Default.LastVersionUpdateChecked; }
            set
            {
                Properties.Settings.Default.LastVersionUpdateChecked = value;
                Properties.Settings.Default.Save();
            }
        }

        override protected DateTime LastUpdateCheck
        {
            get { return Properties.Settings.Default.LastVersionUpdateCheck; }
            set { 
                Properties.Settings.Default.LastVersionUpdateCheck = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// アップデートチェックを行う
        /// </summary>
        /// <returns></returns>
        public void CheckUpdate(bool forceCheck = false)
        {
            if (!forceCheck && !IsUpdateTime())
            {
                return;
            }

            var data = DownloadRemoteUrl();
            if (data == null) return; // do nothing

            var sr = new StringReader(data);
            string recentVersion = sr.ReadLine().Trim();

            SaveLastUpdated();

            if (recentVersion.CompareTo(GetCurrentVersion()) <= 0)
            {
                return; // 最新版を使用している
            }

            // 最新版が見つかった
            DialogResult result = MessageBox.Show("新しいバージョンが見つかりました。Webサイトを表示しますか？", "確認",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                try
                {
                    System.Diagnostics.Process.Start(SiteUrl);

                    //String helpFile = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\Felica2Money.html";
                    //System.Diagnostics.Process.Start(helpFile);
                }
                catch
                {
                    // do nothing
                }
            }
        }

        // バージョン番号の取得
        public static string GetCurrentVersion()
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            System.Version ver = asm.GetName().Version;
            return string.Format("{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
        }
    }
}
