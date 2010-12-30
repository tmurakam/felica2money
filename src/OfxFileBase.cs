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
    /// OFXファイル生成: 基底クラス
    /// </summary>
    abstract class OfxFile
    {
        protected string mOfxFilePath;

        /// <summary>
        /// OFXファイルインスタンス生成
        /// </summary>
        /// <param name="version">バージョン</param>
        /// <returns>OfxFile</returns>
        public static OfxFile newOfxFile(int version)
        {
            switch (version)
            {
                case 1:
                    return new OfxFileV1();
                case 2:
                    return new OfxFileV2();
            }
            return null;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public OfxFile()
        {
        }

        /// <summary>
        /// OFXファイルパスを設定する
        /// </summary>
        /// <param name="path">OFXファイルパス</param>
        public void SetOfxFilePath(String path)
        {
            mOfxFilePath = path;
        }

        /// <summary>
        /// OFXファイル書き出し
        /// </summary>
        /// <param name="account">アカウント</param>
        public void WriteFile(Account account)
        {
            List<Account> accounts = new List<Account>();
            accounts.Add(account);
            WriteFile(accounts);
        }

        /// <summary>
        /// OFXファイル書き出し
        /// </summary>
        /// <param name="accounts">アカウントリスト</param>
        public abstract void WriteFile(List<Account> accounts);

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

        /// <summary>
        /// OFX ファイルをアプリケーションで開く
        /// </summary>
        public void Execute()
        {
            System.Diagnostics.Process.Start(mOfxFilePath);
        }
    }
}
