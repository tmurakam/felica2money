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

/*
   OFX ファイル生成
   ファイルバージョンは 1.0.2 (SGML)
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace FeliCa2Money
{
    class OfxFile
    {
        protected string mOfxFilePath;

        protected Transaction mAllFirst, mAllLast;

        public OfxFile()
        {
        }

        public void SetOfxFilePath(String path)
        {
            mOfxFilePath = path;
        }

        protected string dateStr(DateTime d)
        {
            string s = String.Format("{0}{1:00}{2:00}", d.Year, d.Month, d.Day);
            s += String.Format("{0:00}{1:00}{2:00}", d.Hour, d.Minute, d.Second);
            s += "[+9:JST]";
            return s;
        }

        protected string transId(Transaction t)
        {
            /* トランザクションの ID は日付と取引番号で生成 */
            string longId = String.Format("{0:0000}{1:00}{2:00}", t.date.Year, t.date.Month, t.date.Day);
            longId += String.Format("{0:0000000}", t.id);
            return longId;
        }

        protected string quoteString(string s)
        {
            s = s.Replace("&", "&amp;");
            s = s.Replace("<", "&lt;");
            s = s.Replace(">", "&gt;");
            //s = s.Replace("'", "&apos;");
            //s = s.Replace("\"", "&quot;");
            return s;
        }

        // 最初のトランザクションと最後のトランザクションを取り出しておく
        // (日付範囲取得のため)
        protected void getFirstLastDate(List<Account> cards)
        {
            mAllFirst = null;
            mAllLast = null;
            foreach (Account card in cards) {
                Transaction t;
                if (card.transactions.Count == 0) continue;

                t = card.transactions[0];
                if (mAllFirst == null || t.date < mAllFirst.date)
                {
                    mAllFirst = t;
                }

                t = card.transactions[card.transactions.Count - 1];
                if (mAllLast == null || t.date > mAllLast.date)
                {
                    mAllLast = t;
                }
            }

        }

        public void WriteFile(Account card)
        {
            List<Account> cards = new List<Account>();
            cards.Add(card);
            WriteFile(cards);
        }

        public virtual void WriteFile(List<Account> cards)
        {
            getFirstLastDate(cards);

            StreamWriter w = new StreamWriter(mOfxFilePath, false); //, Encoding.UTF8);
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
            w.WriteLine("  <DTSERVER>{0}", dateStr(mAllLast.date));

            w.WriteLine("  <LANGUAGE>JPN");
            w.WriteLine("  <FI>");
            if (cards.Count == 1)
            {
                w.WriteLine("    <ORG>{0}", cards[0].ident);
            }
            else
            {
                // 複数アカウントなので、組織名は固定
                w.WriteLine("    <ORG>FeliCa2Money");
            }
            w.WriteLine("  </FI>");
            w.WriteLine("</SONRS>");
            w.WriteLine("</SIGNONMSGSRSV1>");

            /* 口座情報(バンクメッセージレスポンス) */
            w.WriteLine("<BANKMSGSRSV1>");

            foreach (Account card in cards)
            {
                if (card.transactions.Count == 0) continue; // no transactions

                Transaction first = card.transactions[0];
                Transaction last = card.transactions[card.transactions.Count - 1];

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
                w.WriteLine("    <BANKID>{0}", card.bankId);
                w.WriteLine("    <BRANCHID>{0}", card.branchId);
                w.WriteLine("    <ACCTID>{0}", card.accountId);
                w.WriteLine("    <ACCTTYPE>SAVINGS");
                w.WriteLine("  </BANKACCTFROM>");

                /* 明細情報開始(バンクトランザクションリスト) */
                w.WriteLine("  <BANKTRANLIST>");
                w.WriteLine("    <DTSTART>{0}", dateStr(first.date));
                w.WriteLine("    <DTEND>{0}", dateStr(last.date));

                /* トランザクション */
                foreach (Transaction t in card.transactions)
                {
                    w.WriteLine("    <STMTTRN>");
                    w.WriteLine("      <TRNTYPE>{0}", t.GetTransString());
                    w.WriteLine("      <DTPOSTED>{0}", dateStr(t.date));
                    w.WriteLine("      <TRNAMT>{0}", t.value);

                    /* トランザクションの ID は日付と取引番号で生成 */
                    w.WriteLine("      <FITID>{0}", transId(t));
                    w.WriteLine("      <NAME>{0}", quoteString(t.desc));
                    if (t.memo != null)
                    {
                        w.WriteLine("      <MEMO>{0}", quoteString(t.memo));
                    }
                    w.WriteLine("    </STMTTRN>");
                }

                w.WriteLine("  </BANKTRANLIST>");

                /* 残高 */
                w.WriteLine("  <LEDGERBAL>");
                w.WriteLine("    <BALAMT>{0}", last.balance);
                w.WriteLine("    <DTASOF>{0}", dateStr(last.date));
                w.WriteLine("  </LEDGERBAL>");

                w.WriteLine("  </STMTRS>");
                w.WriteLine("</STMTTRNRS>");
            }

            /* OFX 終了 */
            w.WriteLine("</BANKMSGSRSV1>");
            w.WriteLine("</OFX>");

            w.Close();

        }

        public void Execute()
        {
            System.Diagnostics.Process.Start(mOfxFilePath);
        }
    }
}
