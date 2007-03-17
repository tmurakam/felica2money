/*
 * Pasori2Money
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

#ifndef	_CONVERT_H
#define	_CONVERT_H

#if 0
//
// 取引種類
//
typedef enum {
	// 入金
	T_INT=0,	// 利息
	T_DIV,		// 配当
	T_DIRECTDEP,	// 振込入金、取立入金、自動引落戻し入金
	T_DEP,		// その他入金

	// 出金
	T_PAYMENT,	// 自動引き落とし
	T_CASH,		// 現金引き出し
	T_ATM,		// カードによる引き出し
	T_CHECK,	// 小切手関連取引	
	T_DEBIT,	// その他出金
} trntype;

#define	T_INCOME	0
#define	T_OUTGO		1



// 日付データ
typedef struct {
	int year;
	int month;
	int date;
	int hour;
	int minutes;
	int seconds;
} DateTime;

//
// トランザクションデータ
//
typedef struct _transaction {
	struct _transaction *next;

	DateTime	date;	// 日付
	unsigned long	id; 	// ID
	AnsiString	desc;          // 説明
	trntype		type;		// 種別
	long		value;		// 金額
        long		balance;	// 残高
} Transaction;

// utility funcs
extern void SplitLine(char *line, char **rows);
extern trntype GetTrnType(const char *desc, int type);
extern AnsiString utf8(char *sjis);

#endif

class Cards;
extern void Convert(AnsiString ofxfile, Cards *cards);

#endif	// _CONVERT_H

