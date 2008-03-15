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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace FeliCa2Money
{
    class CsvCard : Card
    {
        private CsvRules rules = new CsvRules();
        private StreamReader sr;
        private CsvRule rule;

        public CsvCard()
        {
            rules.LoadFromFile("csvrule.xml");
        }

        public void OpenFile(string path)
        {
            // ###
            rules.LoadFromFile("csvrule.xml");
            sr = new StreamReader(path, System.Text.Encoding.Default);

            string firstLine = sr.ReadLine();

            // 合致するルールを探す
            rule = rules.FindRule(firstLine);
            if (rule == null)
            {
                throw new Exception("未知のCSVフォーマットです");
            }

            // 銀行IDを設定
            ident = rule.Ident;

            // TODO: 支店番号, 口座番号をここで指定
        }

        public void Close()
        {
            sr.Close();
        }
            
        public override List<Transaction> ReadCard()
        {
            List<Transaction> transactions = new List<Transaction>();
            string line;

            while ((line = sr.ReadLine()) != null)
            {
                // ad hoc...
                string[] row = line.Split(new char[] { ',' });
                if (row.Length <= 1) continue; // ad hoc...

                Transaction t = rule.parse(row);
                transactions.Add(t);
            }

            if (!rule.IsAscent)
            {
                transactions.Reverse();
            }

            return transactions;
        }
    }
}
