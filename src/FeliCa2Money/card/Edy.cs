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
    public class Edy : FelicaCard
    {
        public Edy()
        {
            Ident         = "Edy";
            AccountName    = "Edy";

            _systemCode  = (int)SystemCode.Edy;
            _serviceCode = 0x170f;
            _needReverse = true;
            _needCalcValue = false;
        }

        public override bool AnalyzeCardId(IFelica f)
        {
            byte[] data = f.ReadWithoutEncryption(0x110b, 0);
            if (data == null)
            {
                return false;
            }

            AccountId = BinString(data, 2, 8);

            return true;
        }

        public override bool AnalyzeTransaction(Transaction t, byte[] data)
        {
            // 日付
            int value = Read4B(data, 4);

            if (value == 0 && data[0] == 0)
            {
                return false; // おそらく空エントリ
            }

            t.Date = new DateTime(2000, 1, 1);

            t.Date += TimeSpan.FromDays(value >> 17);
            t.Date += TimeSpan.FromSeconds(value & 0x1ffff);

            // 金額
            t.Value = Read4B(data, 8);

            // 残高
            t.Balance = Read4B(data, 12);

            // 連番
            t.Id = Read3B(data, 1);

            // 種別
            switch (data[0])
            {
                case 0x20:
                default:
                    t.Type = TransType.Debit;   // 支払い
                    t.Desc = "支払";
                    t.Value = - t.Value;

                    // 適用が"支払" だけだと、Money が過去の履歴から店舗名を勝手に
                    // 補完してしまうので、連番を追加しておく。
                    t.Desc += " ";
                    t.Desc += t.Id.ToString();
                    break;

                case 0x02:
                    t.Type = TransType.DirectDep;
                    t.Desc = "Edyチャージ";
                    break;

                case 0x04:
                    t.Type = TransType.DirectDep;       
                    t.Desc = "Edyギフト";
                    break;
            }
            t.Memo = "";

            return true;
        }
    }
}
