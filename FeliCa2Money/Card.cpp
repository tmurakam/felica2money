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

#include "Card.h"

Cards::Cards(void)
{
	num_cards = 0;
}

void Cards::AddCard(Card *card)
{
	cards[num_cards] = card;
	num_cards++;
}       

TransactionList * Cards::ReadCard(Card **matchcard)
{
	TransactionList *t;

	for (int i=0; i<num_cards; i++) {
		t = cards[i]->ReadCard();
		if (t) {
			*matchcard = cards[i];
			return t;
		}
	}
	Application->MessageBox("カードを読むことができませんでした",
				"エラー", MB_OK);
	return NULL;
}
