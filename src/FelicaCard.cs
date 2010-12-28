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
using FelicaLib;

namespace FeliCa2Money
{
    /// <summary>
    /// FeliCa カードクラス
    /// </summary>
    public abstract class FelicaCard : Account, IDisposable
    {
        protected int mSystemCode;   // システムコード
        protected int mServiceCode;  // サービスコード
        protected int mBlocksPerTransaction = 1;  // 1トランザクションあたりのブロック数
        protected int mMaxTransactions = 100;     // 最大トランザクション数
        protected bool mNeedReverse = false;      // レコード順序を逆転するかどうか
        protected bool mNeedCalcValue = false;     // 入出金額を残高から計算するかどうか

        //--------------------------------------------------------------------
        // 以下のメソッドはサブクラスで必要に応じてオーバライドする

        /// <summary>
        /// Transaction 解析
        /// </summary>
        /// <param name="t"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract bool analyzeTransaction(Transaction t, byte[] data);

        /// <summary>
        /// 後処理
        /// </summary>
        /// <param name="list"></param>
        protected virtual void PostProcess(List<Transaction> list) { }

        /// <summary>
        /// Dispose 処理
        /// </summary>
        public virtual void Dispose() { }

        /// <summary>
        /// カード ID 取得
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public virtual bool analyzeCardId(IFelica f)
        {
            // デフォルトでは、IDm を用いる。
            byte[] data = f.IDm();
            if (data == null)
            {
                return false;
            }

            mAccountId = binString(data, 0, 8);

            return true;
        }

        //--------------------------------------------------------------------

        /// <summary>
        /// カード読み込み
        /// </summary>
        public sealed override void ReadCard()
        {
            using (IFelica f = new Felica()) {
                mTransactions = ReadCard(f);
            }
        }

        /// <summary>
        /// カード読み込み
        /// Note: 本来はこのメソッドは private で良いが、UnitTest 用に public にしてある。
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public List<Transaction> ReadCard(IFelica f)
        {
            List<Transaction> list = new List<Transaction>();

            f.Polling(mSystemCode);

            if (!analyzeCardId(f)) {
                throw new Exception(Properties.Resources.CantReadCardNo);
            }

            for (int i = 0; i < mMaxTransactions; i++)
            {
                byte[] data = new byte[16 * mBlocksPerTransaction];
                byte[] block = null;

                for (int j = 0; j < mBlocksPerTransaction; j++)
                {
                    block = f.ReadWithoutEncryption(mServiceCode, i * mBlocksPerTransaction + j);
                    if (block == null)
                    {
                        break;
                    }

                    block.CopyTo(data, j * 16);
                }
                if (block == null)
                {
                    break;
                }

                Transaction t = new Transaction();
                    
                // データが全0かどうかチェック
                int x = 0;
                foreach (int xx in data)
                {
                    x |= xx;
                }
                if (x == 0) 
                {
                    // データが全0なら無視(空エントリ)
                    t.Invalidate();
                }

                // トランザクション解析
                else if (!analyzeTransaction(t, data))
                {
                    t.Invalidate();
                }
                list.Add(t);
            }

            if (mNeedReverse)
            {
                list.Reverse();
            }
            if (mNeedCalcValue)
            {
                CalcValueFromBalance(list);
            }
            PostProcess(list);

            return list;
        }

        //-------------------------------------------------------------------
        // ユーティリティ

        /// <summary>
        /// バイナリデータを16進文字列に変換
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        protected string binString(byte[] data, int offset, int len)
        {
            string s = "";
            for (int i = offset; i < offset + len; i++)
            {
                s += data[i].ToString("X2");
            }
            return s;
        }

        // 残高から金額を計算する
        private void CalcValueFromBalance(List<Transaction> list)
        {
            int prevBalance = 0;

            foreach (Transaction t in list)
            {
                t.value = t.balance - prevBalance;
                prevBalance = t.balance;
            }
            list.RemoveAt(0);   // 最古のエントリは捨てる
        }

        // 複数バイト読み込み (big endian)
        protected int read2b(byte[] b, int pos)
        {
            int ret = b[pos] << 8 | b[pos + 1];
            return ret;
        }

        protected int read3b(byte[] b, int pos)
        {
            int ret = b[pos] << 16 | b[pos + 1] << 8 | b[pos + 2];
            return ret;
        }

        protected int read4b(byte[] b, int pos)
        {
            int ret = b[pos] << 24 | b[pos + 1] << 16 | b[pos + 2] << 8 | b[pos + 3];
            return ret;
        }

        // little endian
        protected int read2l(byte[] b, int pos)
        {
            int ret = b[pos + 1] << 8 | b[pos];
            return ret;
        }
    }
}
