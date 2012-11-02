using System;
using System.Collections.Generic;
using System.Text;

namespace FeliCa2Money
{
    public class TransactionList
    {
        private List<Transaction> mList = new List<Transaction>();

        public List<Transaction> list
        {
            get { return mList; }
        }

        // イテレータ
        public IEnumerator<Transaction> GetEnumerator()
        {
            return mList.GetEnumerator();
        }

        // 指定位置の取引を返す
        public Transaction getAt(int index)
        {
            return mList[index];
        }

        // 取引を追加
        public void Add(Transaction t)
        {
            mList.Add(t);
        }

        // 取引数を返す
        public int Count
        {
            get { return mList.Count; }
        }

        // 逆順にする
        public void Reverse() {
            mList.Reverse();
        }

        // 無効な取引を削除する
        public void removeInvalidTransactions()
        {
            mList.RemoveAll(Transaction.isInvalid);
        }
        
        // 0円の取引を削除する
        public void removeZeroTransactions()
        {
            if (Properties.Settings.Default.IgnoreZeroTransaction)
            {
                mList.RemoveAll(Transaction.isZeroTransaction);
            }
        }

        // トランザクションIDの採番
        public void assignTransactionId()
        {
            int idSerial = 0;
            DateTime prevDate = new DateTime(1900, 1, 1, 0, 0, 0);

            foreach (Transaction t in mList)
            {
                if (t.isIdUnassigned())
                {
                    if (t.date == prevDate)
                    {
                        idSerial++;
                    }
                    else
                    {
                        idSerial = 0;
                        prevDate = t.date;
                    }
                    t.id = idSerial;
                }
            }
        }
    }
}
