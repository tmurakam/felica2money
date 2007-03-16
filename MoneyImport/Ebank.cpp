/*
 * MoneyImport : Convert Bank csv file to MS Money OFX file.
 *
 * Copyright (c) 2001-2003 Takuya Murakami. All rights reserved.
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
 * $Id$
 */

#include <vcl.h>
#pragma hdrstop
#include <stdio.h>
#include "Account.h"
#include "Transaction.h"
#include "Ebank.h"

//
// Ebank アカウント
//
EbankAccount::EbankAccount(void)
{
	Ident = "Ebank";
	BankName = "イーバンク銀行";
	BankId = "0036";
}

TransactionList * EbankAccount::ReadFile(FILE *fp)
{
	TransactionList *list = new EbankTransaction;
	if (list->ReadCsv(fp) < 0) {
		delete list;
		return NULL;
	}
	return list;
}

// 
// Ebank トランザクションリスト
//
Transaction *EbankTransaction::GenerateTransaction(int nrows, char **rows, int *err)
{
	*err = 0;
	if (nrows != 4) return NULL;

	Transaction *trans = new Transaction;

	/* 取引日,入出金(円),残高(円),入出金先内容*/
	int date = atoi(rows[0]);
	trans->date.year  = date / 10000;
	trans->date.month = (date % 10000) / 100;
	trans->date.date  = date % 100;

	trans->date.hour = 0;
	trans->date.minutes = 0;
	trans->date.seconds = 0;

	/* Transaction ID は、日付内の取引順で決めることにする */
	trans->id = GenerateTransactionId(date);

	trans->value = atol(rows[1]);
	if (trans->value < 0) {
		trans->SetTransactionType(rows[3], T_OUTGO);
	} else {
		trans->SetTransactionType(rows[3], T_INCOME);
	}
	trans->desc = utf8(rows[3]);
	trans->balance = atol(rows[2]);

	return trans;
}
