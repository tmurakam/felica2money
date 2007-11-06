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
#include "felicalib.h"
#include "Card.h"
#include "Transaction.h"
#include "SfcPeep.h"
#include "Nanaco.h"

//
// Nanaco
//

static int read4b(uint8 *p);
static int read2b(uint8 *p);

/// コンストラクタ
NanacoCard::NanacoCard(void)
{
	Ident = "Nanaco";
	CardName = "Nanaco";
}

TransactionList * NanacoCard::ReadCard(void)
{
    NanacoTransactionList *list = new NanacoTransactionList;
    if (list->readCard(CardId) < 0) {
	delete list;
	return NULL;
    }

    return list;
}

int NanacoTransactionList::readCard(AnsiString& cardId)
{
    pasori *p = pasori_open(NULL);
    if (!p) {
	return -1;	// open failed
    }
    pasori_init(p);

    felica *f = felica_polling(p, 0xfe00, 0, 0);
    if (!f) {
	pasori_close(p);
	return -1;	// can't read card
    }

    // get card id
    int i;
    for (i = 0; i < 16; i++) {
    	char buf[8];
        sprintf(buf, "%02x", f->IDm[i]);
        cardId += buf;
    }

    for (i = 0; ; i++) {
	uint8 data[16];
	if (felica_read_without_encryption02(f, 0x564f, 0, (uint8)i, data) != 0) {
	    break;
	}

	Transaction *t = new Transaction;

	switch (data[0]) {
	case 0x47:
	default:
	    t->desc = sjis2utf8("Nanaco支払");
	    t->type = T_DEBIT;
	    break;
	case 0x6f:
	    t->desc = sjis2utf8("Nanacoチャージ");
	    t->type = T_DEP;
	    break;
	}

	// 金額
	t->value = read4b(data + 1);
        if (t->type == T_DEBIT) {
            t->value = - t->value;
        }

	// 残高
	t->balance = read4b(data + 5);

	// 日付/時刻
	int v = read4b(data + 9);
	t->date.year = 2000 + (v >> 21);
	t->date.month = (v >> 17) & 0xf;
	t->date.date =  (v >> 12) & 0x1f;
	t->date.hour = (v >> 6) & 0x3f;
	t->date.minutes = v & 0x3f;
	t->date.seconds = 0;

	// ID
	t->id = read2b(data + 13);

	list.insert(list.begin(), t);
    }

    pasori_close(p);
}

// dummy
Transaction *NanacoTransactionList::GenerateTransaction(int nrows, AnsiString *rows, int *err)
{
    return NULL;
}

static int read4b(uint8 *p)
{
    int v;
    v = (*p++) << 24;
    v |= (*p++) << 16;
    v |= (*p++) << 8;
    v |= *p;
    return v;
}

static int read2b(uint8 *p)
{
    int v;
    v = (*p++) << 8;
    v |= *p;
    return v;
}
