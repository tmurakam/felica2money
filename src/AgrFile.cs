// -*-  Mode:C++; c-basic-offset:4; tab-width:4; indent-tabs-mode:nil -*-
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

// AGR(Agurippa電子明細)ファイル 取り込み処理

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
    /// <summary>
    /// AGRファイル解析クラス
    /// </summary>
    public class AgrFile
    {
        private List<Account> mAccounts;

        enum State
        {
            SearchingStart,
            ReadAccountInfo,
            ReadTransactions
        };

        /// <summary>
        /// アカウントリスト
        /// </summary>
        public List<Account> accounts
        {
            get { return mAccounts; }
        }

        /// <summary>
        /// AGRファイルを読み込む
        /// </summary>
        /// <param name="path">AGRファイルパス</param>
        /// <returns>成功時は true、失敗時はfalse</returns>
        public bool loadFromFile(string path)
        {
            // SJIS で開く
            StreamReader sr = new StreamReader(path, System.Text.Encoding.Default);
            return load(sr);
        }

        public bool load(StreamReader sr) 
        {
            try
            {
                // フォーマットチェック
                string line = sr.ReadLine();
                if (line != "\"あぐりっぱ\",\"1.0\"")
                {
                    return false;
                }

                mAccounts = new List<Account>();
                AgrAccount account = null;

                AgrAccount.Builder builder = new AgrAccount.Builder();

                // 行をパースする
                State state = State.SearchingStart;
                bool isCreditCard = false;

                while ((line = sr.ReadLine()) != null)
                {
                    switch (state)
                    {
                        case State.SearchingStart:
                            if (line.StartsWith("<START_CP"))
                            {
                                if (line.EndsWith("_PAY>"))
                                {
                                    state = State.ReadAccountInfo;
                                    isCreditCard = true;
                                }
                                else if (line.EndsWith("_ORD>"))
                                {
                                    state = State.ReadAccountInfo;
                                    isCreditCard = false;
                                }
                                else
                                {
                                    // ignore : _BILL など
                                }
                            }
                            break;

                        case State.ReadAccountInfo:
                            if (line.StartsWith("<END_"))
                            {
                                // no definition line : ignore
                                state = State.SearchingStart;
                                break;
                            }
                            if (isCreditCard)
                            {
                                account = builder.newCreditCardAccount(line);
                            }
                            else
                            {
                                account = builder.newBankAccount(line);
                            }
                            if (account == null)
                            {
                                sr.Close();
                                return false;
                            }
                            state = State.ReadTransactions;
                            break;

                        case State.ReadTransactions:
                            if (line.StartsWith("<END_"))
                            {
                                mAccounts.Add(account);
                                state = State.SearchingStart;
                            }
                            else
                            {
                                if (!account.readTransaction(line))
                                {
                                    // 解析エラー: この取引は無視する
                                }
                            }
                            break;
                    }
                }
            }
            finally
            {
                if (sr != null)
                {
                    sr.Close();
                }
            }
            return true;
        }

        /// <summary>
        /// 全トランザクション数を返す
        /// </summary>
        /// <returns></returns>
        public int numTransactions()
        {
            int count = 0;
            foreach (AgrAccount account in mAccounts) {
                count += account.transactions.Count;
            }
            return count;
        }
    }

    /// <summary>
    /// AGRアカウント
    /// </summary>
    public class AgrAccount : Account
    {
        /// <summary>
        /// AGRアカウントビルダ
        /// </summary>
        public class Builder
        {
            private Hashtable mNameHash;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public Builder()
            {
                mNameHash = new Hashtable();
            }

            /// <summary>
            /// 銀行型アカウントの生成
            /// </summary>
            /// <returns>アカウント</returns>
            public AgrAccount newBankAccount(string line)
            {
                return newAccount(line, false);
            }

            /// <summary>
            /// クレジットカード型アカウントの作成
            /// </summary>
            /// <returns>アカウント</returns>
            public AgrAccount newCreditCardAccount(string line)
            {
                return newAccount(line, true);
            }

            private AgrAccount newAccount(string line, bool isCreditCard)
            {
                AgrAccount account = new AgrAccount();
                account.isCreditCard = isCreditCard;
                if (!account.readAccountInfo(line, mNameHash))
                {
                    return null;
                }
                return account;
            }
        }

        private AgrAccount()
        {
            mTransactions = new List<Transaction>();
        }

        public override void ReadTransactions()
        {
            // 使用しない
        }

        /// <summary>
        /// アカウント情報を読み込む
        /// </summary>
        /// <param name="line">アカウント情報行</param>
        /// <returns></returns>
        private bool readAccountInfo(string line, Hashtable nameHash)
        {
            string[] columns = CsvUtil.SplitCsv(line, false);

            if (columns.Length < 3)
            {
                return false;
            }
            if (columns.Length >= 5 && columns[4] != "JPY")
            {
                return false;
            }

            if (!isCreditCard)
            {
                // 銀行口座
                string bankName = columns[0];
                string branchName = columns[1];
                string accountId = columns[2];
                if (columns.Length >= 4)
                {
                    try
                    {
                        balance = int.Parse(columns[3]);
                        hasBalance = true;
                    }
                    catch
                    {
                        hasBalance = false;
                    }
                }

                mIdent = bankName;
                mBankId = bankName;
                mBranchId = branchName; // getDummyId(branchName).ToString();
                mAccountId = accountId;
            }
            else
            {
                // クレジットカード
                string cardName = columns[0];
                try
                {
                    // 借入額
                    balance = - int.Parse(columns[2]);
                    hasBalance = true;
                }
                catch
                {
                    hasBalance = false;
                }

                // 末尾の 'カード' という文字を抜く
                cardName = "CARD_" + Regex.Replace(cardName, @"カード$", "");


                // 2カラム目は空の模様
                //string balance = columns[2];
                mIdent = "";
                mBankId = "";
                mBranchId = "";

                // 重複しないよう、連番を振る
                int counter;
                if (!nameHash.ContainsKey(cardName))
                {
                    counter = 1;
                }
                else
                {
                    counter = (int)nameHash[cardName];
                }
                mAccountId = cardName + counter.ToString();
                nameHash[cardName] = counter + 1;
            }

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

        /// <summary>
        /// 取引行の解析
        /// </summary>
        /// <param name="line">取引行</param>
        /// <returns>成功フラグ</returns>
        /// 
        public bool readTransaction(string line)
        {
            string[] columns = CsvUtil.SplitCsv(line, false);
            if (columns.Length < 8)
            {
                return false;
            }

            Transaction transaction = new Transaction();

            // 日付の処理
            string[] ary = columns[0].Split(new char[] { '/' });
            try
            {
                if (ary.Length == 3)
                {
                    transaction.date = new DateTime(int.Parse(ary[0]), int.Parse(ary[1]), int.Parse(ary[2]), 0, 0, 0);
                }
                else if (ary.Length == 2)
                {
                    DateTime now = DateTime.Now;

                    int n1 = int.Parse(ary[0]);
                    int n2 = int.Parse(ary[1]);

                    if (n1 >= 2000)
                    {
                        // 年と月のみ: 日は1日とする
                        transaction.date = new DateTime(n1, n2, 1, 0, 0, 0);
                    }
                    else
                    {
                        // 月と日のみ。年は推定する。
                        int mm = n1;
                        int dd = n2;

                        DateTime d = new DateTime(now.Year, mm, dd, 0, 0, 0);

                        // 同一年として、日付が６ヶ月以上先の場合、昨年とみなす。
                        // 逆に６ヶ月以上前の場合、翌年とみなす。
                        TimeSpan ts = d - now;
                        if (ts.TotalDays > 366 / 2)
                        {
                            d = new DateTime(now.Year - 1, mm, dd, 0, 0, 0);
                        }
                        else if (ts.TotalDays < -366 / 2)
                        {
                            d = new DateTime(now.Year + 1, mm, dd, 0, 0, 0);
                        }
                        transaction.date = d;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                // 日付が範囲外 (ArgumentRangeOutOfException など)
                return false;
            }

            // 摘要
            transaction.desc = columns[1];

            // 入金額/出金額
            try
            {
                transaction.value = int.Parse(columns[2]);
            }
            catch
            {
                try
                {
                    transaction.value = -int.Parse(columns[4]);
                }
                catch
                {
                    return false;
                }
            }

            // 残高
            try
            {
                transaction.balance = int.Parse(columns[6]);
            }
            catch
            {
                // Note: 残高は入っていない場合もある
                transaction.balance = 0;
            }

            // ID採番
            // TODO:
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
    }
}
