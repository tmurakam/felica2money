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
    public class Nanaco : FelicaCard
    {
        public    Nanaco()
        {
            Ident       = "Nanaco";
            AccountName    = "Nanaco";

            _systemCode  = (int)SystemCode.Common;
            _serviceCode = 0x564f;
            _needReverse = true;
            _needCalcValue = false;
        }

        public override bool AnalyzeCardId(IFelica f)
        {
            byte[] data = f.ReadWithoutEncryption(0x558b, 0);
            if (data == null)
            {
                return false;
            }

            AccountId = BinString(data, 0, 8);

            return true;
        }

        public override bool AnalyzeTransaction(Transaction t, byte[] data)
        {
            // 日付
            int value = Read4B(data, 9);
            int year = (value >> 21) + 2000;
            int month = (value >> 17) & 0xf;
            int date = (value >> 12) & 0x1f;
            int hour = (value >> 6) & 0x3f;
            int min = value & 0x3f;
            t.Date = new DateTime(year, month, date, hour, min, 0);

            // 金額
            value = Read4B(data, 1);

            // 種別
            t.Type = TransType.DirectDep;
            t.Value = value;

            switch (data[0])
            {
                default:
                case 0x47:
                    t.Type = TransType.Debit;   // 支払い
                    t.Desc = "nanaco支払";
                    t.Value = - value;
                    break;

                case 0x35:  
                    t.Desc = "引継";
                    break;

                case 0x6f:
                case 0x70:
                    t.Desc = "nanacoチャージ";
                    break;

                case 0x83:
                    t.Desc = "nanacoポイント交換";
                    break;
            }
            t.Memo = "";

            // 残高
            value = Read4B(data, 5);
            t.Balance = value;

            // 連番
            value = Read2B(data, 13);
            t.Id = value;

            return true;
        }
    }
}
