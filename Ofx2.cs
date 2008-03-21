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

// OFX 2.0

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace FeliCa2Money
{
    class Ofx2
    {
        private XmlDocument doc;

        // OFX 2 ドキュメント生成
        public XmlDocument Generate(Card card,  List<Transaction> transactions)
        {
            Transaction first = transactions[0];
            Transaction last = transactions[transactions.Count - 1];

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
            appendElementWithText(sonrs, "DTSERVER", dateStr(last.date));
            appendElementWithText(sonrs, "LANGUAGE", "JPN");
            XmlElement fi = appendElement(sonrs, "FI");
            appendElementWithText(fi, "ORG", card.Ident);
            // FITIDは？

            /* 口座情報(バンクメッセージレスポンス) */
            XmlElement bankMsgSrsv1 = appendElement(root, "BANKMSGSRSV1");

            /* 預金口座型明細情報作成 */
            XmlElement stmttrnrs = appendElement(bankMsgSrsv1, "STMTTRNRS");
            appendElementWithText(stmttrnrs, "TRNUID", "0");

            status = appendElement(stmttrnrs, "STATUS");
            appendElementWithText(status, "CODE", "0");
            appendElementWithText(status, "SEVERITY", "INFO");

            /* STMTRS */
            XmlElement stmtrs = appendElement(stmttrnrs, "STMTRS");
            appendElementWithText(stmtrs, "CURDEF", "JPY");

            // 口座番号など
            XmlElement bankacctfrom = doc.CreateElement("BANKACCTFROM");
            stmtrs.AppendChild(bankacctfrom);

            appendElementWithText(bankacctfrom, "BANKID", card.BankId.ToString());            
            appendElementWithText(bankacctfrom, "BRANCHID", card.BranchId);
            appendElementWithText(bankacctfrom, "ACCTID", card.AccountId);
            appendElementWithText(bankacctfrom, "ACCTTYPE", "SAVINGS");

            /* 明細情報開始(バンクトランザクションリスト) */
            XmlElement banktranlist = appendElement(stmtrs, "BANKTRANLIST");

            appendElementWithText(banktranlist, "DTSTART", dateStr(first.date));
            appendElementWithText(banktranlist, "DTEND", dateStr(last.date));            

            /* トランザクション */
            foreach (Transaction t in transactions)
            {
                XmlElement stmttrn = appendElement(banktranlist, "STMTTRN");

                appendElementWithText(stmttrn, "TRNTYPE", t.GetTransString());
                appendElementWithText(stmttrn, "DTPOSTED", dateStr(t.date));
                appendElementWithText(stmttrn, "TRNAMT", t.value.ToString());
                
                // トランザクションの ID は日付と取引番号で生成
                appendElementWithText(stmttrn, "FITID", transId(t));
                appendElementWithText(stmttrn, "NAME", t.desc);
                if (t.memo != null)
                {
                    appendElementWithText(stmttrn, "MEMO", t.memo);
                }
            }

            /* 残高 */
            XmlElement ledgerbal = appendElement(stmtrs, "LEDGERBAL");

            appendElementWithText(ledgerbal, "BALAMT", last.balance.ToString());
            appendElementWithText(ledgerbal, "DTASOF", dateStr(last.date));

            return doc;
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

        // 日付文字列処理
        private string dateStr(DateTime d)
        {
            string s = String.Format("{0}{1:00}{2:00}", d.Year, d.Month, d.Day);
            s += String.Format("{0:00}{1:00}{2:00}", d.Hour, d.Minute, d.Second);
            s += "[+9:JST]";
            return s;
        }

        // トランザクション ID 生成
        private string transId(Transaction t)
        {
            // 日付と取引番号で生成
            string longId = String.Format("{0:0000}{1:00}{2:00}", t.date.Year, t.date.Month, t.date.Day);
            longId += String.Format("{0:0000000}", t.id);
            return longId;
        }
    }
}
