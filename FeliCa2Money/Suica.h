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

#ifndef _SUICA_H
#define _SUICA_H

#include "Transaction.h"

class SuicaCard : public Card {
    public:
	SuicaCard(void);
	virtual TransactionList *ReadCard(void);
};

class SuicaTransactionList : public TransactionList
{
    private:
	virtual const char *Ident(void) { return ""; }
	long prevBalance;
        static const UndefBalance = -99999999;

    public:
    	SuicaTransactionList(void) { prevBalance = UndefBalance; }
	virtual Transaction *GenerateTransaction(int nrows, AnsiString *rows, int *err);
};

#endif

