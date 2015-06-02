/*
 * FeliCa2Money
 *
 * Copyright (C) 2001-2011 Takuya Murakami
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
   OFX データ生成
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows.Forms;

namespace FeliCa2Money
{
    /// <summary>
    /// OFXデータ生成
    /// </summary>
    public class Ofx
    {
        private XmlDocument mDoc;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Ofx()
        {
            mDoc = new XmlDocument();
        }

        public XmlDocument doc
        {
            get { return mDoc; }
        }

        // OFX要素生成
        public void genOfx(List<Account> accounts)
        {
            Transaction allFirst, allLast;
            getFirstLastDate(accounts, out allFirst, out allLast);
            if (allFirst == null)
            {
                throw new System.InvalidOperationException("No entry");
            }
            
            XmlElement root = mDoc.CreateElement("OFX");
            mDoc.AppendChild(root);

            // Signon MessageSet Response
            signonMessageSetResponse(root, allLast.date);

            // Bank / CreditCard MessageSet Response
            List<Account> banks = new List<Account>();
            List<Account> creditCards = new List<Account>();

            foreach (Account account in accounts)
            {
                if (account.transactions.Count > 0)
                {
                    if (account.isCreditCard)
                    {
                        creditCards.Add(account);
                    }
                    else
                    {
                        banks.Add(account);
                    }
                }
            }

            if (banks.Count > 0)
            {
                bankMessageSetResponse(root, banks);
            }
            if (creditCards.Count > 0)
            {
                creditCardMessageSetResponse(root, creditCards);
            }
        }

        // ------------ private I/F

        // Signon MessageSet Response (SIGNONMSGSRSV1)
        private void signonMessageSetResponse(XmlElement parent, DateTime dtserver)
        {
            XmlElement e = appendElement(parent, "SIGNONMSGSRSV1");

            // Signon (Response)
            XmlElement sonrs = appendElement(e, "SONRS");

            XmlElement status = appendElement(sonrs, "STATUS");
            appendElementWithText(status, "CODE", "0");
            appendElementWithText(status, "SEVERITY", "INFO");
            appendElementWithText(sonrs, "DTSERVER", dateStr(dtserver));
            appendElementWithText(sonrs, "LANGUAGE", "JPN");

            XmlElement fi = appendElement(sonrs, "FI");
            appendElementWithText(fi, "ORG", "FeliCa2Money");
            // FITIDは？
        }

        // Bank MessageSet Response (BANKMSGSRSV1)
        private void bankMessageSetResponse(XmlElement parent, List<Account> accounts)
        {
            // XXXMSGSRSV1 生成
            XmlElement e = appendElement(parent, "BANKMSGSRSV1");
            foreach (Account account in accounts)
            {
                statementTransactionResponse(e, account);
            }
        }

        // Credit Card MessageSet Response (CREDITCARDMSGSRSV1)
        private void creditCardMessageSetResponse(XmlElement parent, List<Account> accounts)
        {
            XmlElement e = appendElement(parent, "CREDITCARDMSGSRSV1");
            foreach (Account account in accounts)
            {
                statementTransactionResponse(e, account);
            }
        }

        // Statement Transaction Response (STMTTRNRS / CCSTMTTRNRS)
        private void statementTransactionResponse(XmlElement parent, Account account)
        {
            XmlElement e;
            if (!account.isCreditCard)
            {
                e = appendElement(parent, "STMTTRNRS");
            }
            else
            {
                e = appendElement(parent, "CCSTMTTRNRS");
            }

            // TRNUID
            appendElementWithText(e, "TRNUID", "0");

            // STATUS
            XmlElement status = appendElement(e, "STATUS");
            appendElementWithText(status, "CODE", "0");
            appendElementWithText(status, "SEVERITY", "INFO");

            // STMTRS / CCSTMTRS
            statementResponse(e, account);
        }

        // Statement Response (STMTRS or CCSTMTRS)
        private void statementResponse(XmlElement parent, Account account)
        {
            Transaction first, last;
            getFirstLastDate(account, out first, out last);

            XmlElement e;
            if (!account.isCreditCard)
            {
                e = appendElement(parent, "STMTRS");
            }
            else
            {
                e = appendElement(parent, "CCSTMTRS");
            }

            // CURDEF
            appendElementWithText(e, "CURDEF", "JPY");

            // BANKACCTFROM / CCACCTFROM
            accountFrom(e, account);

            // BANKTRANLIST
            bankTransactionList(e, account, first, last);

            // LEDGERBAL
            ledgerBal(e, account, last);
        }

        // Account From (BANKACCTFROM or CCACCTFROM)
        private void accountFrom(XmlElement parent, Account account)
        {
            XmlElement e;
            if (!account.isCreditCard)
            {
                e = appendElement(parent, "BANKACCTFROM");
            }
            else
            {
                e = appendElement(parent, "CCACCTFROM");
            }

            if (!account.isCreditCard)
            {
                appendElementWithText(e, "BANKID", account.bankId != null ? account.bankId.ToString() : null); // TBD
                appendElementWithText(e, "BRANCHID", account.branchId);
            }
            appendElementWithText(e, "ACCTID", account.accountId);
            if (!account.isCreditCard)
            {
                appendElementWithText(e, "ACCTTYPE", "SAVINGS");
            }
        }

        // Bank Transaction List
        private void bankTransactionList(XmlElement parent, Account account, Transaction first, Transaction last)
        {
            XmlElement e = appendElement(parent, "BANKTRANLIST");
            
            appendElementWithText(e, "DTSTART", dateStr(first.date));
            appendElementWithText(e, "DTEND", dateStr(last.date));

            foreach (Transaction t in account.transactions)
            {
                statementTransaction(e, t);
            }
        }

        // Statement Transaction
        private void statementTransaction(XmlElement parent, Transaction t)
        {
            // Statement Transaction
            XmlElement e = appendElement(parent, "STMTTRN");

            appendElementWithText(e, "TRNTYPE", t.GetTransString());
            appendElementWithText(e, "DTPOSTED", dateStr(t.date));
            appendElementWithText(e, "TRNAMT", t.value.ToString());

            // トランザクションの ID は日付と取引番号で生成
            appendElementWithText(e, "FITID", t.transId());
            appendElementWithText(e, "NAME", limitString(t.desc, 32));
            if (t.memo != null)
            {
                appendElementWithText(e, "MEMO", t.memo);
            }
        }

        // 残高
        private void ledgerBal(XmlElement parent, Account account, Transaction last)
        {
            XmlElement e = appendElement(parent, "LEDGERBAL");

            int balance;
            if (account.hasBalance)
            {
                balance = account.balance;
            }
            else
            {
                balance = last.balance;
            }

            appendElementWithText(e, "BALAMT", balance.ToString());
            appendElementWithText(e, "DTASOF", dateStr(last.date));
        }

        // 要素追加
        private XmlElement appendElement(XmlElement parent, string elem)
        {
            XmlElement e = mDoc.CreateElement(elem);
            parent.AppendChild(e);
            return e;
        }

        // 要素追加 (テキストノード付き)
        private void appendElementWithText(XmlElement parent, string elem, string text)
        {
            XmlElement e = mDoc.CreateElement(elem);
            e.AppendChild(mDoc.CreateTextNode(text));
            parent.AppendChild(e);
        }

        private string dateStr(DateTime d)
        {
            string s = String.Format("{0}{1:00}{2:00}", d.Year, d.Month, d.Day);
            s += String.Format("{0:00}{1:00}{2:00}", d.Hour, d.Minute, d.Second);
            s += "[+9:JST]";
            return s;
        }

        private string limitString(string s, int maxlen)
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
        private void getFirstLastDate(List<Account> accounts, out Transaction allFirst, out Transaction allLast)
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

        private void getFirstLastDate(Account account, out Transaction first, out Transaction last)
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
    }
}
