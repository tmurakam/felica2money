/*
 * ImportMoney : Convert Bank csv file to MS Money OFX file.
 *
 * Copyright (c) 2001, Takuya Murakami. All rights reserved.
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

#ifndef _SONYBANK_H
#define _SONYBANK_H

#define SB_IDENT	"\"Ç®éÊÇËà¯Ç´ì˙\",\"ìEóv\",\"Ç®óaÇØì¸ÇÍäz\",\"Ç®à¯Ç´èoÇµäz\",\"ç∑Çµà¯Ç´écçÇ\""

class SBAccount : public Account {
    public:
	SBAccount(void);
	virtual TransactionList *	ReadFile(FILE *fp);
};

class SBTransaction : public TransactionList
{
    private:
	virtual const char *Ident(void) { return SB_IDENT; }
	int prev_date;
	int serial;

    public:
	inline SBTransaction(void) { prev_date = serial = 0; }
	virtual Transaction *GenerateTransaction(int nrows, char **rows, int *err);
};

#endif

