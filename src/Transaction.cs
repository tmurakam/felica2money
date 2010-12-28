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
        public const int UNASSIGNED_ID = -1;

        public int id = UNASSIGNED_ID; // ID
        public DateTime date;
        public TransType type;        // トランザクションタイプ
        public string desc = "";
        public string memo = "";
        public int value = 0;      // 金額
        public int balance = 0;    // 残高

        private bool mValid = true;

        private static Hashtable mTransIncome;
        private static Hashtable mTransOutgo;
        private static Hashtable mTransStrings;

        static Transaction()
        {
            // initialize
            mTransStrings = new Hashtable();
            mTransStrings[TransType.Int] = "INT";
            mTransStrings[TransType.Div] = "DIV";
            mTransStrings[TransType.DirectDep] = "DIRECTDEP";
            mTransStrings[TransType.Dep] = "DEP";
            mTransStrings[TransType.Payment] = "PAYMENT";
            mTransStrings[TransType.Cash] = "CASH";
            mTransStrings[TransType.ATM] = "ATM";
            mTransStrings[TransType.Check] = "CHECK";
            mTransStrings[TransType.Debit] = "DEBIT";

            mTransIncome = new Hashtable();
            mTransIncome["利息"] = TransType.Int;
            mTransIncome["振込"] = TransType.DirectDep;
            mTransIncome["ﾁｬｰｼﾞ"]= TransType.DirectDep;  // Edy チャージ
            mTransIncome["入金"] = TransType.DirectDep;    // Suica チャージ

            mTransOutgo = new Hashtable();
            mTransOutgo["ＡＴＭ"] = TransType.ATM;
            mTransOutgo["ATM"]    = TransType.ATM;
        }

        public string GetTransString()
        {
            return (string)mTransStrings[type];
        }

        public void GuessTransType(bool isIncome)
        {
            Hashtable h = mTransOutgo;

            if (isIncome)
            {
                h = mTransIncome;
            }

            foreach (string key in h.Keys)
            {
                if (desc != null && desc.Contains(key))
                {
                    type = (TransType)h[key];
                    return;
                }
            }

            // no match
            if (isIncome)
            {
                type = TransType.Dep;
            }
            else
            {
                type = TransType.Debit;
            }
        }

        public void Invalidate()
        {
            mValid = false;
        }

        public static bool isInvalid(Transaction t)
        {
            return !t.mValid;
        }

        public static bool isZeroTransaction(Transaction t)
        {
            return t.value == 0;
        }
    }
}
