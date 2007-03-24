using System;
using System.Collections.Generic;
using System.Text;

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

    class GuessTransTypeTable
    {
        private string key;
        private TransType type;

        public TransType Type
        {
            get { return type; }
        }

        public GuessTransTypeTable(string k, TransType t)
        {
            key = k;
            type = t;
        }

        public bool Match(string d)
        {
            if (d.Contains(key))
            {
                return true;
            }
            return false;
        }
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

        private static GuessTransTypeTable[] TransIncome = new GuessTransTypeTable[]
        {
            new GuessTransTypeTable("利息", TransType.Int),
            new GuessTransTypeTable("振込", TransType.DirectDep),
            new GuessTransTypeTable("ﾁｬｰｼﾞ", TransType.DirectDep),  // Edy チャージ
            new GuessTransTypeTable("入金", TransType.DirectDep)    // Suica チャージ
        };

        private static GuessTransTypeTable[] TransOutgo = new GuessTransTypeTable[]
        {
            new GuessTransTypeTable("ＡＴＭ", TransType.ATM),
            new GuessTransTypeTable("ATM", TransType.ATM)
        };

        public void GuessTransType(bool isIncome)
        {
            GuessTransTypeTable[] tab = TransOutgo;

            if (isIncome)
            {
                tab = TransIncome;
            }

            foreach (GuessTransTypeTable e in tab)
            {
                if (e.Match(desc))
                {
                    type = e.Type;
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
