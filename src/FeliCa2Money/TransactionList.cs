using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FeliCa2Money
{
    public class TransactionList : List<Transaction>
    {
        // 無効な取引を削除する
        public void RemoveInvalidTransactions()
        {
            RemoveAll(Transaction.isInvalid);
        }
        
        // 0円の取引を削除する
        public void RemoveZeroTransactions()
        {
            if (Properties.Settings.Default.IgnoreZeroTransaction)
            {
                RemoveAll(Transaction.isZeroTransaction);
            }
        }

        // シリアル番号の採番
        // ID が付与されていない取引について、同一日付内でシリアル番号を採番する。
        // Note: mList は日付順にソートされている必要がある
        public void AssignSerials()
        {
            int serial = 0;
            var prevDate = new DateTime(1900, 1, 1, 0, 0, 0);

            foreach (Transaction t in this)
            {
                if (t.isIdUnassigned())
                {
                    if (t.date == prevDate)
                    {
                        serial++;
                    }
                    else
                    {
                        serial = 0;
                        prevDate = t.date;
                    }
                    t.serial = serial;
                }
            }
        }
    }
}
