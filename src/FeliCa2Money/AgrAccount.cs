// -*-  Mode:C++; c-basic-offset:4; tab-width:4; indent-tabs-mode:nil -*-
/*
 * FeliCa2Money
 *
 * Copyright (C) 2001-2015 Takuya Murakami
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
                account.IsCreditCard = isCreditCard;
                if (!account.readAccountInfo(line, mNameHash))
                {
                    return null;
                }
                return account;
            }
        }

        private AgrAccount()
        {
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

            if (!IsCreditCard)
            {
                // 銀行口座
                string bankName = columns[0];
                string branchName = columns[1];
                string accountId = columns[2];
                if (columns.Length >= 4)
                {
                    try
                    {
                        Balance = int.Parse(columns[3]);
                        HasBalance = true;
                    }
                    catch
                    {
                        HasBalance = false;
                    }
                }

                Ident = bankName;
                BankId = bankName;
                BranchId = branchName; // getDummyId(branchName).ToString();
                AccountId = accountId;
            }
            else
            {
                // クレジットカード
                string cardName = columns[0];
                try
                {
                    // 借入額
                    Balance = - int.Parse(columns[2]);
                    HasBalance = true;
                }
                catch
                {
                    HasBalance = false;
                }

                // 末尾の 'カード' という文字を抜く
                cardName = "CARD_" + Regex.Replace(cardName, @"カード$", "");


                // 2カラム目は空の模様
                //string balance = columns[2];
                Ident = "";
                BankId = "";
                BranchId = "";

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
                AccountId = cardName + counter.ToString();
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
                    transaction.Date = new DateTime(int.Parse(ary[0]), int.Parse(ary[1]), int.Parse(ary[2]), 0, 0, 0);
                }
                else if (ary.Length == 2)
                {
                    DateTime now = DateTime.Now;

                    int n1 = int.Parse(ary[0]);
                    int n2 = int.Parse(ary[1]);

                    if (n1 >= 2000)
                    {
                        // 年と月のみ: 日は1日とする
                        transaction.Date = new DateTime(n1, n2, 1, 0, 0, 0);
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
                        transaction.Date = d;
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
            transaction.Desc = columns[1];

            // 入金額/出金額
            try
            {
                transaction.Value = int.Parse(columns[2]);
            }
            catch
            {
                try
                {
                    transaction.Value = -int.Parse(columns[4]);
                }
                catch
                {
                    return false;
                }
            }

            // 残高
            try
            {
                transaction.Balance = int.Parse(columns[6]);
            }
            catch
            {
                // Note: 残高は入っていない場合もある
                transaction.Balance = 0;
            }

            Transactions.Add(transaction);

            return true;
        }
    }
}
