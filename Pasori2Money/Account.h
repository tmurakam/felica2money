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

#ifndef	_ACCOUNT_H
#define	_ACCOUNT_H

#include <stdio.h>
#include "Convert.h"
#include "Transaction.h"

class Account {
    protected:
	AnsiString	Ident;
	AnsiString	BankName;
	AnsiString	BankId;
	AnsiString	BranchId;
	AnsiString	AccountId;

    public:
	virtual TransactionList *ReadFile(FILE *fp) = 0;
	inline void SetAccount(AnsiString &b, AnsiString &a) {
		BranchId = b;
		AccountId = a;
	}
	inline AnsiString getIdent(void) { return Ident; }
	
	inline char *getBankName(void)	{ return BankName.c_str(); }
	inline char *getBankId(void)    { return BankId.c_str(); }
	inline char *getBranchId(void)  { return BranchId.c_str(); }
	inline char *getAccountId(void) { return AccountId.c_str(); }
};

class Accounts {
    protected:
	Account	*acct[10];
	int num_acct;

    public:
	Accounts(void);
	void AddAcount(Account *ac);
	inline int NumAccount(void) { return num_acct; }
	inline Account *GetAccount(int n) { return acct[n]; }
	TransactionList * ReadFile(FILE *fp, Account **match);
};

#endif	// _ACCOUNT_H
