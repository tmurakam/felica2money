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
using System.Windows.Forms;

namespace FeliCa2Money
{
    class Suica : Card
    {
        private int prevBalance = UndefBalance;
        private const int UndefBalance = -9999999;

        public Suica()
        {
            ident = "Suica";
            cardName = "Suica";
        }

        public override List<Transaction> ReadCard()
        {
            SfcPeep s = new SfcPeep();

            // IDm 読み込み
            List<string> lines = s.Execute("-i");
            if (!lines[0].StartsWith("IDm:"))
	        {
                return null;
            }

            CardId = lines[0].Substring(4);

            // 履歴データ読み込み
            lines = s.Execute("-h");
            if (lines.Count < 1 || !lines[0].StartsWith("HT00:"))
            {
                return null;
            }

            // 順序反転
            lines.Reverse();

            // Parse lines
            List<Transaction> transactions = new List<Transaction>();
            foreach (string line in lines)
            {
                Transaction t = new Transaction();

                string[] items = ParseLine(line);
                if (SetTransaction(t, items)) {
                    transactions.Add(t);
                }
            }
            return transactions;
        }

        private bool SetTransaction(Transaction t, string[] items)
        {
            // 0:端末種コード,1:処理,2:日付時刻,
            // 3:入線区コード,4:入駅順コード,5:入会社,6:入駅名,
            // 7:出線区コード,8:出駅順コード,9:出会社,10:出駅名,
            // 11:残高,12:履歴連番

            // 処理
	    t.desc = items[1];
            if (t.desc == "----") {
                return false;	// 空エントリ
            }

            // 残高
	    t.balance = int.Parse(items[11]);

	    // 取引額計算
	    // Suica の各取引には、残高しか記録されていない (ouch!)
	    // なので、前回残高との差分で取引額を計算する
	    // よって、最初の１取引は処理不能なので読み飛ばす
	    if (prevBalance == UndefBalance)
	    {
		prevBalance = t.balance;
		return false;
	    }
	    else
	    {
		t.value = t.balance - prevBalance;
		prevBalance = t.balance;
	    }

	    // 日付
            string d = items[2];
            int yy = int.Parse(d.Substring(0, 2)) + 2000;
            int mm = int.Parse(d.Substring(3, 2));
            int dd = int.Parse(d.Substring(6, 2));

            t.date = new DateTime(yy, mm, dd, 0, 0, 0);

            // ID
	    t.id = Convert.ToInt32(items[12], 16);

            // 説明/メモ
	    if (items[5] != "")
	    {
		// 運賃の場合、入会社を適用に表示
		appendDesc(t, items[5]);

		// 備考に入出会社/駅名を記載
		t.memo = items[5] + "(" + items[6] + ")";
		if (items[9] != "")
		{
		    t.memo += " - " + items[9] + "(" + items[10] + ")";
                }
	    }
	    else
	    {
		// おもに物販の場合、9, 10 に店名が入る
		appendDesc(t, items[9]);
                appendDesc(t, items[10]);

		// 特殊処理
		if (t.desc == "物販")
		{
		    // 未登録店舗だと適用がすべて「物販」になってしまう。
		    // すると Money が勝手に過去の履歴から店舗名を補完してしまい
		    // 都合がわるい。ここでは通し番号を振っておく。
		    t.desc += " " + items[12];
		}
            }

            // トランザクションタイプ
            if (t.value < 0) {
		t.GuessTransType(false);
	    }
	    else
	    {
                t.GuessTransType(true);
	    }

            return true;
        }

        private void appendDesc(Transaction t, string d)
        {
            if (d == "" || d == "未登録")
            {
                return;
            }

            if (t.desc == "支払")
            {
                t.desc = d;       // "支払"は削除して上書き
            }
            else
            {
                t.desc += " " + d;
            }
        }
    }
}
