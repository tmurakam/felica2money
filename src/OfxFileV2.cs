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

// OFX ver 2.0 (XML)

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows.Forms;

namespace FeliCa2Money
{
    /// <summary>
    /// OFXファイルバージョン2
    /// </summary>
    class OfxFileV2 : OfxFile
    {
        private XmlDocument mDoc;

        /// <summary>
        /// OFX V2 ファイルを生成する
        /// </summary>
        /// <param name="accounts">アカウント</param>
        public override void WriteFile(List<Account> accounts)
        {
            XmlDocument d = Generate(accounts);
            d.Save(mOfxFilePath);
        }

        // OFX 2 ドキュメント生成
        private XmlDocument Generate(List<Account> accounts)
        {
            // XML ドキュメント生成
            mDoc = new XmlDocument();

            XmlDeclaration decl = mDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            mDoc.AppendChild(decl);

            // OFX 宣言
            XmlProcessingInstruction pi = mDoc.CreateProcessingInstruction("OFX",
                "OFXHEADER=\"200\" VERSION=\"200\" SECURITY=\"NONE\" OLDFILEUID=\"NONE\" NEWFILEUID=\"NONE\"");
            mDoc.AppendChild(pi);

            ofx(mDoc, accounts);

            return mDoc;
        }

        // OFX要素生成
        private void ofx(XmlDocument doc, List<Account> accounts)
        {
            Transaction allFirst, allLast;
            getFirstLastDate(accounts, out allFirst, out allLast);
            if (allFirst == null)
            {
                throw new System.InvalidOperationException("No entry");
            }
            
            XmlElement root = mDoc.CreateElement("OFX");
            mDoc.AppendChild(root);

            // SIGNONMSGSRSV1
            signonMsgSrsv1(root, allLast.date);

            // BANKMSGSRSV1 / CREDITCARDMSGSRSV1
            for (int i = 0; i < 2; i++)
            {
                bool isCreditCard = (i == 0) ? false : true;

                // BANK or CREDITCARD アカウントのみを取り出す
                List<Account> subaccts = new List<Account>();
                foreach (Account account in accounts)
                {
                    if (account.isCreditCard == isCreditCard && account.transactions.Count > 0)
                    {
                        subaccts.Add(account);
                    }
                }

                if (!isCreditCard)
                {
                    bankMsgSrsv1(root, subaccts);
                }
                else
                {
                    creditCardMsgSrsv1(root, subaccts);
                }
            }
        }

        // SIGNONMSGSRSV1
        private void signonMsgSrsv1(XmlElement parent, DateTime dtserver)
        {
            XmlElement signOnMsgSrsv1 = appendElement(parent, "SIGNONMSGSRSV1");
            XmlElement sonrs = appendElement(signOnMsgSrsv1, "SONRS");
            XmlElement status = appendElement(sonrs, "STATUS");
            appendElementWithText(status, "CODE", "0");
            appendElementWithText(status, "SEVERITY", "INFO");
            appendElementWithText(sonrs, "DTSERVER", dateStr(dtserver));
            appendElementWithText(sonrs, "LANGUAGE", "JPN");
            XmlElement fi = appendElement(sonrs, "FI");
            appendElementWithText(fi, "ORG", "FeliCa2Money");
            // FITIDは？
        }

        // BANKMSGSRSV1 要素生成
        private void bankMsgSrsv1(XmlElement parent, List<Account> accounts)
        {
            // XXXMSGSRSV1 生成
            XmlElement msgsrsv1 = appendElement(parent, "BANKMSGSRSV1");
            foreach (Account account in accounts)
            {
                stmttrnrs(msgsrsv1, account);
            }
        }

        // CREDITCARDMSGSRSV1 要素生成
        private void creditCardMsgSrsv1(XmlElement parent, List<Account> accounts)
        {
            XmlElement msgsrsv1 = appendElement(parent, "CREDITCARDMSGSRSV1");
            foreach (Account account in accounts)
            {
                stmttrnrs(msgsrsv1, account);
            }
        }

        // STMTTRNRS または CCSTMTTRNRS
        private void stmttrnrs(XmlElement parent, Account account)
        {
            /* 預金口座型明細情報作成 */
            XmlElement stmttrnrs;
            if (!account.isCreditCard)
            {
                stmttrnrs = appendElement(parent, "STMTTRNRS");
            }
            else
            {
                stmttrnrs = appendElement(parent, "CCSTMTTRNRS");
            }

            // TRNUID
            appendElementWithText(stmttrnrs, "TRNUID", "0");

            // STATUS
            XmlElement status = appendElement(stmttrnrs, "STATUS");
            appendElementWithText(status, "CODE", "0");
            appendElementWithText(status, "SEVERITY", "INFO");

            // STMTRS / CCSTMTRS
            stmtrs(stmttrnrs, account);
        }

        // STMTRS または CCSTMTRS
        private void stmtrs(XmlElement parent, Account account)
        {
            Transaction first, last;
            getFirstLastDate(account, out first, out last);

            /* STMTRS */
            XmlElement stmtrs;
            if (!account.isCreditCard)
            {
                stmtrs = appendElement(parent, "STMTRS");
            }
            else
            {
                stmtrs = appendElement(parent, "CCSTMTRS");
            }

            // CURDEF
            appendElementWithText(stmtrs, "CURDEF", "JPY");

            // BANKACCTFROM / CCACCTFROM
            acctFrom(stmtrs, account);

            // BANKTRANLIST
            bankTranList(stmtrs, account, first, last);

            // LEDGERBAL
            ledgerBal(stmtrs, account, last);
        }

        // BANKACCTFROM または CCACCTFROM
        private void acctFrom(XmlElement parent, Account account)
        {
            XmlElement acctfrom;
            if (!account.isCreditCard)
            {
                acctfrom = mDoc.CreateElement("BANKACCTFROM");
            }
            else
            {
                acctfrom = mDoc.CreateElement("CCACCTFROM");
            }
            parent.AppendChild(acctfrom);

            if (!account.isCreditCard)
            {
                appendElementWithText(acctfrom, "BANKID", account.bankId.ToString());
                appendElementWithText(acctfrom, "BRANCHID", account.branchId);
            }
            appendElementWithText(acctfrom, "ACCTID", account.accountId);
            if (!account.isCreditCard)
            {
                appendElementWithText(acctfrom, "ACCTTYPE", "SAVINGS");
            }
        }

        // 明細情報開始(バンクトランザクションリスト)
        private void bankTranList(XmlElement parent, Account account, Transaction first, Transaction last)
        {
            XmlElement banktranlist = appendElement(parent, "BANKTRANLIST");
            
            appendElementWithText(banktranlist, "DTSTART", dateStr(first.date));
            appendElementWithText(banktranlist, "DTEND", dateStr(last.date));

            /* トランザクション */
            foreach (Transaction t in account.transactions)
            {
                XmlElement stmttrn = appendElement(banktranlist, "STMTTRN");

                appendElementWithText(stmttrn, "TRNTYPE", t.GetTransString());
                appendElementWithText(stmttrn, "DTPOSTED", dateStr(t.date));
                appendElementWithText(stmttrn, "TRNAMT", t.value.ToString());

                // トランザクションの ID は日付と取引番号で生成
                appendElementWithText(stmttrn, "FITID", transId(t));
                appendElementWithText(stmttrn, "NAME", quoteString(limitString(t.desc, 32)));
                if (t.memo != null)
                {
                    appendElementWithText(stmttrn, "MEMO", quoteString(t.memo));
                }
            }
        }

        // 残高
        private void ledgerBal(XmlElement parent, Account account, Transaction last)
        {
            XmlElement ledgerbal = appendElement(parent, "LEDGERBAL");

            int balance;
            if (account.hasBalance)
            {
                balance = account.balance;
            }
            else
            {
                balance = last.balance;
            }

            appendElementWithText(ledgerbal, "BALAMT", balance.ToString());
            appendElementWithText(ledgerbal, "DTASOF", dateStr(last.date));
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
    }
}
