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
//---------------------------------------------------------------------------

#include <vcl.h>
#pragma hdrstop
#include <stdio.h>

#define DEFINE_TRNNAME
#include "Transaction.h"

struct trntable trntable_income[] = {
	{"利息", T_INT},
	{"振込 ", T_DIRECTDEP},
        {"ﾁｬｰｼﾞ", T_DIRECTDEP},	// Edy チャージ
  	{"入金", T_DIRECTDEP},	// Suica チャージ
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

// タブで区切られた token を取得する
char * TransactionList::getTabbedToken(char **pos)
{
        char *ret = *pos;

	if (*pos == NULL) {
        	return NULL;	// no more token
        }

	char *nextpos = strchr(*pos, '\t');
	if (nextpos) {
                *nextpos = '\0';
                *pos = nextpos + 1;
        } else {
        	*pos = NULL;	// no more token
        }
        return ret;

}

//
// タブ区切りデータを処理する
//
int TransactionList::ParseLines(TStringList *lines, bool reverse)
{
	char buf[3000];
	AnsiString rows[30];
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
                char *p;
                char *pp = buf;

                int n;
                for (n= 0; (p = getTabbedToken(&pp)) != NULL; n++) {
                	rows[n] = p;
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


