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

#ifndef	_CARD_H
#define	_CARD_H

#include <stdio.h>
#include "Transaction.h"

/**
   @brief カード情報クラス
*/
class CardBase {
protected:	
    AnsiString	Ident;		///< カード名 (Ident)
    AnsiString	CardName;	///< カード名
    AnsiString	CardId;		///< カード固有ID (IDm)

public:
    /// カードIDを設定する
    inline void SetCardId(AnsiString &id) {
	CardId = id;
    }

    /// Ident を返す
    inline char *getIdent(void)	{ return Ident.c_str(); }

    /// カード名を返す
    inline char *getCardName(void)	{ return CardName.c_str(); }

    /// カード固有IDを返す
    inline char *getCardId(void)    { return CardId.c_str(); }
};

/**
   @brief カード情報クラス(トランザクション管理つき)
*/
class Card : public CardBase
{
protected:
    vector<Transaction*> list;	///< トランザクションリスト

private:
    int prev_key, serial;		///< トランザクションID生成用

public:
    Card();
    virtual ~Card();

    /**
       @brief カードを読み込む
	   
       カードを読み込み、カードIDを取得し、トランザクションリストを構成する。
    */
    virtual int ReadCard(void) = 0;

    inline bool hasAnyTransaction(void) { return list.size() > 0 ? true : false; }

    inline vector<Transaction *>::iterator begin() {
	return list.begin();
    }
    inline vector<Transaction *>::iterator end() {
	return list.end();
    }

    /** トランザクション ID 生成 */
    int GenerateTransactionId(int key);
};

/**
   @brief カード情報クラス (ラインパーサ付き)
*/
class CardWithLineParser : public Card
{
public:
    int ParseLines(TStringList *lines, bool reverse = false);

private:    
    /**
       @brief トランザクションの生成
       @param nrows[in] 入力カラム数
       @param rows[in] 入力カラムの各文字列
       @param err[out] エラーコード
       @return トランザクション

       SFCPeep の各出力行に対し、各行のカラムを分解したものが引数に渡される。
       本関数は、これを解析し、トランザクションを生成して返す。
    */
    virtual Transaction *GenerateTransaction(int nrows, AnsiString *rows, int *err) = 0;

    /** タブで区切られたトークンを切り出す */
    char * getTabbedToken(char **pos);
};

#endif	// _CARD_H
