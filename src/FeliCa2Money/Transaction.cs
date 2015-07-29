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

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace FeliCa2Money
{
    public enum TransType
    {
        Int,    // 利息
        Div,    // 配当
        DirectDep,  // 振り込み入金、取り立て入金、自動引き落とし戻し入金
        Dep,    // その他入金

        Payment,
        Cash,
        ATM,
        Check,
        Debit       // その他出金
    }

    public class Transaction
    {
        public const int UnassignedId = -1;

        // 取引ID生成用のシリアル番号。mId が UNASSIGNED_ID の場合にのみ使用

        private bool mValid = true;

        private static readonly Dictionary<string,TransType> mTransIncome;
        private static readonly Dictionary<string,TransType> mTransOutgo;

        private static readonly Dictionary<TransType,string> mTransStrings;

        private static System.Security.Cryptography.MD5 sMd5 = new System.Security.Cryptography.MD5CryptoServiceProvider();

        // プロパティ
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public TransType Type { get; set; }

        public string Desc { get; set; }

        public string Memo { get; set; }

        public int Value { get; set; }

        public int Balance { get; set; }

        public int Serial { get; set; }

        public bool IsIdUnassigned()
        {
            return (Id == UnassignedId);
        }
    

        // 初期化
        static Transaction()
        {
            // initialize
            mTransStrings = new Dictionary<TransType, string>();
            mTransStrings[TransType.Int] = "INT";
            mTransStrings[TransType.Div] = "DIV";
            mTransStrings[TransType.DirectDep] = "DIRECTDEP";
            mTransStrings[TransType.Dep] = "DEP";
            mTransStrings[TransType.Payment] = "PAYMENT";
            mTransStrings[TransType.Cash] = "CASH";
            mTransStrings[TransType.ATM] = "ATM";
            mTransStrings[TransType.Check] = "CHECK";
            mTransStrings[TransType.Debit] = "DEBIT";

            mTransIncome = new Dictionary<string, TransType>();
            mTransIncome["利息"] = TransType.Int;
            mTransIncome["振込"] = TransType.DirectDep;
            mTransIncome["ﾁｬｰｼﾞ"]= TransType.DirectDep;  // Edy チャージ
            mTransIncome["入金"] = TransType.DirectDep;    // Suica チャージ

            mTransOutgo = new Dictionary<string, TransType>();
            mTransOutgo["ＡＴＭ"] = TransType.ATM;
            mTransOutgo["ATM"]    = TransType.ATM;
        }

        public Transaction()
        {
            Serial = 0;
            Balance = 0;
            Value = 0;
            Memo = "";
            Desc = "";
            Id = UnassignedId;
        }

        public string GetTransString()
        {
            return (string)mTransStrings[Type];
        }

        public void GuessTransType(bool isIncome)
        {
            var h = mTransOutgo;

            if (isIncome)
            {
                h = mTransIncome;
            }

            foreach (var key in h.Keys)
            {
                if (Desc != null && Desc.Contains(key))
                {
                    Type = h[key];
                    return;
                }
            }

            // no match
            Type = isIncome ? TransType.Dep : TransType.Debit;
        }

        public void Invalidate()
        {
            mValid = false;
        }

        public static bool IsInvalid(Transaction t)
        {
            return !t.mValid;
        }

        public static bool IsZeroTransaction(Transaction t)
        {
            return t.Value == 0;
        }

        //
        // トランザクションID文字列の生成
        //
        public string TransId()
        {
            string tid;
            if (!IsIdUnassigned())
            {
                /* トランザクションの ID は日付と取引番号で生成 */
                tid = string.Format("{0:0000}{1:00}{2:00}", Date.Year, Date.Month, Date.Day);
                tid += string.Format("{0:0000000}", Id);
            }
            else
            {
                /* 日付とハッシュで生成 */
                tid = MakeHash();
            }
            return tid;
        }

        // トランザクションの hash を生成する
        private string MakeHash()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0};{1};{2};", Date.Year, Date.Month, Date.Day);
            sb.AppendFormat("{0};{1};{2};{3}", Serial, Value, Desc, Memo);

            // MD5 ハッシュを計算
            var hash = sMd5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));

            // 16進文字列に変換
            var result = new StringBuilder();
            foreach (var b in hash)
            {
                result.Append(b.ToString("x2"));
            }
            return result.ToString();
        }
    }
}
