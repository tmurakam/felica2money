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

// CSV 取り込み処理

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FeliCa2Money
{
    class CsvCard : Card
    {
        private CsvRules rules = new CsvRules();
        private StreamReader sr;
        private CsvRule rule;

        public bool LoadAllRules()
        {
            return rules.LoadAllRules();
        }

        public bool OpenFile(string path)
        {
            // TODO: とりあえず SJIS で開く (UTF-8 とかあるかも?)
            sr = new StreamReader(path, System.Text.Encoding.Default);

            string firstLine = sr.ReadLine();

            // 合致するルールを探す
            rule = rules.FindRule(firstLine);

            CsvDialog dlg = new CsvDialog(rules);

            // ルール/口座番号などを選択
            dlg.SelectRule(rule);
            if (dlg.ShowDialog() == DialogResult.Cancel)
            {
                return false;
            }

            // 選択されたルールを取り出す
            rule = dlg.SelectedRule();
            if (rule == null)
            {
                MessageBox.Show("CSV変換ルールが選択されていません", "エラー");
                return false;
            }

            // 銀行IDなどを設定
            org = rule.Ident;
            bankId = rule.BankId;
            branchId = dlg.BranchId;
            accountId = dlg.AccountId;

            // 1行目から再度読み込み直す
            sr.Close();
            sr = new StreamReader(path, System.Text.Encoding.Default);

            // firstLine まで読み飛ばす
            if (rule.FirstLine != null)
            {
                while ((firstLine = sr.ReadLine()) != null)
                {
                    if (firstLine == rule.FirstLine) break;
                }
            }

            // 読み込み準備
            rule.Reset();

            return true;
        }

        public void Close()
        {
            sr.Close();
        }
            
        // CSV 読み込み処理
        public override List<Transaction> ReadCard()
        {
            List<Transaction> transactions = new List<Transaction>();
            string line;

            while ((line = sr.ReadLine()) != null)
            {
                // CSV カラム分割
                string[] row = SplitCsv(line);
                if (row.Length <= 1) continue; // ad hoc...

                // パース
                Transaction t = rule.parse(row);
                transactions.Add(t);
            }

            // 順序逆転処理
            if (!rule.IsAscent)
            {
                transactions.Reverse();
            }

            return transactions;
        }

        // CSV のフィールド分割
        private string[] SplitCsv(string line)
        {
            ArrayList fields = new ArrayList();
            Regex regCsv;

            if (rule.IsTSV)
            {
                regCsv = new Regex("([^\\t]*)\\t");
                line = line + "\t";
            }
            else
            {
                regCsv = new Regex("\\s*(\"(?:[^\"]|\"\")*\"|[^,]*)\\s*,", RegexOptions.None);
                line = line + ",";
            }

            Match m = regCsv.Match(line);
            int count = 0;
            while (m.Success)
            {
                string field = m.Groups[1].Value;

                // 前後の空白を削除
                field = field.Trim();

                // ダブルクォートを抜く
                if (field.StartsWith("\"") && field.EndsWith("\""))
                {
                    field = field.Substring(1, field.Length - 2);
                }
                // "" を " に変換
                field = field.Replace("\"\"", "\"");

                // もう一度前後の空白を削除
                field = field.Trim();

                fields.Add(field);
                count++;
                m = m.NextMatch();
            }

            return fields.ToArray(typeof(string)) as string[];
        }
    }
}
