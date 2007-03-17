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

#ifndef	_CARD_H
#define	_CARD_H

#include <stdio.h>
#include "Convert.h"
#include "Transaction.h"

class Card {
    protected:
	AnsiString	Ident;
	AnsiString	CardName;
	AnsiString	CardId;
        AnsiString	SFCPeepPath;

    public:
	virtual TransactionList *ReadCard(void) = 0;
	inline void SetCardInfo(AnsiString &id) {
        	CardId = id;
	}
        inline void SetSFCPeepPath(AnsiString path) {
        	SFCPeepPath = path;
        }
	inline TransactionList * Card::ReadCard(TransactionList *list) {
		if (list->ReadData(SFCPeepPath) < 0) {
			delete list;
			return NULL;
		} 
		return list;
	}

	inline char *getIdent(void)	{ return Ident.c_str(); }
	inline char *getCardName(void)	{ return CardName.c_str(); }
	inline char *getCardId(void)    { return CardId.c_str(); }
};

class Cards {
    protected:
	Card	*cards[10];
	int num_cards;

    public:
	Cards(void);
	void AddCard(Card *card);
	inline int NumCards(void) { return num_cards; }
	inline Card *GetCard(int n) { return cards[n]; }
	TransactionList * ReadCard(AnsiString SFCPeepPath, Card **match);
};

#endif	// _CARD_H
