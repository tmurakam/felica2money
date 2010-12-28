/*
 * FeliCa2Money
 *
 * Copyright (C) 2001-2010 Takuya Murakami
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

// AGRファイル 取り込み処理

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace FeliCa2Money
{
    class AgrFile
    {
        private List<Account> mCards;

        enum State
        {
            SearchingStart,
            ReadAccountInfo,
            ReadTransactions
        };

        public List<Account> cards
        {
            get { return mCards; }
        }

        public bool loadFromFile(string path)
        {
            // SJIS で開く
            StreamReader sr = new StreamReader(path, System.Text.Encoding.Default);

            // フォーマットチェック
            string line = sr.ReadLine();
            if (line != "\"あぐりっぱ\",\"1.0\"")
            {
                return false;
            }

            mCards = new List<Account>();
            AgrAccount account = null;

            // 行をパースする
            State state = State.SearchingStart;
            bool isCreditCard = false;

            while ((line = sr.ReadLine()) != null)
            {
                switch (state) {
                    case State.SearchingStart:
                        if (line.StartsWith("<START_CP")) {
                            state = State.ReadAccountInfo;
                            if (line.EndsWith("_PAY>"))
                            {
                                isCreditCard = true;
                            }
                            else
                            {
                                isCreditCard = false;
                            }
                        }
                        break;

                    case State.ReadAccountInfo:
                        account = new AgrAccount();
                        account.isCreditCard = isCreditCard;
                        if (!account.readAccountInfo(line))
                        {
                            return false;
                        }
                        state = State.ReadTransactions;
                        break;

                    case State.ReadTransactions:
                        if (line.StartsWith("<END_"))
                        {
                            mCards.Add(account);
                            state = State.SearchingStart;
                        }
                        else
                        {
                            if (!account.readTransaction(line))
                            {
                                // 解析エラー: この口座は無視する
                                state = State.SearchingStart;
                            }
                        }
                        break;
                }
            }
            return true;
        }
    }

    class AgrAccount : Account
    {
        public AgrAccount()
        {
            mTransactions = new List<Transaction>();
        }

        public override void ReadCard()
        {
            // 使用しない
        }

        public bool readAccountInfo(string line)
        {
            string[] columns = SplitCsv(line);

            if (columns.Length < 3)
            {
                return false;
            }
            if (columns.Length >= 5 && columns[4] != "JPY")
            {
                return false;
            }

            string bankName = columns[0];
            string branchName = columns[1];
            string accountId = columns[2];

            mIdent = bankName;
            mBankId = bankName;
            mBranchId = getDummyId(branchName).ToString();
            mAccountId = accountId;

            return true;
        }

        private int getDummyId(string name)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = MD5.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(name));

            // 先頭３バイトだけを使う
            int result = (int)hash[0] << 16 | (int)hash[1] << 8 | (int)hash[2];
            return result;
        }

        public bool readTransaction(string line)
        {
            string[] columns = SplitCsv(line);
            if (columns.Length < 8)
            {
                return false;
            }

            Transaction transaction = new Transaction();

            // 日付の処理
            string[] ary = columns[0].Split(new char[] { '/' });
            if (ary.Length == 3)
            {
                transaction.date = new DateTime(int.Parse(ary[0]), int.Parse(ary[1]), int.Parse(ary[2]), 0, 0, 0);
            }
            else if (ary.Length == 2)
            {
                // 月と日のみ。年は推定する。
                DateTime now = DateTime.Now;
                int mm = int.Parse(ary[0]);
                int dd = int.Parse(ary[1]);

                DateTime d = new DateTime(now.Year, mm, dd, 0, 0, 0);

                // ３ヶ月以上先の場合、昨年とみなす。
                TimeSpan ts = d - now;
                if (ts.TotalDays > 90) {
                    d = new DateTime(now.Year - 1, mm, dd, 0, 0, 0);
                }
                transaction.date = d;
            }
            else
            {
                return false;
            }
            

            // 摘要
            transaction.desc = columns[1];

            // 入金額/出金額
            if (columns[2] != "" && columns[2] != "--" && columns[2] != "*")
            {
                transaction.value = int.Parse(columns[2]);
            }
            else if (columns[4] != "" && columns[4] != "--" && columns[4] != "*")
            {
                transaction.value = -int.Parse(columns[4]);
            }
            else
            {
                return false;
            }

            // 残高
            if (columns[6] != "" && columns[6] != "--" && columns[6] != "*")
            {
                transaction.balance = int.Parse(columns[6]);
            }
            // Note: 残高は入っていない場合もある

            // ID採番
            transaction.id = 0;
            if (mTransactions.Count > 0)
            {
                Transaction prev = mTransactions[mTransactions.Count - 1];

                if (transaction.date == prev.date)
                {
                    transaction.id = prev.id + 1;
                }
            }
            mTransactions.Add(transaction);

            return true;
        }

        // CSV のフィールド分割
        private string[] SplitCsv(string line)
        {
            ArrayList fields = new ArrayList();
            Regex regCsv;

            // カンマ区切り
            regCsv = new Regex("\\s*(\"(?:[^\"]|\"\")*\"|[^,]*)\\s*,", RegexOptions.None);
            line = line + ",";

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
