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
using System.Linq;

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
        public const string CsvMasterRulsFilename = "CsvRules.xml";

        // ルールセット
        private readonly IList<CsvRule> _rules;

        // マスタルールバージョン
        private string _masterVersion = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CsvRules()
        {
            _rules = new List<CsvRule>();
        }

        /// <summary>
        /// イテレータ
        /// </summary>
        public IEnumerator<CsvRule> GetEnumerator()
        {
            return _rules.GetEnumerator();
        }

        /// <summary>
        /// マスタバージョン
        /// </summary>
        public string MasterVersion
        {
            get { return _masterVersion; }
        }

        /// <summary>
        /// ルール数
        /// </summary>
        public int Count
        {
            get { return _rules.Count; }
        }

        /// <summary>
        /// 指定したインデックスのルールを返す
        /// </summary>
        /// <param name="idx">インデックス</param>
        /// <returns>ルール</returns>
        public CsvRule GetAt(int idx)
        {
            return _rules[idx];
        }

        /// <summary>
        /// 指定したルールのインデックスを返す
        /// </summary>
        /// <param name="rule">ルール</param>
        /// <returns>インデックス</returns>
        public int IndexOf(CsvRule rule)
        {
            return _rules.IndexOf(rule);
        }

        /// <summary>
        /// ルール追加(単体テスト用)
        /// </summary>
        /// <param name="rule">ルール</param>
        public void Add(CsvRule rule)
        {
            _rules.Add(rule);
        }

        /// <summary>
        /// ルール名一覧を返す
        /// </summary>
        /// <returns>ルール名</returns>
        public IEnumerable<string> Names()
        {
            return from x in _rules select x.Name;
        }

        /// <summary>
        /// ident に一致するルールを探す
        /// </summary>
        /// <param name="ident">識別子</param>
        /// <returns>ルール</returns>
        public CsvRule FindRuleWithIdent(string ident)
        {
            return _rules.FirstOrDefault(rule => rule.Ident == ident);
        }

        /// <summary>
        /// firstLine に一致するルールを探す
        /// </summary>
        /// <param name="firstLine">firstLine</param>
        /// <returns>ルール</returns>
        public CsvRule FindRuleForFirstLine(string firstLine)
        {
            return _rules.FirstOrDefault(rule => rule.FirstLine == firstLine);
        }

        /// <summary>
        /// 全 CSV 変換ルールを読み出す
        /// </summary>
        /// <returns></returns>
        public bool LoadAllRules()
        {
            _rules.Clear();

            // ユーザ設定フォルダのほうから読み出す
            var path = GetRulesPath();
            //String path = Path.GetDirectoryName(Application.ExecutablePath);

            var xmlFiles = Directory.GetFiles(path, "*.xml");
            if (xmlFiles.Length == 0)
            {
                // 定義ファイルがなければダウンロードさせる
                var updater = new CsvRulesUpdater();
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
                    var version = LoadFromFile(xmlFile);
                    if (Path.GetFileName(xmlFile) == CsvMasterRulsFilename)
                    {
                        _masterVersion = version;
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
        public string LoadFromFile(string path)
        {
            var doc = new XmlDocument();
            doc.Load(path);

            return LoadFromXml(doc);
        }

        public string LoadFromString(string xmlString)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlString);

            return LoadFromXml(doc);
        }

        private string LoadFromXml(XmlDocument doc)
        {
            var root = doc.DocumentElement;

            // Version 取得
            string version = null;
            var versionList = root.GetElementsByTagName("Version");
            if (versionList.Count > 0)
            {
                version = versionList[0].FirstChild.Value;
            }

            // Rule 子要素について処理
            var list = root.GetElementsByTagName("Rule");

            for (var i = 0; i < list.Count; i++)
            {
                // rule 生成
                var rule = new CsvRule();

                // rule の各メンバを設定
                foreach (XmlElement e in list[i].ChildNodes)
                {
                    var value = "";
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
                            rule.SetFormat(value);
                            break;
                        case "Order":
                            rule.OrderString = value;
                            break;
                        case "Separator":
                            rule.Separator = value;
                            break;
                        default:
                            // ignore
                            break;
                    }
                }

                _rules.Add(rule);
            }

            return version;
        }


        /// <summary>
        /// 定義ファイルのディレクトリを返す
        /// UserAppDataPath からバージョン番号を除いたものが帰る
        /// </summary>
        /// <returns></returns>
        public static string GetRulesPath()
        {
            var path = Application.LocalUserAppDataPath;
            path = System.IO.Path.GetDirectoryName(path);
            return path;
        }
    }
}
