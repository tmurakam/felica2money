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
        public int id; // ID
        public DateTime date;
        public TransType type;        // トランザクションタイプ
        public string desc;
        public string memo;
        public int value;      // 金額
        public int balance;    // 残高

        private static Hashtable TransIncome;
        private static Hashtable TransOutgo;
        private static Hashtable TransStrings;

        public Transaction()
        {
            if (TransStrings != null) return;

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
    }
}
