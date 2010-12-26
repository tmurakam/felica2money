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
    public class CsvRules
    {
        private List<CsvRule> ruleList;

        public CsvRules()
        {
            ruleList = new List<CsvRule>();
        }

        public int Count
        {
            get { return ruleList.Count; }
        }

        // 全 CSV 変換ルールを読み出す
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

        // 定義ファイルを１つ読み込む
        public void LoadFromFile(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            LoadFromXml(doc);
        }

        public void LoadFromXml(XmlDocument doc)
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
                            rule.Ident = value;
                            break;
                        case "Name":
                            rule.Name = value;
                            break;
                        case "BankId":
                            rule.BankId = value;
                            break;
                        case "FirstLine":
                            rule.FirstLine = value;
                            break;
                        case "Format":
                            rule.Format = value;
                            break;
                        case "Order":
                            rule.Order = value;
                            break;
                        case "Separator":
                            rule.Separator = value;
                            break;
                        default:
                            // ignore
                            break;
                    }
                }

                ruleList.Add(rule);
            }
        }

        // 名前一覧を返す
        public string[] names()
        {
            string[] names = new string[ruleList.Count];
            int i = 0;
            foreach (CsvRule rule in ruleList)
            {
                names[i] = rule.Name;
                i++;
            }
            return names;
        }

        // 指定したインデックスのルールを返す
        public CsvRule GetAt(int idx)
        {
            return ruleList[idx];
        }

        // 指定したルールのインデックスを返す
        public int IndexOf(CsvRule rule)
        {
            return ruleList.IndexOf(rule);
        }

        // firstLine に一致するルールを探す
        public CsvRule FindRule(string firstLine)
        {
            foreach (CsvRule rule in ruleList)
            {
                if (rule.FirstLine == firstLine)
                {
                    return rule;
                }
            }
            return null;
        }

        // 定義ファイルをダウンロードする
        public static bool DownloadRule()
        {
            string path = getRulesPath() + "\\CsvRules.xml";
            //string url = "http://moneyimport.sourceforge.jp/CsvRules.xml";
            //string url = "http://svn.sourceforge.jp/svnroot/moneyimport/trunk/FeliCa2Money.net/CsvRules.xml";
            string url = "https://github.com/tmurakam/felica2money/raw/master/src/CsvRules.xml";

            WebClient w = new WebClient();
            try
            {
                w.DownloadFile(url, path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Properties.Resources.Error);
                return false;
            }
            return true;
        }

        // 定義ファイルのディレクトリを返す
        //   UserAppDataPath からバージョン番号を除いたもの
        private static string getRulesPath()
        {
            string path = Application.LocalUserAppDataPath;
            path = System.IO.Path.GetDirectoryName(path);
            return path;
        }
    }
}
