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

namespace FeliCa2Money
{
    class OfxFile2 : OfxFile
    {
        private XmlDocument doc;

        public override void WriteFile(List<Account> accounts)
        {
            XmlDocument d = Generate(accounts);
            d.Save(mOfxFilePath);
        }

        // OFX 2 ドキュメント生成
        private XmlDocument Generate(List<Account> accounts)
        {
            getFirstLastDate(accounts);

            // XML ドキュメント生成
            doc = new XmlDocument();

            XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            doc.AppendChild(decl);

            // OFX 宣言
            XmlProcessingInstruction pi = doc.CreateProcessingInstruction("OFX",
                "OFXHEADER=\"200\" VERSION=\"200\" SECURITY=\"NONE\" OLDFILEUID=\"NONE\" NEWFILEUID=\"NONE\"");
            doc.AppendChild(pi);

            XmlElement root = doc.CreateElement("OFX");
            doc.AppendChild(root);

            // ヘッダ部分
            XmlElement signOnMsgSrsv1 = appendElement(root, "SIGNONMSGSRSV1");
            XmlElement sonrs = appendElement(signOnMsgSrsv1, "SONRS");
            XmlElement status = appendElement(sonrs, "STATUS");
            appendElementWithText(status, "CODE", "0");
            appendElementWithText(status, "SEVERITY", "INFO");
            appendElementWithText(sonrs, "DTSERVER", dateStr(mAllLast.date));
            appendElementWithText(sonrs, "LANGUAGE", "JPN");
            XmlElement fi = appendElement(sonrs, "FI");
            appendElementWithText(fi, "ORG", "FeliCa2Money");
            // FITIDは？

            genCardsInfo(root, accounts, false);
            genCardsInfo(root, accounts, true);

            return doc;
        }

        private void genCardsInfo(XmlElement root, List<Account> accounts, bool isCreditCard)
        {
            /* 口座情報(バンクメッセージレスポンス) */
            XmlElement accountElem;
            if (!isCreditCard)
            {
                accountElem = appendElement(root, "BANKMSGSRSV1");
            }
            else
            {
                accountElem = appendElement(root, "CREDITCARDMSGSRSV1");
            }

            foreach (Account account in accounts)
            {
                if (account.transactions.Count == 0) continue;
                if (account.isCreditCard != isCreditCard) continue;

                Transaction first = account.transactions[0];
                Transaction last = account.transactions[account.transactions.Count - 1];

                /* 預金口座型明細情報作成 */
                XmlElement stmttrnrs;
                if (!isCreditCard)
                {
                    stmttrnrs = appendElement(accountElem, "STMTTRNRS");
                }
                else
                {
                    stmttrnrs = appendElement(accountElem, "CCSTMTTRNRS");
                }
                appendElementWithText(stmttrnrs, "TRNUID", "0");

                XmlElement status = appendElement(stmttrnrs, "STATUS");
                appendElementWithText(status, "CODE", "0");
                appendElementWithText(status, "SEVERITY", "INFO");

                /* STMTRS */
                XmlElement stmtrs;
                if (!isCreditCard)
                {
                    stmtrs = appendElement(stmttrnrs, "STMTRS");
                }
                else
                {
                    stmtrs = appendElement(stmttrnrs, "CCSTMTRS");
                }
                appendElementWithText(stmtrs, "CURDEF", "JPY");

                // 口座番号など
                XmlElement acctfrom;
                if (!isCreditCard)
                {
                    acctfrom = doc.CreateElement("BANKACCTFROM");
                }
                else
                {
                    acctfrom = doc.CreateElement("CCACCTFROM");
                }

                stmtrs.AppendChild(acctfrom);

                appendElementWithText(acctfrom, "BANKID", account.bankId.ToString());
                appendElementWithText(acctfrom, "BRANCHID", account.branchId);
                appendElementWithText(acctfrom, "ACCTID", account.accountId);
                if (!isCreditCard)
                {
                    appendElementWithText(acctfrom, "ACCTTYPE", "SAVINGS");
                }

                /* 明細情報開始(バンクトランザクションリスト) */
                XmlElement banktranlist = appendElement(stmtrs, "BANKTRANLIST");

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
                    appendElementWithText(stmttrn, "NAME", quoteString(t.desc));
                    if (t.memo != null)
                    {
                        appendElementWithText(stmttrn, "MEMO", quoteString(t.memo));
                    }
                }

                /* 残高 */
                XmlElement ledgerbal = appendElement(stmtrs, "LEDGERBAL");

                appendElementWithText(ledgerbal, "BALAMT", last.balance.ToString());
                appendElementWithText(ledgerbal, "DTASOF", dateStr(last.date));
            }
        }

        // 要素追加
        private XmlElement appendElement(XmlElement parent, string elem)
        {
            XmlElement e = doc.CreateElement(elem);
            parent.AppendChild(e);
            return e;
        }

        // 要素追加 (テキストノード付き)
        private void appendElementWithText(XmlElement parent, string elem, string text)
        {
            XmlElement e = doc.CreateElement(elem);
            e.AppendChild(doc.CreateTextNode(text));
            parent.AppendChild(e);
        }
    }
}
