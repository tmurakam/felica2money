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
using System.Linq;
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
        enum State
        {
            SearchingStart,
            ReadAccountInfo,
            ReadTransactions
        };

        /// <summary>
        /// アカウントリスト
        /// </summary>
        public List<Account> Accounts { get; private set; }

        /// <summary>
        /// AGRファイルを読み込む
        /// </summary>
        /// <param name="path">AGRファイルパス</param>
        /// <returns>成功時は true、失敗時はfalse</returns>
        public bool LoadFromFile(string path)
        {
            // SJIS で開く
            StreamReader sr = new StreamReader(path, System.Text.Encoding.Default);
            return Load(sr);
        }

        public bool Load(StreamReader sr) 
        {
            try
            {
                // フォーマットチェック
                string line = sr.ReadLine();
                if (line != "\"あぐりっぱ\",\"1.0\"")
                {
                    return false;
                }

                Accounts = new List<Account>();
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
                                Accounts.Add(account);
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
        public int CountTransactions()
        {
            return Accounts.Sum(account => account.Transactions.Count);
        }
    }
}
