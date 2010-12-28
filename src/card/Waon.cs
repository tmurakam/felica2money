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

// WAON (試作中)

using System;
using System.Collections.Generic;
using System.Text;
using FelicaLib;

namespace FeliCa2Money
{
    public class Waon : FelicaCard
    {
        public Waon()
        {
            mIdent       = "WAON";
            mAccountName    = "WAON";

            systemCode  = (int)SystemCode.Common;
            serviceCode = 0x680b;

            needReverse = false;
            needCalcValue = false;

            blocksPerTransaction = 2;  // 2ブロックで１履歴
            maxTransactions = 3; // 履歴数は３
        }

        public override bool analyzeCardId(IFelica f)
        {
            byte[] data = f.ReadWithoutEncryption(0x67cf, 0);
            byte[] data2 = f.ReadWithoutEncryption(0x67cf, 1);
            if (data == null || data2 == null)
            {
                return false;
            }

            mAccountId = binString(data, 12, 4) + binString(data2, 0, 4);

            return true;
        }

        // 履歴連番でソートする
        protected override void PostProcess(List<Transaction> list)
        {
            list.Sort(compareById);
        }

        private static int compareById(Transaction x, Transaction y)
        {
            int ret = x.id - y.id;
            // 周回したときの処理
            if (ret < - 0x8000)
            {
                ret += 0x10000;
            }
            else if (ret > 0x8000)
            {
                ret -= 0x10000;
            }
            return ret;
        }

        // トランザクション解析
        public override bool analyzeTransaction(Transaction t, byte[] data)
        {
            // ID
            t.id = read2b(data, 13);

            // 日付
            int x = read4b(data, 18);
            int yy = x >> 27;
            int mm = (x >> 23) & 0xf;
            int dd = (x >> 18) & 0x1f;
            int hh = (x >> 13) & 0x1f;
            int min = (x >> 7) & 0x3f;
            t.date = new DateTime(yy + 2005, mm, dd, hh, min, 0);

            // 残高
            x = read3b(data, 21);
            t.balance = (x >> 5) & 0x3ffff;

            // 出金額
            x = read3b(data, 23);
            t.value = -((x >> 3) & 0x3ffff);

            // 入金額
            x = read3b(data, 25);
            t.value += (x >> 2) & 0x1ffff;

            // 適用
            switch (data[17])
            {
                case 0x0c:
                case 0x10:
                    t.desc = "WAONチャージ";
                    break;

                case 0x04:
                default:
                    t.desc = "WAON支払";
                    break;
            }
            // TBD : 0-12 に備考が入っているのでこちらを使うべきか？
            
            // トランザクションタイプを自動設定
            t.GuessTransType(t.value >= 0);

            return true;
        }
    }
}
