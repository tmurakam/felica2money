/*
 * FeliCa2Money
 *
 * Copyright (C) 2001-2011 Takuya Murakami
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

// CSV 変換ルールセット

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace FeliCa2Money
{
    /// <summary>
    /// CSVルールセット
    /// </summary>
    public class CsvRules
    {
        /// <summary>
        /// CSVルール定義ファイルの URL
        /// </summary>
        private const String CSV_RULE_URL = "https://github.com/tmurakam/felica2money/raw/master/defs/CsvRules.xml";
        // "http://moneyimport.sourceforge.jp/CsvRules.xml";
        // "http://svn.sourceforge.jp/svnroot/moneyimport/trunk/FeliCa2Money.net/CsvRules.xml";

        private List<CsvRule> mRules;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CsvRules()
        {
            mRules = new List<CsvRule>();
        }

        /// <summary>
        /// ルール数
        /// </summary>
        public int Count
        {
            get { return mRules.Count; }
        }

        /// <summary>
        /// ルール追加(単体テスト用)
        /// </summary>
        /// <param name="rule">ルール</param>
        public void Add(CsvRule rule)
        {
            mRules.Add(rule);
        }

        /// <summary>
        /// 全 CSV 変換ルールを読み出す
        /// </summary>
        /// <returns></returns>
        public bool LoadAllRules()
        {
            // ユーザ設定フォルダのほうから読み出す
            String path = getRulesPath();
            //String path = Path.GetDirectoryName(Application.ExecutablePath);

            string[] xmlFiles = Directory.GetFiles(path, "*.xml");
            if (xmlFiles.Length == 0)
            {
                if (MessageBox.Show(Properties.Resources.QueryCsvRuleDownload, Properties.Resources.Confirm,
                    MessageBoxButtons.OKCancel) != DialogResult.OK) return false;

                if (!DownloadRule()) return false;

                xmlFiles = Directory.GetFiles(path, "*.xml");                
            }

            foreach (string xmlFile in xmlFiles)
            {
                try
                {
                    LoadFromFile(xmlFile);
                }
                catch (Exception)
                {
                    MessageBox.Show(Properties.Resources.CsvRuleError + " in " + xmlFile, Properties.Resources.Error);
                }
            }
            return true;
        }

        /// <summary>
        /// 定義ファイルを１つ読み込む
        /// </summary>
        /// <param name="path">定義ファイルパス</param>
        private void LoadFromFile(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            LoadFromXml(doc);
        }

        private void LoadFromXml(XmlDocument doc)
        {
            XmlElement root = doc.DocumentElement;

            // Rule 子要素について処理
            XmlNodeList list = root.GetElementsByTagName("Rule");

            for (int i = 0; i < list.Count; i++)
            {
                // rule 生成
                CsvRule rule = new CsvRule();

                // rule の各メンバを設定
                foreach (XmlElement e in list[i].ChildNodes)
                {
                    string value = "";
                    if (e.FirstChild == null)
                    {
                        // 空要素
                    }
                    else
                    {
                        value = e.FirstChild.Value;
                    }

                    switch (e.Name)
                    {
                        case "Ident":
                            rule.ident = value;
                            break;
                        case "Name":
                            rule.name = value;
                            break;
                        case "BankId":
                            rule.bankId = value;
                            break;
                        case "FirstLine":
                            rule.firstLine = value;
                            break;
                        case "Format":
                            rule.format = value;
                            break;
                        case "Order":
                            rule.order = value;
                            break;
                        case "Separator":
                            rule.separator = value;
                            break;
                        default:
                            // ignore
                            break;
                    }
                }

                mRules.Add(rule);
            }
        }

        /// <summary>
        /// ルール名一覧を返す
        /// </summary>
        /// <returns>ルール名</returns>
        public string[] names()
        {
            string[] names = new string[mRules.Count];
            int i = 0;
            foreach (CsvRule rule in mRules)
            {
                names[i] = rule.name;
                i++;
            }
            return names;
        }

        /// <summary>
        /// 指定したインデックスのルールを返す
        /// </summary>
        /// <param name="idx">インデックス</param>
        /// <returns>ルール</returns>
        public CsvRule GetAt(int idx)
        {
            return mRules[idx];
        }

        /// <summary>
        /// 指定したルールのインデックスを返す
        /// </summary>
        /// <param name="rule">ルール</param>
        /// <returns>インデックス</returns>
        public int IndexOf(CsvRule rule)
        {
            return mRules.IndexOf(rule);
        }

        /// <summary>
        /// firstLine に一致するルールを探す
        /// </summary>
        /// <param name="firstLine">firstLine</param>
        /// <returns>ルール</returns>
        public CsvRule FindRule(string firstLine)
        {
            foreach (CsvRule rule in mRules)
            {
                if (rule.firstLine == firstLine)
                {
                    return rule;
                }
            }
            return null;
        }

        /// <summary>
        /// 定義ファイルをダウンロードする
        /// </summary>
        /// <returns></returns>
        public static bool DownloadRule()
        {
            string path = getRulesPath() + "\\CsvRules.xml";

            WebClient w = new WebClient();
            try
            {
                w.DownloadFile(CSV_RULE_URL, path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Properties.Resources.Error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 定義ファイルのディレクトリを返す
        /// UserAppDataPath からバージョン番号を除いたものが帰る
        /// </summary>
        /// <returns></returns>
        private static string getRulesPath()
        {
            string path = Application.LocalUserAppDataPath;
            path = System.IO.Path.GetDirectoryName(path);
            return path;
        }
    }
}
