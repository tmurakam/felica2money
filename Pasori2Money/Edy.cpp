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

	if (list->ParseLines(lines) < 0) {
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
#if 0
	Transaction *trans = new Transaction;

	/* "操作日(年)","操作日(月)","操作日(日)","取引順番号",
	   "摘要",  "お支払金額","お預り金額","残高" */
	trans->date.year  = atoi(rows[0]);
	trans->date.month = atoi(rows[1]);
	trans->date.date  = atoi(rows[2]);

	trans->date.hour = 0;
	trans->date.minutes = 0;
	trans->date.seconds = 0;

	trans->id = atol(rows[3]);
	if (strcmp(rows[5], "") != 0) {
		trans->SetTransactionType(rows[4], T_OUTGO);
		trans->value = - atol(rows[5]);
	} else {
		trans->SetTransactionType(rows[4], T_INCOME);
		trans->value = atol(rows[6]);
	}
	trans->desc = utf8(rows[4]);
	trans->balance = atol(rows[7]);

	return trans;
#endif
}
