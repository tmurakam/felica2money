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
#include "Edy.h"

//
// Edy
//

/// コンストラクタ
EdyCard::EdyCard(void)
{
	Ident = "Edy";
	CardName = "Edy";
}

TransactionList * EdyCard::ReadCard(void)
{
	// Edy
        if (SfcPeep->Execute("-e") < 0) return NULL;

        // 一行目を確認
        TStringList *lines = SfcPeep->Lines();
 	if (lines->Count < 1) {
        	// no data
                return NULL;
        }
        AnsiString head = lines->Strings[0];
        lines->Delete(0);

        if (head.SubString(1,4) != "EDY:") {
        	return NULL;
       	}
        CardId = head.SubString(5, head.Length() - 4);

	// transaction list を生成
	TransactionList *list = new EdyTransactionList;

	if (list->ParseLines(lines, true) < 0) {
		delete list;
		return NULL;
	}
	return list;
}

//
//  
// トランザクションリスト
//
Transaction *EdyTransactionList::GenerateTransaction(int nrows, AnsiString *rows, int *err)
{
	Transaction *trans = new Transaction;

       	// 0:処理,1:日付時刻,2:今回取引額,3:チャージ残高, 4:取引連番
        // ET00:ﾁｬｰｼﾞ	2007年03月14日23時08分16秒	24000	49428	59

	AnsiString date = rows[1];
        trans->date.year  = date.SubString(1, 4).ToInt();
  	trans->date.month = date.SubString(7, 2).ToInt();
  	trans->date.date  = date.SubString(11, 2).ToInt();

	trans->date.hour    = date.SubString(15,2).ToInt();
	trans->date.minutes = date.SubString(19,2).ToInt();
	trans->date.seconds = date.SubString(23,2).ToInt();

	trans->id = rows[4].ToInt();

	AnsiString desc = rows[0];
        desc = desc.SubString(6, desc.Length() - 5);

	if (desc == "----") {
		delete trans;
		return NULL;	// empty
	}

        if (desc == "支払") {
        	trans->SetTransactionType(desc.c_str(), T_OUTGO);
                trans->value = - rows[2].ToInt();

		// 適用が "支払" だけだと、Money が過去の履歴から店舗名を
		// 勝手に補完してしまう。これを避けるため、連番を追加しておく。
		desc += " ";
		desc += trans->id;
	} else {
		trans->SetTransactionType(desc.c_str(), T_INCOME);
		trans->value = rows[2].ToInt();
	}
	trans->desc = sjis2utf8(desc);
	trans->balance = rows[3].ToInt();

	return trans;
}
