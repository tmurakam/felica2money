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
using FelicaLib;

namespace FeliCa2Money
{
    class Edy : CardWithFelicaLib
    {
        public Edy()
        {
            org         = "Edy";
            cardName    = "Edy";

            systemCode  = (int)SystemCode.Edy;
            serviceCode = 0x170f;
            needReverse = true;
        }

        public override void analyzeCardId(Felica f)
        {
            byte[] data = f.ReadWithoutEncryption(0x110b, 0);
            if (data == null)
            {
                throw new Exception("Edy番号を読み取れません");
            }
            
            accountId = "";
            for (int i = 2; i < 10; i++) {
                accountId += data[i].ToString("X2");
            }
        }

        public override bool analyzeTransaction(Transaction t, byte[] data)
        {
            // 日付
            int value = read4b(data, 4);

            if (value == 0 && data[0] == 0)
            {
                return false; // おそらく空エントリ
            }

            t.date = new DateTime(2000, 1, 1);

            t.date += TimeSpan.FromDays(value >> 17);
            t.date += TimeSpan.FromSeconds(value & 0x1fff);

            // 金額
            t.value = read4b(data, 8);

            // 残高
            t.balance = read4b(data, 12);

            // 連番
            t.id = read3b(data, 1);

            // 種別
            switch (data[0])
            {
                case 0x20:
                default:
                    t.type = TransType.Debit;   // 支払い
                    t.desc = "支払";
                    t.value = - t.value;

                    // 適用が"支払" だけだと、Money が過去の履歴から店舗名を勝手に
                    // 補完してしまうので、連番を追加しておく。
                    t.desc += " ";
                    t.desc += t.id.ToString();
                    break;

                case 0x02:
                    t.type = TransType.DirectDep;
                    t.desc = "Edyチャージ";
                    break;

                case 0x04:
                    t.type = TransType.DirectDep;       
                    t.desc = "Edyギフト";
                    break;
            }
            t.memo = "";

            return true;
        }
    }
}
