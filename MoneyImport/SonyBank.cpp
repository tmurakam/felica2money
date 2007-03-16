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
#include "SonyBank.h"

SBAccount::SBAccount(void)
{
	Ident = "SonyBank";
	BankName = "ソニー銀行";
	BankId = "0035";
}

/*
 * ',' を含む文字列を数値に変換する
 */
static long atoi_wc(char *s)
{
	char buf[100], *p;

	p = buf;
	while (*s) {
		if (*s != ',') {
			*p++ = *s;
		}
		s++;
	}
	*p = '\0';
	return atol(buf);
}

TransactionList * SBAccount::ReadFile(FILE *fp)
{
	TransactionList *list = new SBTransaction;
	if (list->ReadCsv(fp) < 0) {
		delete list;
		return NULL;
	}
	return list;
}

// 
// SB トランザクションリスト
//

Transaction *SBTransaction::GenerateTransaction(int nrows, char **rows, int *err)
{
	Transaction *trans = new Transaction;

	/* 2002年01月01日 */
	/* 01234567890123 */
	char *d = rows[0];
	d[4] = '\0'; d[8] = '\0'; d[12] = '\0';
	trans->date.year = atoi(d);
	trans->date.month = atoi(d + 6);
	trans->date.date = atoi(d + 10);

	trans->date.hour = 0;
	trans->date.minutes = 0;
	trans->date.seconds = 0;

	/* Transaction ID は、日付内の取引順で決めることにする */
	int date = trans->date.year - 1970;
	date = date * 12 + trans->date.month;
	date = date * 31 + trans->date.date;
	trans->id = GenerateTransactionId(date);

	/* Description */
	trans->desc = utf8(rows[1]);

	if (strcmp(rows[3], "") != 0) {
		trans->SetTransactionType(rows[2], T_OUTGO);
		trans->value = - atoi_wc(rows[3]);
	} else {
		trans->SetTransactionType(rows[2], T_INCOME);
		trans->value = atoi_wc(rows[2]);
	}
	trans->balance = atoi_wc(rows[4]);

	return trans;
}
