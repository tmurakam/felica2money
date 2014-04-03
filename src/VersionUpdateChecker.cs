using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace FeliCa2Money
{
    class VersionUpdateChecker : UpdateChecker
    {
        private const string SITE_URL = "http://felica2money.tmurakam.org";

        protected override string getRemoteUrl()
        {
            //return "https://github.com/tmurakam/felica2money/raw/master/defs/RecentVersion.txt";
            //return "https://raw.githubusercontent.com/tmurakam/felica2money/master/defs/RecentVersion.txt";
            return "http://felica2money.tmurakam.org/data/RecentVersion.php";
        }

        override protected DateTime lastUpdated
        {
            get { return Properties.Settings.Default.LastVersionUpdateChecked; }
            set
            {
                Properties.Settings.Default.LastVersionUpdateChecked = value;
                Properties.Settings.Default.Save();
            }
        }

        override protected DateTime lastUpdateCheck
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
            if (!forceCheck && !isUpdateTime())
            {
                return;
            }

            String data = downloadRemoteUrl();
            if (data == null) return; // do nothing

            StringReader sr = new StringReader(data);
            string recentVersion = sr.ReadLine().Trim();

            saveLastUpdated();

            if (recentVersion.CompareTo(getCurrentVersion()) <= 0)
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
                    System.Diagnostics.Process.Start(SITE_URL);

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
        public static string getCurrentVersion()
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            System.Version ver = asm.GetName().Version;
            return String.Format("{0}.{1}.{2}", ver.Major, ver.Minor, ver.Revision);
        }
    }
}
