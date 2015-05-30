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

        // シリアル番号の採番
        // ID が付与されていない取引について、同一日付内でシリアル番号を採番する。
        // Note: mList は日付順にソートされている必要がある
        public void assignSerials()
        {
            int serial = 0;
            DateTime prevDate = new DateTime(1900, 1, 1, 0, 0, 0);

            foreach (Transaction t in mList)
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
