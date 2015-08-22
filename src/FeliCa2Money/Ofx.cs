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
        private readonly XmlDocument _doc;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Ofx()
        {
            _doc = new XmlDocument();
        }

        public XmlDocument Doc
        {
            get { return _doc; }
        }

        // OFX要素生成
        public void GenOfx(List<Account> accounts)
        {
            Transaction allFirst, allLast;
            GetFirstLastDate(accounts, out allFirst, out allLast);
            if (allFirst == null)
            {
                throw new System.InvalidOperationException("No entry");
            }
            
            var root = _doc.CreateElement("OFX");
            _doc.AppendChild(root);

            // Signon MessageSet Response
            SignonMessageSetResponse(root, allLast.Date);

            // Bank / CreditCard MessageSet Response
            var banks = new List<Account>();
            var creditCards = new List<Account>();

            foreach (var account in accounts)
            {
                if (account.Transactions.Count > 0)
                {
                    if (account.IsCreditCard)
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
                BankMessageSetResponse(root, banks);
            }
            if (creditCards.Count > 0)
            {
                CreditCardMessageSetResponse(root, creditCards);
            }
        }

        // ------------ private I/F

        // Signon MessageSet Response (SIGNONMSGSRSV1)
        private void SignonMessageSetResponse(XmlElement parent, DateTime dtserver)
        {
            var e = AppendElement(parent, "SIGNONMSGSRSV1");

            // Signon (Response)
            var sonrs = AppendElement(e, "SONRS");

            var status = AppendElement(sonrs, "STATUS");
            AppendElementWithText(status, "CODE", "0");
            AppendElementWithText(status, "SEVERITY", "INFO");
            AppendElementWithText(sonrs, "DTSERVER", dateStr(dtserver));
            AppendElementWithText(sonrs, "LANGUAGE", "JPN");

            var fi = AppendElement(sonrs, "FI");
            AppendElementWithText(fi, "ORG", "FeliCa2Money");
            // FITIDは？
        }

        // Bank MessageSet Response (BANKMSGSRSV1)
        private void BankMessageSetResponse(XmlElement parent, List<Account> accounts)
        {
            // XXXMSGSRSV1 生成
            var e = AppendElement(parent, "BANKMSGSRSV1");
            foreach (var account in accounts)
            {
                StatementTransactionResponse(e, account);
            }
        }

        // Credit Card MessageSet Response (CREDITCARDMSGSRSV1)
        private void CreditCardMessageSetResponse(XmlElement parent, List<Account> accounts)
        {
            var e = AppendElement(parent, "CREDITCARDMSGSRSV1");
            foreach (var account in accounts)
            {
                StatementTransactionResponse(e, account);
            }
        }

        // Statement Transaction Response (STMTTRNRS / CCSTMTTRNRS)
        private void StatementTransactionResponse(XmlElement parent, Account account)
        {
            XmlElement e;
            if (!account.IsCreditCard)
            {
                e = AppendElement(parent, "STMTTRNRS");
            }
            else
            {
                e = AppendElement(parent, "CCSTMTTRNRS");
            }

            // TRNUID
            AppendElementWithText(e, "TRNUID", "0");

            // STATUS
            var status = AppendElement(e, "STATUS");
            AppendElementWithText(status, "CODE", "0");
            AppendElementWithText(status, "SEVERITY", "INFO");

            // STMTRS / CCSTMTRS
            StatementResponse(e, account);
        }

        // Statement Response (STMTRS or CCSTMTRS)
        private void StatementResponse(XmlElement parent, Account account)
        {
            Transaction first, last;
            GetFirstLastDate(account, out first, out last);

            XmlElement e;
            if (!account.IsCreditCard)
            {
                e = AppendElement(parent, "STMTRS");
            }
            else
            {
                e = AppendElement(parent, "CCSTMTRS");
            }

            // CURDEF
            AppendElementWithText(e, "CURDEF", "JPY");

            // BANKACCTFROM / CCACCTFROM
            AccountFrom(e, account);

            // BANKTRANLIST
            BankTransactionList(e, account, first, last);

            // LEDGERBAL
            LedgerBal(e, account, last);
        }

        // Account From (BANKACCTFROM or CCACCTFROM)
        private void AccountFrom(XmlElement parent, Account account)
        {
            XmlElement e;
            if (!account.IsCreditCard)
            {
                e = AppendElement(parent, "BANKACCTFROM");
            }
            else
            {
                e = AppendElement(parent, "CCACCTFROM");
            }

            if (!account.IsCreditCard)
            {
                AppendElementWithText(e, "BANKID", account.BankId != null ? account.BankId.ToString() : null); // TBD
                AppendElementWithText(e, "BRANCHID", account.BranchId);
            }
            AppendElementWithText(e, "ACCTID", account.AccountId);
            if (!account.IsCreditCard)
            {
                AppendElementWithText(e, "ACCTTYPE", "SAVINGS");
            }
        }

        // Bank Transaction List
        private void BankTransactionList(XmlElement parent, Account account, Transaction first, Transaction last)
        {
            var e = AppendElement(parent, "BANKTRANLIST");
            
            AppendElementWithText(e, "DTSTART", dateStr(first.Date));
            AppendElementWithText(e, "DTEND", dateStr(last.Date));

            foreach (Transaction t in account.Transactions)
            {
                StatementTransaction(e, t);
            }
        }

        // Statement Transaction
        private void StatementTransaction(XmlElement parent, Transaction t)
        {
            // Statement Transaction
            var e = AppendElement(parent, "STMTTRN");

            AppendElementWithText(e, "TRNTYPE", t.GetTransString());
            AppendElementWithText(e, "DTPOSTED", dateStr(t.Date));
            AppendElementWithText(e, "TRNAMT", t.Value.ToString());

            // トランザクションの ID は日付と取引番号で生成
            AppendElementWithText(e, "FITID", t.TransId());
            AppendElementWithText(e, "NAME", LimitString(t.Desc, 32));
            if (t.Memo != null)
            {
                AppendElementWithText(e, "MEMO", t.Memo);
            }
        }

        // 残高
        private void LedgerBal(XmlElement parent, Account account, Transaction last)
        {
            var e = AppendElement(parent, "LEDGERBAL");

            int balance;
            if (account.HasBalance)
            {
                balance = account.Balance;
            }
            else
            {
                balance = last.Balance;
            }

            AppendElementWithText(e, "BALAMT", balance.ToString());
            AppendElementWithText(e, "DTASOF", dateStr(last.Date));
        }

        // 要素追加
        private XmlElement AppendElement(XmlElement parent, string elem)
        {
            var e = _doc.CreateElement(elem);
            parent.AppendChild(e);
            return e;
        }

        // 要素追加 (テキストノード付き)
        private void AppendElementWithText(XmlElement parent, string elem, string text)
        {
            var e = _doc.CreateElement(elem);
            e.AppendChild(_doc.CreateTextNode(text));
            parent.AppendChild(e);
        }

        private string dateStr(DateTime d)
        {
            var s = string.Format("{0}{1:00}{2:00}", d.Year, d.Month, d.Day);
            s += string.Format("{0:00}{1:00}{2:00}", d.Hour, d.Minute, d.Second);
            s += "[+9:JST]";
            return s;
        }

        private string LimitString(string s, int maxlen)
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
        private void GetFirstLastDate(List<Account> accounts, out Transaction allFirst, out Transaction allLast)
        {
            allFirst = null;
            allLast = null;
            foreach (var account in accounts) {
                foreach (var t in account.Transactions)
                {
                    // 先頭エントリ: 同じ日付の場合は、前のエントリを優先
                    if (allFirst == null || t.Date < allFirst.Date)
                    {
                        allFirst = t;
                    }
                    // 最終エントリ: 同じ日付の場合は、後のエントリを優先
                    if (allLast == null || t.Date >= allLast.Date)
                    {
                        allLast = t;
                    }
                }
            }
        }

        private void GetFirstLastDate(Account account, out Transaction first, out Transaction last)
        {
            first = null;
            last = null;
            foreach (var t in account.Transactions)
            {
                // 先頭エントリ: 同じ日付の場合は、前のエントリを優先
                if (first == null || t.Date < first.Date)
                {
                    first = t;
                }
                // 最終エントリ: 同じ日付の場合は、後のエントリを優先
                if (last == null || t.Date >= last.Date)
                {
                    last = t;
                }
            }
        }
    }
}
