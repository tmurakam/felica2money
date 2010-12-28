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
            mIdent       = "Nanaco";
            mAccountName    = "Nanaco";

            systemCode  = (int)SystemCode.Common;
            serviceCode = 0x564f;
            needReverse = true;
            needCalcValue = false;
        }

        public override bool analyzeCardId(IFelica f)
        {
            byte[] data = f.ReadWithoutEncryption(0x558b, 0);
            if (data == null)
            {
                return false;
            }

            mAccountId = binString(data, 0, 8);

            return true;
        }

        public override bool analyzeTransaction(Transaction t, byte[] data)
        {
            // 日付
            int value = read4b(data, 9);
            int year = (value >> 21) + 2000;
            int month = (value >> 17) & 0xf;
            int date = (value >> 12) & 0x1f;
            int hour = (value >> 6) & 0x3f;
            int min = value & 0x3f;
            t.date = new DateTime(year, month, date, hour, min, 0);

            // 金額
            value = read4b(data, 1);

            // 種別
            t.type = TransType.DirectDep;
            t.value = value;

            switch (data[0])
            {
                default:
                case 0x47:
                    t.type = TransType.Debit;   // 支払い
                    t.desc = "nanaco支払";
                    t.value = - value;
                    break;

                case 0x35:  
                    t.desc = "引継";
                    break;

                case 0x6f:
                case 0x70:
                    t.desc = "nanacoチャージ";
                    break;

                case 0x83:
                    t.desc = "nanacoポイント交換";
                    break;
            }
            t.memo = "";

            // 残高
            value = read4b(data, 5);
            t.balance = value;

            // 連番
            value = read2b(data, 13);
            t.id = value;

            return true;
        }
    }
}
