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

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace FeliCa2Money
{
    enum TransType
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

    class Transaction
    {
        public int id = -1; // ID
        public DateTime date;
        public TransType type;        // トランザクションタイプ
        public string desc = "";
        public string memo = "";
        public int value = 0;      // 金額
        public int balance = 0;    // 残高

        private bool valid = true;

        private static Hashtable TransIncome;
        private static Hashtable TransOutgo;
        private static Hashtable TransStrings;

        static Transaction()
        {
            // initialize
            TransStrings = new Hashtable();
            TransStrings[TransType.Int] = "INT";
            TransStrings[TransType.Div] = "DIV";
            TransStrings[TransType.DirectDep] = "DIRECTDEP";
            TransStrings[TransType.Dep] = "DEP";
            TransStrings[TransType.Payment] = "PAYMENT";
            TransStrings[TransType.Cash] = "CASH";
            TransStrings[TransType.ATM] = "ATM";
            TransStrings[TransType.Check] = "CHECK";
            TransStrings[TransType.Debit] = "DEBIT";

            TransIncome = new Hashtable();
            TransIncome["利息"] = TransType.Int;
            TransIncome["振込"] = TransType.DirectDep;
            TransIncome["ﾁｬｰｼﾞ"]= TransType.DirectDep;  // Edy チャージ
            TransIncome["入金"] = TransType.DirectDep;    // Suica チャージ

            TransOutgo = new Hashtable();
            TransOutgo["ＡＴＭ"] = TransType.ATM;
            TransOutgo["ATM"]    = TransType.ATM;
        }

        public string GetTransString()
        {
            return (string)TransStrings[type];
        }

        public void GuessTransType(bool isIncome)
        {
            Hashtable h = TransOutgo;

            if (isIncome)
            {
                h = TransIncome;
            }

            foreach (string key in h.Keys)
            {
                if (desc.Contains(key))
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
            valid = false;
        }

        public static bool isInvalid(Transaction t)
        {
            return !t.valid;
        }

        public static bool isZeroTransaction(Transaction t)
        {
            return t.value == 0;
        }
    }
}
