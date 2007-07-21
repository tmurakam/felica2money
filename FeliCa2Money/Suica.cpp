/*
 * FeliCa2Money
 *
 * Copyright (C) 2001-2007 Takuya Murakami
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

#include <vcl.h>
#pragma hdrstop
#include <stdio.h>
#include "Card.h"
#include "Transaction.h"
#include "SfcPeep.h"
#include "Suica.h"

//
// Suica
//

SuicaCard::SuicaCard(void)
{
	Ident = "Suica";
	CardName = "Suica";
}

TransactionList * SuicaCard::ReadCard(void)
{
	//
	// IDm読み込み
	//
	if (SfcPeep->Execute("-i") < 0) return NULL;

	// 一行目を確認
	TStringList *lines = SfcPeep->Lines();
	if (lines->Count < 1) {
		// no data
                return NULL;
        }
        AnsiString head = lines->Strings[0];
        lines->Delete(0);

        if (head.SubString(1,4) != "IDm:") {
		return NULL;
	}
	CardId = head.SubString(5, head.Length() - 4);

	//
	// 履歴データ読み込み
        //
        if (SfcPeep->Execute("-h") < 0) return NULL;

        // 一行目チェック
	lines = SfcPeep->Lines();
        if (lines->Count < 1) {
		// no data
		return NULL;
	}
	head = lines->Strings[0];
        if (head.SubString(1,5) != "HT00:") {
                return NULL;
        }

	// transaction list を生成
	TransactionList *list = new SuicaTransactionList;
	if (list->ParseLines(lines, true) < 0) {
		delete list;
		return NULL;
	}
	return list;
}

//
// トランザクションリスト
//
Transaction *SuicaTransactionList::GenerateTransaction(int nrows, AnsiString *rows, int *err)
{
	// 0:端末種コード,1:処理,2:日付時刻,
	// 3:入線区コード,4:入駅順コード,5:入会社,6:入駅名,
	// 7:出線区コード,8:出駅順コード,9:出会社,10:出駅名,
	// 11:残高,12:履歴連番

	// 残高
	long balance = rows[11].ToInt();
	long value;

	// 取引額計算
	// Suica の各取引には、残高しか記録されていない (ouch!)
	// なので、前回残高との差分で取引額を計算する
	// よって、最初の１取引は処理不能なので読み飛ばす
	if (prevBalance == UndefBalance) {
		prevBalance = balance;
                *err = 0;
		return NULL;
	} else {
		value = balance - prevBalance;
		prevBalance = balance;
	}

        // 処理
	AnsiString desc = rows[1];
        if (desc == "----") {
                return NULL;	// 空エントリ
        }


        Transaction *trans = new Transaction;

	trans->value = value;
	trans->balance = balance;

	// 日付
	AnsiString date = rows[2];
        trans->date.year  = date.SubString(1, 2).ToInt() + 2000;
  	trans->date.month = date.SubString(5, 2).ToInt();
  	trans->date.date  = date.SubString(9, 2).ToInt();

	trans->date.hour    = 0;
	trans->date.minutes = 0;
	trans->date.seconds = 0;

        // ID
	AnsiString hex = "0x";
        hex += rows[12];
	trans->id = StrToInt(hex.c_str());

	// 説明
	AnsiString memo;
	if (!rows[5].IsEmpty()) {
        	// 運賃の場合、入会社を適用に追加
		desc += " ";
		desc += rows[5];

		// 備考に入出会社/駅名を記載
        	memo = rows[5] + "(" + rows[6] + ")";
		if (!rows[9].IsEmpty()) {
                	memo += " - " + rows[9] + "(" + rows[10] + ")";
                }
	} else {
		// おもに物販の場合、9, 10 に店名が入る
		if (!rows[9].IsEmpty() && rows[9] != "未登録") {
			desc += " ";
			desc += rows[9];
		}
		if (!rows[10].IsEmpty() && rows[10] != "未登録") {
			desc += " ";
			desc += rows[10];
		}

		// 特殊処理
		if (desc == "物販") {
			// 未登録店舗だと適用がすべて"物販"のみになってしまう。
			// すると、Money が勝手に過去の履歴から店舗名を補完してしまい、
			// 都合が悪い。ので、ここでは通し番号を付けておく
			desc += " ";
			desc += rows[12];
		}
	}

        if (value < 0) {
		trans->SetTransactionType(desc.c_str(), T_OUTGO);
	} else {
		trans->SetTransactionType(desc.c_str(), T_INCOME);
	}

	trans->desc = sjis2utf8(desc);
	if (!memo.IsEmpty()) {
		trans->memo = sjis2utf8(memo);
        }

	return trans;
}
