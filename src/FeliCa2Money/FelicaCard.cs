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
using FelicaLib;

namespace FeliCa2Money
{
    /// <summary>
    /// FeliCa カードクラス
    /// </summary>
    public abstract class FelicaCard : Account, IDisposable
    {
        protected int _systemCode;   // システムコード
        protected int _serviceCode;  // サービスコード
        protected int _blocksPerTransaction = 1;  // 1トランザクションあたりのブロック数
        protected int _maxTransactions = 100;     // 最大トランザクション数
        protected bool _needReverse = false;      // レコード順序を逆転するかどうか
        protected bool _needCalcValue = false;     // 入出金額を残高から計算するかどうか

        //--------------------------------------------------------------------
        // 以下のメソッドはサブクラスで必要に応じてオーバライドする

        /// <summary>
        /// Transaction 解析
        /// </summary>
        /// <param name="t"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract bool AnalyzeTransaction(Transaction t, byte[] data);

        /// <summary>
        /// 後処理
        /// </summary>
        /// <param name="transactions"></param>
        protected virtual void PostProcess(TransactionList transactions) { }

        /// <summary>
        /// Dispose 処理
        /// </summary>
        public virtual void Dispose() { }

        /// <summary>
        /// カード ID 取得
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public virtual bool AnalyzeCardId(IFelica f)
        {
            // デフォルトでは、IDm を用いる。
            var data = f.IDm();
            if (data == null)
            {
                return false;
            }

            AccountId = BinString(data, 0, 8);

            return true;
        }

        //--------------------------------------------------------------------

        /// <summary>
        /// カード読み込み
        /// </summary>
        public sealed override void ReadTransactions()
        {
            using (IFelica f = new Felica()) {
                Transactions = ReadTransactions(f);
            }
        }

        /// <summary>
        /// カード読み込み
        /// Note: 本来はこのメソッドは private で良いが、UnitTest 用に public にしてある。
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public TransactionList ReadTransactions(IFelica f)
        {
            var transactions = new TransactionList();

            f.Polling(_systemCode);

            if (!AnalyzeCardId(f)) {
                throw new Exception(Properties.Resources.CantReadCardNo);
            }

            for (var i = 0; i < _maxTransactions; i++)
            {
                var data = new byte[16 * _blocksPerTransaction];
                byte[] block = null;

                for (var j = 0; j < _blocksPerTransaction; j++)
                {
                    block = f.ReadWithoutEncryption(_serviceCode, i * _blocksPerTransaction + j);
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
                var x = 0;
                foreach (var xx in data)
                {
                    x |= xx;
                }
                if (x == 0) 
                {
                    // データが全0なら無視(空エントリ)
                    t.Invalidate();
                }

                // トランザクション解析
                else if (!AnalyzeTransaction(t, data))
                {
                    t.Invalidate();
                }
                transactions.Add(t);
            }

            if (_needReverse)
            {
                transactions.Reverse();
            }
            if (_needCalcValue)
            {
                CalcValueFromBalance(transactions);
            }
            PostProcess(transactions);

            return transactions;
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
        protected string BinString(byte[] data, int offset, int len)
        {
            var s = "";
            for (var i = offset; i < offset + len; i++)
            {
                s += data[i].ToString("X2");
            }
            return s;
        }

        // 残高から金額を計算する
        private void CalcValueFromBalance(TransactionList transactions)
        {
            var prevBalance = 0;

            foreach (var t in transactions)
            {
                t.Value = t.Balance - prevBalance;
                prevBalance = t.Balance;
            }
            transactions.RemoveAt(0);   // 最古のエントリは捨てる
        }

        // 複数バイト読み込み (big endian)
        protected int Read2B(byte[] b, int pos)
        {
            var ret = b[pos] << 8 | b[pos + 1];
            return ret;
        }

        protected int Read3B(byte[] b, int pos)
        {
            var ret = b[pos] << 16 | b[pos + 1] << 8 | b[pos + 2];
            return ret;
        }

        protected int Read4B(byte[] b, int pos)
        {
            var ret = b[pos] << 24 | b[pos + 1] << 16 | b[pos + 2] << 8 | b[pos + 3];
            return ret;
        }

        // little endian
        protected int Read2L(byte[] b, int pos)
        {
            var ret = b[pos + 1] << 8 | b[pos];
            return ret;
        }
    }
}
