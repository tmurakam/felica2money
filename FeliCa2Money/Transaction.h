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
#ifndef _TRANSACTION_H
#define _TRANSACTION_H

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

//
// 取引種類名 (上の値と順序一致すること)
//
#ifdef DEFINE_TRNNAME
const char *trnname[] = {
	"INT", "DIV", "DIRECTDEP", "DEP",
	"PAYMENT", "CASH", "ATM", "CHECK", "DEBIT"
};
#endif

//
// 取引種類変換表
//
struct trntable {
	const char	*key;
	trntype		type;
};

#define	T_INCOME	0
#define	T_OUTGO		1

//
// 日付情報
//
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
class Transaction {
    public:
	Transaction	*next;

	DateTime	date;		// 日付
	unsigned long	id; 		// ID
	AnsiString	desc;         	// 説明
        AnsiString	memo;		// メモ
	trntype		type;		// 種別
	long		value;		// 金額
	long		balance;	// 残高
	
	Transaction(void) { next = NULL; }
	void SetTransactionType(const char *desc, int type);

	const char *GetTrnTypeStr(void);
};

//
// トランザクション管理クラス
//   pure virtual なクラス。各銀行毎に派生させて使用する。
//
class Card;
class TransactionList {
    private:
	Transaction	*head, *tail, *pos;
	int prev_key, serial;
        AnsiString SFCPeepPath;

	virtual const char *Ident(void) = 0;
	virtual Transaction *GenerateTransaction(int nrows, AnsiString *rows, int *err) = 0;

        char * TransactionList::getTabbedToken(char **pos);

    public:
	inline TransactionList(void) { head = tail = 0; prev_key = serial = 0; }
	~TransactionList();
	int ParseLines(TStringList *lines, bool reverse = false);

	int GenerateTransactionId(int key);

 	inline bool hasAnyTransaction(void) { return head ? true : false; }

	inline Transaction *Tail(void) { return tail; }
	inline Transaction *Head(void) { pos = head; return head; }
	inline Transaction *Next(void) {
                if (pos != NULL) {
                	pos = pos->next;
                }
		return pos;
	}
};

// ユーティリティ関数
AnsiString utf8(char *sjis);

#endif

