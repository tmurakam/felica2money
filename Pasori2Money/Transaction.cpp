/*
 * MoneyImport : Convert Japan Net Bank csv file to MS Money OFX file.
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
//---------------------------------------------------------------------------

#include <vcl.h>
#pragma hdrstop
#include <stdio.h>

#define DEFINE_TRNNAME
#include "Transaction.h"

struct trntable trntable_income[] = {
	{"利息", T_INT},
	{"振込 ", T_DIRECTDEP},
        {"ﾁｬｰｼﾞ", T_DIRECTDEP},
	{NULL, T_DEP}
};

struct trntable trntable_outgo[] = {
	{"ＡＴＭ", T_ATM},
	{NULL, T_DEBIT}
};

void Transaction::SetTransactionType(const char *desc, int type)
{
	struct trntable *tab;

	switch (type) {
	    case T_INCOME:
		tab = trntable_income;
		break;

	    case T_OUTGO:
		tab = trntable_outgo;
		break;

	    default:
		/* ### */
		break;
	}

	while (tab->key) {
		if (strstr(desc, tab->key) != 0) {
			this->type = tab->type;
			return;
		}
		tab++;
	}
	this->type = tab->type;
	return;
}

const char *Transaction::GetTrnTypeStr(void)
{
	return trnname[type];
}

TransactionList::~TransactionList()
{
	Transaction *next;

	while (head) {
		next = head->next;
		delete head;

		head = next;
	}
}

//
// タブ区切りデータを処理する
//
int TransactionList::ParseLines(TStringList *lines, bool reverse)
{
	char buf[3000];
	char *rows[30];
	int i;
        int start, incr, end, count, err;

	count = lines->Count;
	if (reverse) {
                start = count - 1;
 		end = -1;
                incr = -1;
        } else {
        	start = 0;
        	end = count;
        	incr = 1;
        }
	for (i = start; i != end; i += incr) {
                strncpy(buf, lines->Strings[i].c_str(), sizeof(buf));

		// タブ区切りを分解
		int n = 0;
		char *p = strtok(buf, "\t");
		while (p && n < 30) {
			rows[n++] = p;
			p = strtok(NULL, "\t");
		}

		Transaction *t = GenerateTransaction(n, rows, &err);
		if (!t) {
			if (err) return -1;	// fatal error
			continue;
		}

		if (!tail) {
			head = tail = t;
		} else {
			tail->next = t;
			tail = t;
		}
		t->next = NULL;
	}
	return 0;
}

//
// トランザクション ID 作成
//
int TransactionList::GenerateTransactionId(int key)
{
	if (key != prev_key) {
		serial = 0;
		prev_key = key;
	} else {
		serial++;
	}
	return serial;
}

//
// ユーティリティ関数
//

// SJIS->UTF8
AnsiString utf8(char *sjis)
{
	wchar_t wbuf[1500];
	char buf[3000];
        AnsiString utf8;

        MultiByteToWideChar(CP_OEMCP, 0, sjis, -1,
        	wbuf, sizeof(buf) / 2);
        WideCharToMultiByte(CP_UTF8, 0, wbuf, -1,
        	buf, sizeof(buf), NULL, NULL);

	utf8 = buf;
        return utf8;
}


