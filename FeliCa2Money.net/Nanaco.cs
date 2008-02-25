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
    class Nanaco : CardWithFelicaLib
    {
        public    Nanaco()
        {
            ident       = "Nanaco";
            cardName    = "Nanaco";

            systemCode  = (int)SystemCode.Common;
            serviceCode = 0x564f;
            needReverse = true;
        }

        public override void analyzeCardId(Felica f)
        {
            byte[] data = f.ReadWithoutEncryption(0x558b, 0);
            if (data == null)
            {
                throw new Exception("nanaco番号を読み取れません");
            }
            
            cardId = "";
            for (int i = 0; i < 8; i++) {
                cardId += data[i].ToString("X2");
            }
        }

        public override void analyzeTransaction(Transaction t, byte[] data)
        {
            // 日付
            int value = (data[9] << 24) + (data[10] << 16) + (data[11] << 8) + data[12];
            int year = (value >> 21) + 2000;
            int month = (value >> 17) & 0xf;
            int date = (value >> 12) & 0x1f;
            int hour = (value >> 6) & 0x3f;
            int min = value & 0x3f;
            t.date = new DateTime(year, month, date, hour, min, 0);

            // 金額
            value = (data[1] << 24) + (data[2] << 16) + (data[3] << 8) + data[4];

            // 種別
            switch (data[0])
            {
                case 0x47:
                default:
                    t.type = TransType.Debit;   // 支払い
                    t.desc = "nanaco支払";
                    t.value = - value;
                    break;
                case 0x6f:
                    t.type = TransType.DirectDep;    // チャージ
                    t.desc = "nanacoチャージ";
                    t.value = value;
                    break;
            }
            t.memo = "";

            // 残高
            value = (data[5] << 24) + (data[6] << 16) + (data[7] << 8) + data[8];
            t.balance = value;

            // 連番
            value = (data[13] << 8) + data[14];
            t.id = value;
        }
    }
}
