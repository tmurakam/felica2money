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
#include <Registry.hpp>

#include <shellapi.h>

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "MainForm.h"
#include "Convert.h"
#include "Account.h"
#include "Transaction.h"

static AnsiString dateStr(DateTime *dt)
{
	AnsiString str;

	/*              Y   M   D   H   M   S */
	str.sprintf("%4d%02d%02d%02d%02d%02d[+9:JST]",
		dt->year, dt->month, dt->date,
		dt->hour, dt->minutes, dt->seconds);
	return str;
}

static void
WriteOfx(FILE *fp, TransactionList *list, Card *card)
{
	unsigned long idoffset;
	Transaction *t, *last;

	last = list->Tail();
	t = list->Head();

	/* OFX ヘッダ */
	fprintf(fp, "OFXHEADER:100\n");
	fprintf(fp, "DATA:OFXSGML\n");
	fprintf(fp, "VERSION:102\n");
	fprintf(fp, "SECURITY:NONE\n");
	fprintf(fp, "ENCODING:UTF-8\n");
	fprintf(fp, "CHARSET:CSUNICODE\n");
	fprintf(fp, "COMPRESSION:NONE\n");
	fprintf(fp, "OLDFILEUID:NONE\n");
	fprintf(fp, "NEWFILEUID:NONE\n");
	fprintf(fp, "\n");

	/* 金融機関情報(サインオンレスポンス) */
	fprintf(fp, "<OFX>\n");
	fprintf(fp, "<SIGNONMSGSRSV1>\n");
	fprintf(fp, "<SONRS>\n");
	fprintf(fp, "  <STATUS>\n");
	fprintf(fp, "    <CODE>0\n");
	fprintf(fp, "    <SEVERITY>INFO\n");
	fprintf(fp, "  </STATUS>\n");
	fprintf(fp, "  <DTSERVER>%s\n", dateStr(&last->date).c_str());

	fprintf(fp, "  <LANGUAGE>JPN\n");
	fprintf(fp, "  <FI>\n");
	fprintf(fp, "    <ORG>%s\n", card->getIdent());
	fprintf(fp, "  </FI>\n");
	fprintf(fp, "</SONRS>\n");
	fprintf(fp, "</SIGNONMSGSRSV1>\n");

	/* 口座情報(バンクメッセージレスポンス) */
	fprintf(fp, "<BANKMSGSRSV1>\n");

	/* 預金口座型明細情報作成 */
	fprintf(fp, "<STMTTRNRS>\n");
	fprintf(fp, "<TRNUID>0\n");
	fprintf(fp, "<STATUS>\n");
	fprintf(fp, "  <CODE>0\n");
	fprintf(fp, "  <SEVERITY>INFO\n");
	fprintf(fp, "</STATUS>\n");

	fprintf(fp, "<STMTRS>\n");
	fprintf(fp, "  <CURDEF>JPY\n");

	fprintf(fp, "  <BANKACCTFROM>\n");
	fprintf(fp, "    <BANKID>%s\n", 	card->getIdent());
	fprintf(fp, "    <BRANCHID>%s\n", 	"000");
	fprintf(fp, "    <ACCTID>%s\n", 	card->getCardId());
	fprintf(fp, "    <ACCTTYPE>SAVINGS\n");
	fprintf(fp, "  </BANKACCTFROM>\n");

	/* 明細情報開始(バンクトランザクションリスト) */
	fprintf(fp, "  <BANKTRANLIST>\n");
	fprintf(fp, "    <DTSTART>%s\n", dateStr(&t->date).c_str());
	fprintf(fp, "    <DTEND>%s\n", dateStr(&last->date).c_str());

	/* トランザクション */
	do {
		fprintf(fp, "    <STMTTRN>\n");
		fprintf(fp, "      <TRNTYPE>%s\n", t->GetTrnTypeStr());
		fprintf(fp, "      <DTPOSTED>%s\n", dateStr(&t->date).c_str());
		fprintf(fp, "      <TRNAMT>%d\n", t->value);

		/* トランザクションの ID は日付と取引番号で生成 */
		fprintf(fp, "      <FITID>%04d%02d%02d%07d\n",
			t->date.year, t->date.month, t->date.date,
			t->id);
		fprintf(fp, "      <NAME>%s\n", t->desc);
		fprintf(fp, "    </STMTTRN>\n");
	} while ((t = list->Next()) != NULL);

	fprintf(fp, "  </BANKTRANLIST>\n");

	/* 残高 */
	fprintf(fp, "  <LEDGERBAL>\n");
	fprintf(fp, "    <BALAMT>%d\n", last->balance);
	fprintf(fp, "    <DTASOF>%s\n", dateStr(&last->date).c_str());
	fprintf(fp, "  </LEDGERBAL>\n");

	/* OFX 終了 */
	fprintf(fp, "  </STMTRS>\n");
	fprintf(fp, "</STMTTRNRS>\n");
	fprintf(fp, "</BANKMSGSRSV1>\n");
	fprintf(fp, "</OFX>\n");
}

void Convert(AnsiString sfcpeeppath, AnsiString ofxfile, Cards *cards)
{
	TransactionList *t;

	// CSV ファイルを読む
	Card *card;
	t = cards->ReadCard(sfcpeeppath, &card);

        if (!t) return;

        // OFX ファイルを書き出す
	FILE *fp = fopen(ofxfile.c_str(), "wb");
	if (!fp) {
        	Application->MessageBox("OFXファイルを開けません", "エラー", MB_OK);
		return;
       	}
        WriteOfx(fp, t, card);
	fclose(fp);

        // Money 起動	
        ShellExecute(NULL, "open", ofxfile.c_str(),
        	NULL, NULL, SW_SHOW);
}

