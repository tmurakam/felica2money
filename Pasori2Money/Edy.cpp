/*
 * Pasori2Money : Read Pasori data and generate MS Money OFX file.
 *
 * Copyright (c) 2001-2007 Takuya Murakami. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE REGENTS OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * $Id: JNB.cpp 14 2007-03-17 03:29:36Z tmurakam $
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

EdyCard::EdyCard(void)
{
	Ident = "Edy";
	CardName = "Edy";
}

TransactionList * EdyCard::ReadCard(void)
{
	// Edy
        SfcPeep->Execute("-e");

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
// トランザクションリスト
//
Transaction *EdyTransactionList::GenerateTransaction(int nrows, char **rows, int *err)
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

	trans->id = atol(rows[4]);

	AnsiString desc = rows[0];
        desc = desc.SubString(6, desc.Length() - 5);

        if (desc == "支払") {
        	trans->SetTransactionType(desc.c_str(), T_OUTGO);
                trans->value = - atol(rows[2]);
	} else {
		trans->SetTransactionType(desc.c_str(), T_INCOME);
		trans->value = atol(rows[2]);
	}
	trans->desc = utf8(desc.c_str());
	trans->balance = atol(rows[3]);

	return trans;
}
