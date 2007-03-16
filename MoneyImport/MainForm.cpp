/*
 * MoneyImport : Convert Bank transaction csv file to MS Money OFX file.
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
#include <ShellApi.h>

#include "MainForm.h"
#include "Convert.h"
#include "Account.h"

#include "JNB.h"
#include "SonyBank.h"
#include "Ebank.h"

//---------------------------------------------------------------------------
#pragma package(smart_init)
#pragma resource "*.dfm"
TMForm *MForm;
//---------------------------------------------------------------------------
__fastcall TMForm::TMForm(TComponent* Owner)
	: TForm(Owner)
{
}
//---------------------------------------------------------------------------
void __fastcall TMForm::FormShow(TObject *Sender)
{
	accounts.AddAcount(new JNBAccount);
	accounts.AddAcount(new SBAccount);
	accounts.AddAcount(new EbankAccount);

	LoadRegistry();

	if (ParamCount() == 1) {
		Convert(ParamStr(1));
		Application->Terminate();
	}
}
//---------------------------------------------------------------------------
void __fastcall TMForm::ButtonConvertClick(TObject *Sender)
{
	if (OpenDialog->Execute()) {
		Convert(OpenDialog->FileName);
	}
}
//---------------------------------------------------------------------------
void TMForm::Convert(AnsiString csvfile)
{
	AnsiString ofxfile = ExtractFilePath(Application->ExeName) +
		"ImportMoney.ofx";

#if 0
	JNBAccount jnb;
	jnb.SetAccount(EditJNBBranch->Text, EditJNBAccount->Text);

	SBAccount sb;
	sb.SetAccount(EditSBBranch->Text, EditSBAccount->Text);

	Accounts acs;
	acs.AddAcount(&jnb);
	acs.AddAcount(&sb);
#endif

	int n = accounts.NumAccount();
	for (int i = 0; i < n; i++) {
		Account *ac = accounts.GetAccount(i);
		ac->SetAccount(
			AcGrid->Cells[2][i+1],	// branch
			AcGrid->Cells[3][i+1]	// account
		);
	}


	::Convert(csvfile, ofxfile, &accounts);
}
//---------------------------------------------------------------------------
void __fastcall TMForm::EditJNBAccountExit(TObject *Sender)
{
	SaveRegistry();
}

//---------------------------------------------------------------------------

void __fastcall TMForm::ButtonQuitClick(TObject *Sender)
{
	Application->Terminate();
}
//---------------------------------------------------------------------------
void TMForm::LoadRegistry(void)
{
	// レジストリから口座番号のよみとり
	TRegistry *reg = new TRegistry();

	reg->RootKey = HKEY_CURRENT_USER;
	reg->OpenKey("\\Software\\Takuya Murakami\\ImportMoney", true);

#if 0
	EditJNBBranch->Text  = reg->ReadString("JNBBranchId");
	EditJNBAccount->Text = reg->ReadString("JNBAccountId");

	EditSBBranch->Text  = reg->ReadString("SBBranchId");
	EditSBAccount->Text = reg->ReadString("SBAccountId");
#endif

	AcGrid->Cells[0][0] = "銀行名";
	AcGrid->Cells[1][0] = "銀行ID";
	AcGrid->Cells[2][0] = "支店番号";
	AcGrid->Cells[3][0] = "口座番号";

	int n = accounts.NumAccount();
	for (int i = 0; i < n; i++) {
		Account *ac = accounts.GetAccount(i);
		AcGrid->Cells[0][i+1] = ac->getBankName();
		AcGrid->Cells[1][i+1] = ac->getBankId();

		AnsiString bid, aid;
		bid = aid = ac->getIdent();
		bid += "BranchId";
		aid += "AccountId";

		AcGrid->Cells[2][i+1] = reg->ReadString(bid);
		AcGrid->Cells[3][i+1] = reg->ReadString(aid);
	}
}
//---------------------------------------------------------------------------
void TMForm::SaveRegistry(void)
{
	TRegIniFile *ini;

	// レジストリに口座番号を保存
	TRegistry *reg = new TRegistry();

	reg->RootKey = HKEY_CURRENT_USER;
	reg->OpenKey("\\Software\\Takuya Murakami\\ImportMoney", true);

#if 0
	reg->WriteString("JNBBranchId", EditJNBBranch->Text);
	reg->WriteString("JNBAccountId", EditJNBAccount->Text);

	reg->WriteString("SBBranchId",  EditSBBranch->Text);
	reg->WriteString("SBAccountId", EditSBAccount->Text);
#endif

	int n = accounts.NumAccount();
	for (int i = 0; i < n; i++) {
		Account *ac = accounts.GetAccount(i);

		AnsiString bid, aid;
		bid = aid = ac->getIdent();
		bid += "BranchId";
		aid += "AccountId";

		reg->WriteString(bid, AcGrid->Cells[2][i+1]);
		reg->WriteString(aid, AcGrid->Cells[3][i+1]);
	}
}

//---------------------------------------------------------------------------
void __fastcall TMForm::ButtonHelpClick(TObject *Sender)
{
	ShellExecute(NULL, "open", "MoneyImport.chm",
        NULL, NULL, SW_SHOWDEFAULT);
}
//---------------------------------------------------------------------------

void __fastcall TMForm::AcGridExit(TObject *Sender)
{
	SaveRegistry();	
}
//---------------------------------------------------------------------------

