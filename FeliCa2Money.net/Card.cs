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
    abstract class Card
    {
        protected string ident;
        protected string cardName;
        protected string cardId;

        public abstract List<Transaction> ReadCard();

        public string Ident
        {
            get { return this.ident; }
        }

        public string CardName
        {
            get { return this.cardName; }
        }
        
        public string CardId
        {
            set { this.cardId = value; }
            get { return this.cardId; }
        }

        protected string[] ParseLine(string line)
        {
            return line.Split('\t');
        }
    }

    abstract class CardWithFelicaLib : Card
    {
        protected int systemCode;   // システムコード
        protected int serviceCode;  // サービスコード
        protected bool needReverse; // レコード順序を逆転するかどうか

        // カード ID 取得
        public abstract void analyzeCardId(Felica f);

        // Transaction 解析
        public abstract void analyzeTransaction(Transaction t, byte[] data);

        public override List<Transaction> ReadCard()
        {
            List<Transaction> list = new List<Transaction>();

            using (Felica f = new Felica())
            {
                f.Polling(systemCode);
                analyzeCardId(f);

                for (int i = 0; ; i++)
                {
                    byte[] data = f.ReadWithoutEncryption(serviceCode, i);
                    if (data == null) break;

                    Transaction t = new Transaction();
                    analyzeTransaction(t, data);

                    list.Add(t);
                }
            }
            if (needReverse)
            {
                list.Reverse();
            }

            return list;
        }
    }
}
