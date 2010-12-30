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
    class OfxFile2 : OfxFile
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
            Transaction allFirst, allLast;
            getFirstLastDate(accounts, out allFirst, out allLast);
            if (allFirst == null)
            {
                throw new System.InvalidOperationException("No entry");
            }
            
            // XML ドキュメント生成
            mDoc = new XmlDocument();

            XmlDeclaration decl = mDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            mDoc.AppendChild(decl);

            // OFX 宣言
            XmlProcessingInstruction pi = mDoc.CreateProcessingInstruction("OFX",
                "OFXHEADER=\"200\" VERSION=\"200\" SECURITY=\"NONE\" OLDFILEUID=\"NONE\" NEWFILEUID=\"NONE\"");
            mDoc.AppendChild(pi);

            XmlElement root = mDoc.CreateElement("OFX");
            mDoc.AppendChild(root);

            // ヘッダ部分
            XmlElement signOnMsgSrsv1 = appendElement(root, "SIGNONMSGSRSV1");
            XmlElement sonrs = appendElement(signOnMsgSrsv1, "SONRS");
            XmlElement status = appendElement(sonrs, "STATUS");
            appendElementWithText(status, "CODE", "0");
            appendElementWithText(status, "SEVERITY", "INFO");
            appendElementWithText(sonrs, "DTSERVER", dateStr(allLast.date));
            appendElementWithText(sonrs, "LANGUAGE", "JPN");
            XmlElement fi = appendElement(sonrs, "FI");
            appendElementWithText(fi, "ORG", "FeliCa2Money");
            // FITIDは？

            genCardsInfo(root, accounts, false);
            genCardsInfo(root, accounts, true);

            return mDoc;
        }

        /// <summary>
        /// 各アカウントのトランザクションを生成する。具体的には BANKMSGSRSV1または CREDITCARDMSGSRSV1 を生成。
        /// </summary>
        /// <param name="root">root要素</param>
        /// <param name="accounts">アカウントリスト</param>
        /// <param name="isCreditCard">クレジットカードの場合は true、銀行の場合は false にする</param>
        private void genCardsInfo(XmlElement root, List<Account> accounts, bool isCreditCard)
        {
            // 該当するトランザクション数を数える
            int count = 0;
            foreach (Account account in accounts)
            {
                if (account.isCreditCard == isCreditCard)
                {
                    count += account.transactions.Count;
                }
            }
            if (count == 0) return; // 該当口座なし
            
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
                if (account.isCreditCard != isCreditCard) continue;
                if (account.transactions.Count == 0) continue;

                Transaction first, last;
                getFirstLastDate(account, out first, out last);

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
                    acctfrom = mDoc.CreateElement("BANKACCTFROM");
                }
                else
                {
                    acctfrom = mDoc.CreateElement("CCACCTFROM");
                }

                stmtrs.AppendChild(acctfrom);

                if (!isCreditCard)
                {
                    appendElementWithText(acctfrom, "BANKID", account.bankId.ToString());
                    appendElementWithText(acctfrom, "BRANCHID", account.branchId);
                }
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
                    appendElementWithText(stmttrn, "NAME", quoteString(limitString(t.desc, 32)));
                    if (t.memo != null)
                    {
                        appendElementWithText(stmttrn, "MEMO", quoteString(t.memo));
                    }
                }

                /* 残高 */
                XmlElement ledgerbal = appendElement(stmtrs, "LEDGERBAL");

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
