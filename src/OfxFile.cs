/*
 * FeliCa2Money
 *
 * Copyright (C) 2001-2010 Takuya Murakami
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
    /// <summary>
    /// OFXファイルバージョン1
    /// </summary>
    class OfxFile
    {
        protected string mOfxFilePath;

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

        protected string limitString(string s, int maxlen)
        {
            if (s.Length <= maxlen)
            {
                return s;
            }
            else
            {
                return s.Substring(0, maxlen);
            }
        }

        // 最初のトランザクションと最後のトランザクションを取り出しておく
        // (日付範囲取得のため)
        protected void getFirstLastDate(List<Account> accounts, out Transaction allFirst, out Transaction allLast)
        {
            allFirst = null;
            allLast = null;
            foreach (Account account in accounts) {
                foreach (Transaction t in account.transactions)
                {
                    // 先頭エントリ: 同じ日付の場合は、前のエントリを優先
                    if (allFirst == null || t.date < allFirst.date)
                    {
                        allFirst = t;
                    }
                    // 最終エントリ: 同じ日付の場合は、後のエントリを優先
                    if (allLast == null || t.date >= allLast.date)
                    {
                        allLast = t;
                    }
                }
            }
        }

        protected void getFirstLastDate(Account account, out Transaction first, out Transaction last)
        {
            first = null;
            last = null;
            foreach (Transaction t in account.transactions)
            {
                // 先頭エントリ: 同じ日付の場合は、前のエントリを優先
                if (first == null || t.date < first.date)
                {
                    first = t;
                }
                // 最終エントリ: 同じ日付の場合は、後のエントリを優先
                if (last == null || t.date >= last.date)
                {
                    last = t;
                }
            }
        }

        public void WriteFile(Account account)
        {
            List<Account> cards = new List<Account>();
            cards.Add(account);
            WriteFile(cards);
        }

        public virtual void WriteFile(List<Account> accounts)
        {
            Transaction allFirst;
            Transaction allLast;
            getFirstLastDate(accounts, out allFirst, out allLast);

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
            w.WriteLine("  <DTSERVER>{0}", dateStr(allLast.date));

            w.WriteLine("  <LANGUAGE>JPN");
            w.WriteLine("  <FI>");
            w.WriteLine("    <ORG>FeliCa2Money");
            w.WriteLine("  </FI>");
            w.WriteLine("</SONRS>");
            w.WriteLine("</SIGNONMSGSRSV1>");

            genCardsInfo(w, accounts, false);
            genCardsInfo(w, accounts, true);

            /* OFX 終了 */
            w.WriteLine("</OFX>");
            w.Close();
        }

        private void genCardsInfo(StreamWriter w, List<Account> accounts, bool isCreditCard)
        {
            /* 口座情報(バンクメッセージレスポンス) */
            if (!isCreditCard)
            {
                w.WriteLine("<BANKMSGSRSV1>");
            }
            else
            {
                w.WriteLine("<CREDITCARDMSGSRSV1>");
            }

            foreach (Account account in accounts)
            {
                if (account.isCreditCard != isCreditCard) continue;
                if (account.transactions.Count == 0) continue; // no transactions

                Transaction first, last;
                getFirstLastDate(account, out first, out last);

                /* 預金口座型明細情報作成 */
                if (!isCreditCard)
                {
                    w.WriteLine("<STMTTRNRS>");
                }
                else
                {
                    w.WriteLine("<CCSTMTTRNRS>");
                }
                w.WriteLine("<TRNUID>0");
                w.WriteLine("<STATUS>");
                w.WriteLine("  <CODE>0");
                w.WriteLine("  <SEVERITY>INFO");
                w.WriteLine("</STATUS>");

                if (!isCreditCard)
                {
                    w.WriteLine("<STMTRS>");
                }
                else
                {
                    w.WriteLine("<CCSTMTRS>");
                }
                w.WriteLine("  <CURDEF>JPY");

                if (!isCreditCard)
                {
                    w.WriteLine("  <BANKACCTFROM>");
                    w.WriteLine("    <BANKID>{0}", account.bankId);
                    w.WriteLine("    <BRANCHID>{0}", account.branchId);
                }
                else
                {
                    w.WriteLine("  <CCACCTFROM>");
                }
                w.WriteLine("    <ACCTID>{0}", account.accountId);
                if (!isCreditCard)
                {
                    w.WriteLine("    <ACCTTYPE>SAVINGS");
                    w.WriteLine("  </BANKACCTFROM>");
                }
                else
                {
                    w.WriteLine("  </CCACCTFROM>");
                }

                /* 明細情報開始(バンクトランザクションリスト) */
                w.WriteLine("  <BANKTRANLIST>");
                w.WriteLine("    <DTSTART>{0}", dateStr(first.date));
                w.WriteLine("    <DTEND>{0}", dateStr(last.date));

                /* トランザクション */
                foreach (Transaction t in account.transactions)
                {
                    w.WriteLine("    <STMTTRN>");
                    w.WriteLine("      <TRNTYPE>{0}", t.GetTransString());
                    w.WriteLine("      <DTPOSTED>{0}", dateStr(t.date));
                    w.WriteLine("      <TRNAMT>{0}", t.value);

                    /* トランザクションの ID は日付と取引番号で生成 */
                    w.WriteLine("      <FITID>{0}", transId(t));
                    w.WriteLine("      <NAME>{0}", quoteString(limitString(t.desc, 32)));
                    if (t.memo != null)
                    {
                        w.WriteLine("      <MEMO>{0}", quoteString(t.memo));
                    }
                    w.WriteLine("    </STMTTRN>");
                }

                w.WriteLine("  </BANKTRANLIST>");

                /* 残高 */
                int balance;
                if (account.hasBalance)
                {
                    balance = account.balance;
                }
                else
                {
                    balance = last.balance;
                }
                w.WriteLine("  <LEDGERBAL>");
                w.WriteLine("    <BALAMT>{0}", balance);
                w.WriteLine("    <DTASOF>{0}", dateStr(last.date));
                w.WriteLine("  </LEDGERBAL>");

                if (!isCreditCard)
                {
                    w.WriteLine("  </STMTRS>");
                    w.WriteLine("</STMTTRNRS>");
                }
                else
                {
                    w.WriteLine("  </CCSTMTRS>");
                    w.WriteLine("</CCSTMTTRNRS>");
                }
            }
            if (!isCreditCard)
            {
                w.WriteLine("</BANKMSGSRSV1>");
            }
            else
            {
                w.WriteLine("</CREDITCARDMSGSRSV1>");
            }
        }

        public void Execute()
        {
            System.Diagnostics.Process.Start(mOfxFilePath);
        }
    }
}
