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

/**
   @brief トランザクションタイプを自動的に設定する
   @param[in] desc 取引文字列
   @param[in] type 取引タイプ(T_INCOME / T_OUTGO)

   トランザクションタイプは type に格納される
*/
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

/**
   @brief トランザクションタイプ文字列を取得する
   @return トランザクションタイプ文字列
*/
const char *Transaction::GetTrnTypeStr(void)
{
	return trnname[type];
}

/// デストラクタ
TransactionList::~TransactionList()
{
	vector<Transaction*>::iterator it;

	for (it = list.begin(); it != list.end(); it++) {
		delete *it;
	}
}

/**
   @brief タブで区切られた token を取得する
   @param[in,out] pos 読み込み位置
   @return token
*/
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

/**
   @brief タブで区切られた各行を解析する
   @param[in] lines 処理する行(複数行)
   @param[in] reverse 逆順に処理するかどうかのフラグ
   @return 0 で成功、-1 でエラー

   解析結果は list に格納される
*/
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

		list.push_back(t);
	}
	return 0;
}

/**
   @brief トランザクションIDの生成
   @param[in] キー
   @return シリアル番号

   キーが前回と異なっていれば常に 0 を返す
   同じ場合はシリアル番号をインクリメントして返す
*/
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

/**
   @brief SJIS->UTF8変換
*/
AnsiString sjis2utf8(const AnsiString & sjis)
{
	wchar_t wbuf[1500];
	char buf[3000];
        AnsiString utf8;

        MultiByteToWideChar(932/*CP_932, Shift-JIS*/, 0, sjis.c_str(), -1,
        	wbuf, sizeof(buf) / 2);
        WideCharToMultiByte(CP_UTF8, 0, wbuf, -1,
        	buf, sizeof(buf), NULL, NULL);

	utf8 = buf;
        return utf8;
}


