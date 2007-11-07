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
#include <Registry.hpp>

#include <shellapi.h>

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include <iostream>
#include <iomanip>

using namespace std;

#include "MainForm.h"
#include "Convert.h"
#include "Card.h"
#include "Transaction.h"

/**
   @brief コンバート実行
   @param[in] card 読み込み元 Card
   @param[out] ofxfile 書き出しを行う OFX ファイル名

   カード情報を読み込み、OFX ファイルに書き出す
*/
void Converter::Convert(Card *c, AnsiString ofxfile)
{
	card = c;

	// CSV ファイルを読む
	int ret = card->ReadCard();

        if (ret) {
               	Application->MessageBox("カードを読むことができませんでした", "エラー", MB_OK);
        	return;
        }
        if (!card->hasAnyTransaction()) {
        	Application->MessageBox("履歴が一件もありません", "エラー", MB_OK);
		return;               
        }

        // OFX ファイルを書き出す
	ofstream ofs(ofxfile.c_str());
	if (!ofs) {
        	Application->MessageBox("OFXファイルを開けません", "エラー", MB_OK);
		return;
       	}
	WriteOfx(ofs);
	ofs.close();

        // Money 起動	
        ShellExecute(NULL, "open", ofxfile.c_str(),
        	NULL, NULL, SW_SHOW);
}

/**
   @brief OFX ファイル書き出し
   @param[in] ofs 出力ストリーム
*/
void Converter::WriteOfx(ofstream& ofs)
{
	unsigned long idoffset;
        vector<Transaction*>::iterator begin, last;

        begin = card->begin();
	last = card->end();
	last--;

        AnsiString beginDate = dateStr((*begin)->date);
        AnsiString endDate = dateStr((*last)->date);

	/* OFX ヘッダ */
	ofs << "OFXHEADER:100" << endl;
	ofs << "DATA:OFXSGML" << endl;
	ofs << "VERSION:102" << endl;
	ofs << "SECURITY:NONE" << endl;
	ofs << "ENCODING:UTF-8" << endl;
	ofs << "CHARSET:CSUNICODE" << endl;
	ofs << "COMPRESSION:NONE" << endl;
	ofs << "OLDFILEUID:NONE" << endl;
	ofs << "NEWFILEUID:NONE" << endl;
	ofs << endl;

	/* 金融機関情報(サインオンレスポンス) */
	ofs << "<OFX>" << endl;
	ofs << "<SIGNONMSGSRSV1>" << endl;
	ofs << "<SONRS>" << endl;
	ofs << "  <STATUS>" << endl;
	ofs << "    <CODE>0" << endl;
	ofs << "    <SEVERITY>INFO" << endl;
	ofs << "  </STATUS>" << endl;
	ofs << "  <DTSERVER>" << beginDate.c_str() << endl;

	ofs << "  <LANGUAGE>JPN" << endl;
	ofs << "  <FI>" << endl;
	ofs << "    <ORG>" <<  card->getIdent() << endl;
	ofs << "  </FI>" << endl;
	ofs << "</SONRS>" << endl;
	ofs << "</SIGNONMSGSRSV1>" << endl;

	/* 口座情報(バンクメッセージレスポンス) */
	ofs << "<BANKMSGSRSV1>" << endl;

	/* 預金口座型明細情報作成 */
	ofs << "<STMTTRNRS>" << endl;
	ofs << "<TRNUID>0" << endl;
	ofs << "<STATUS>" << endl;
	ofs << "  <CODE>0" << endl;
	ofs << "  <SEVERITY>INFO" << endl;
	ofs << "</STATUS>" << endl;

	ofs << "<STMTRS>" << endl;
	ofs << "  <CURDEF>JPY" << endl;

	ofs << "  <BANKACCTFROM>" << endl;
	ofs << "    <BANKID>" << card->getIdent() << endl;
	ofs << "    <BRANCHID>" << "000" << endl;
	ofs << "    <ACCTID>" << card->getCardId() << endl;
	ofs << "    <ACCTTYPE>SAVINGS" << endl;
	ofs << "  </BANKACCTFROM>" << endl;

	/* 明細情報開始(バンクトランザクションリスト) */
	ofs << "  <BANKTRANLIST>" << endl;
	ofs << "    <DTSTART>" << beginDate.c_str() << endl;
	ofs << "    <DTEND>" << endDate.c_str() << endl;

	/* トランザクション */
	vector<Transaction*>::iterator it;

        for (it = card->begin(); it != card->end(); it++) {
        	Transaction *t = *it;

		ofs << "    <STMTTRN>" << endl;
		ofs << "      <TRNTYPE>" <<  t->GetTrnTypeStr() << endl;
		ofs << "      <DTPOSTED>" << dateStr(t->date).c_str() << endl;
		ofs << "      <TRNAMT>" <<  t->value << endl;

		ofs << "      <FITID>" << transIdStr(t).c_str() << endl;

		ofs << "      <NAME>" << t->desc.c_str() << endl;
                if (!t->memo.IsEmpty()) {
			ofs << "      <MEMO>" << t->memo.c_str() << endl;
                }
		ofs << "    </STMTTRN>" << endl;
	};

	ofs << "  </BANKTRANLIST>" << endl;

	/* 残高 */
	ofs << "  <LEDGERBAL>" << endl;
	ofs << "    <BALAMT>" << (*last)->balance << endl;
	ofs << "    <DTASOF>" << endDate.c_str() << endl;
	ofs << "  </LEDGERBAL>" << endl;

	/* OFX 終了 */
	ofs << "  </STMTRS>" << endl;
	ofs << "</STMTTRNRS>" << endl;
	ofs << "</BANKMSGSRSV1>" << endl;
	ofs << "</OFX>" << endl;
}

AnsiString Converter::dateStr(const DateTime & dt)
{
	AnsiString str;

	/*              Y   M   D   H   M   S */
	str.sprintf("%4d%02d%02d%02d%02d%02d[+9:JST]",
		dt.year, dt.month, dt.date,
		dt.hour, dt.minutes, dt.seconds);
	return str;
}

/**
   @brief トランザクションIDを取得
   @param[in] t トランザクション
   @out トランザクションID

   日付とトランザクションID から文字列を生成する
*/
AnsiString Converter::transIdStr(const Transaction *t)
{
	AnsiString str;

	str.sprintf("%04d%02d%02d%07d",
		    t->date.year, t->date.month, t->date.date, t->id);
	return str;
}


