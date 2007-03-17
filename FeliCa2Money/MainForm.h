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
//---------------------------------------------------------------------------

#ifndef MainFormH
#define MainFormH
//---------------------------------------------------------------------------
#include <Classes.hpp>
#include <Controls.hpp>
#include <StdCtrls.hpp>
#include <Forms.hpp>
#include <Menus.hpp>
#include <Dialogs.hpp>
#include <Grids.hpp>
#include "Card.h"

//---------------------------------------------------------------------------
class TMForm : public TForm
{
__published:	// IDE 管理のコンポーネント
	TOpenDialog *OpenDialog;
	TButton *ButtonConvert;
	TButton *ButtonQuit;
	TButton *ButtonConfig;
	TButton *ButtonHelp;
	TLabel *Label1;
	TLabel *Label2;
	TLabel *Label3;
	void __fastcall ButtonConvertClick(TObject *Sender);
	void __fastcall ButtonQuitClick(TObject *Sender);
	void __fastcall FormShow(TObject *Sender);
	void __fastcall ButtonHelpClick(TObject *Sender);
	void __fastcall ButtonConfigClick(TObject *Sender);
private:
	void SaveRegistry(void);
	void LoadRegistry(void);
public:		// ユーザー宣言
	__fastcall TMForm(TComponent* Owner);
	Cards cards;
 	AnsiString SFCPeepPath;
};
//---------------------------------------------------------------------------
extern PACKAGE TMForm *MForm;
//---------------------------------------------------------------------------
#endif
