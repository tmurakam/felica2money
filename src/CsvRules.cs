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
        /// マスタCSVルール定義ファイル名
        /// </summary>
        public const String CSV_MASTER_RULE_FILENAME = "CsvRules.xml";

        // ルールセット
        private List<CsvRule> mRules;

        // マスタルールバージョン
        private String mMasterVersion = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CsvRules()
        {
            mRules = new List<CsvRule>();
        }

        /// <summary>
        /// イテレータ
        /// </summary>
        public IEnumerator<CsvRule> GetEnumerator()
        {
            return mRules.GetEnumerator();
        }

        /// <summary>
        /// マスタバージョン
        /// </summary>
        public String MasterVersion
        {
            get { return mMasterVersion; }
        }

        /// <summary>
        /// ルール数
        /// </summary>
        public int Count
        {
            get { return mRules.Count; }
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
        /// ルール追加(単体テスト用)
        /// </summary>
        /// <param name="rule">ルール</param>
        public void Add(CsvRule rule)
        {
            mRules.Add(rule);
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
        /// ident に一致するルールを探す
        /// </summary>
        /// <param name="firstLine">firstLine</param>
        /// <returns>ルール</returns>
        public CsvRule FindRuleWithIdent(string ident)
        {
            foreach (CsvRule rule in mRules)
            {
                if (rule.ident == ident)
                {
                    return rule;
                }
            }
            return null;
        }

        /// <summary>
        /// firstLine に一致するルールを探す
        /// </summary>
        /// <param name="firstLine">firstLine</param>
        /// <returns>ルール</returns>
        public CsvRule FindRuleForFirstLine(string firstLine)
        {
            foreach (CsvRule rule in mRules)
            {
                if (rule.firstLineCheck(firstLine))
                {
                    return rule;
                }
            }
            return null;
        }

        /// <summary>
        /// 全 CSV 変換ルールを読み出す
        /// </summary>
        /// <returns></returns>
        public bool LoadAllRules()
        {
            mRules.Clear();

            // ユーザ設定フォルダのほうから読み出す
            String path = getRulesPath();
            //String path = Path.GetDirectoryName(Application.ExecutablePath);

            string[] xmlFiles = Directory.GetFiles(path, "*.xml");
            if (xmlFiles.Length == 0)
            {
                // 定義ファイルがなければダウンロードさせる
                CsvRulesUpdater updater = new CsvRulesUpdater();
                if (!updater.ConfirmDownloadRule())
                {
                    // ダウンロード失敗 or ユーザキャンセル
                    return false;
                }

                xmlFiles = Directory.GetFiles(path, "*.xml");                
            }

            foreach (string xmlFile in xmlFiles)
            {
                try
                {
                    String version = LoadFromFile(xmlFile);
                    if (Path.GetFileName(xmlFile) == CSV_MASTER_RULE_FILENAME)
                    {
                        mMasterVersion = version;
                    }
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
        /// <returns>バージョン文字列</returns>
        public String LoadFromFile(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            return LoadFromXml(doc);
        }

        public String LoadFromString(string xmlString)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);

            return LoadFromXml(doc);
        }

        private String LoadFromXml(XmlDocument doc)
        {
            XmlElement root = doc.DocumentElement;

            // Version 取得
            String version = null;
            XmlNodeList versionList = root.GetElementsByTagName("Version");
            if (versionList.Count > 0)
            {
                version = versionList[0].FirstChild.Value;
            }

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

            return version;
        }


        /// <summary>
        /// 定義ファイルのディレクトリを返す
        /// UserAppDataPath からバージョン番号を除いたものが帰る
        /// </summary>
        /// <returns></returns>
        public static string getRulesPath()
        {
            string path = Application.LocalUserAppDataPath;
            path = System.IO.Path.GetDirectoryName(path);
            return path;
        }
    }
}
