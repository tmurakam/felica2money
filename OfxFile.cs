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
using System.IO;

namespace FeliCa2Money
{
    class OfxFile
    {
        private string ofxFilePath;

        public OfxFile()
        {
        }

        public void SetOfxFilePath(String path)
        {
            ofxFilePath = path;
        }

        private string dateStr(DateTime d)
        {
            string s = String.Format("{0}{1:00}{2:00}", d.Year, d.Month, d.Day);
            s += String.Format("{0:00}{1:00}{2:00}", d.Hour, d.Minute, d.Second);
            s += "[+9:JST]";
            return s;
        }

        private string transId(Transaction t)
        {
            /* トランザクションの ID は日付と取引番号で生成 */
            string longId = String.Format("{0:0000}{1:00}{2:00}", t.date.Year, t.date.Month, t.date.Day);
            longId += String.Format("{0:0000000}", t.id);
            return longId;
        }

        public void WriteFile(Card card,  List<Transaction> transactions)
        {
            Transaction first = transactions[0];
            Transaction last = transactions[transactions.Count - 1];

            StreamWriter w = new StreamWriter(ofxFilePath, false); //, Encoding.UTF8);
            w.NewLine = "\n";

            w.WriteLine("OFXHEADER:100");
            w.WriteLine("DATA:OFXSGML");
            w.WriteLine("VERSION:102");
            w.WriteLine("SECURITY:NONE");
            w.WriteLine("ENCODING:UTF-8");
            w.WriteLine("CHARSET:CSUNICODE");
            w.WriteLine("COMPRESSION:NONE");
            w.WriteLine("OLDFILEUID:NONE");
            w.WriteLine("NEWFILEUID:NONE");
            w.WriteLine("");

            /* 金融機関情報(サインオンレスポンス) */
            w.WriteLine("<OFX>");
            w.WriteLine("<SIGNONMSGSRSV1>");
            w.WriteLine("<SONRS>");
            w.WriteLine("  <STATUS>");
            w.WriteLine("    <CODE>0");
            w.WriteLine("    <SEVERITY>INFO");
            w.WriteLine("  </STATUS>");
            w.WriteLine("  <DTSERVER>{0}", dateStr(last.date));

            w.WriteLine("  <LANGUAGE>JPN");
            w.WriteLine("  <FI>");
            w.WriteLine("    <ORG>{0}", card.Org);
            w.WriteLine("  </FI>");
            w.WriteLine("</SONRS>");
            w.WriteLine("</SIGNONMSGSRSV1>");

            /* 口座情報(バンクメッセージレスポンス) */
            w.WriteLine("<BANKMSGSRSV1>");

            /* 預金口座型明細情報作成 */
            w.WriteLine("<STMTTRNRS>");
            w.WriteLine("<TRNUID>0");
            w.WriteLine("<STATUS>");
            w.WriteLine("  <CODE>0");
            w.WriteLine("  <SEVERITY>INFO");
            w.WriteLine("</STATUS>");

            w.WriteLine("<STMTRS>");
            w.WriteLine("  <CURDEF>JPY");

            w.WriteLine("  <BANKACCTFROM>");
            w.WriteLine("    <BANKID>{0}", card.BankId);
            w.WriteLine("    <BRANCHID>{0}", card.BranchId);
            w.WriteLine("    <ACCTID>{0}", card.AccountId);
            w.WriteLine("    <ACCTTYPE>SAVINGS");
            w.WriteLine("  </BANKACCTFROM>");

            /* 明細情報開始(バンクトランザクションリスト) */
            w.WriteLine("  <BANKTRANLIST>");
            w.WriteLine("    <DTSTART>{0}", dateStr(first.date));
            w.WriteLine("    <DTEND>{0}", dateStr(last.date));

            /* トランザクション */
            foreach (Transaction t in transactions)
            {
                w.WriteLine("    <STMTTRN>");
                w.WriteLine("      <TRNTYPE>{0}", t.GetTransString());
                w.WriteLine("      <DTPOSTED>{0}", dateStr(t.date));
                w.WriteLine("      <TRNAMT>{0}", t.value);

                /* トランザクションの ID は日付と取引番号で生成 */
                w.WriteLine("      <FITID>{0}", transId(t));
                w.WriteLine("      <NAME>{0}", t.desc);
                if (t.memo != null)
                {
                    w.WriteLine("      <MEMO>{0}", t.memo);
                }
                w.WriteLine("    </STMTTRN>");
            }

            w.WriteLine("  </BANKTRANLIST>");

            /* 残高 */
            w.WriteLine("  <LEDGERBAL>");
            w.WriteLine("    <BALAMT>{0}", last.balance);
            w.WriteLine("    <DTASOF>{0}", dateStr(last.date));
            w.WriteLine("  </LEDGERBAL>");

            /* OFX 終了 */
            w.WriteLine("  </STMTRS>");
            w.WriteLine("</STMTTRNRS>");
            w.WriteLine("</BANKMSGSRSV1>");
            w.WriteLine("</OFX>");

            w.Close();

        }

        public void Execute()
        {
            System.Diagnostics.Process.Start(ofxFilePath);
        }
    }
}
