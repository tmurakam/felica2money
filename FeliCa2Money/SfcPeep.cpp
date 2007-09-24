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

#include "SfcPeep.h"

//---------------------------------------------------------------------------
class SFCPeep *SfcPeep = NULL;

//---------------------------------------------------------------------------
/// constructor
SFCPeep::SFCPeep(void)
{
	lines = new TStringList;
}
//---------------------------------------------------------------------------
/// destructor
SFCPeep::~SFCPeep()
{
	delete lines;
}

//---------------------------------------------------------------------------
/**
   @brief SFCPeep を実行
   @param[in] arg SFCPeepに渡すオプション
   @return エラーコード (0:成功, -1:失敗)

   読み込まれたデータは lines に格納される。
*/
int SFCPeep::Execute(AnsiString arg)
{
	AnsiString cmdline;

	cmdline.sprintf("\"%s\" %s", SFCPeepPath.c_str(), arg.c_str());
        STARTUPINFO si;
        PROCESS_INFORMATION pi;

        // open temp file
        SECURITY_ATTRIBUTES sa;
        memset(&sa, 0, sizeof(sa));
        sa.nLength = sizeof(sa);
        sa.lpSecurityDescriptor = NULL;
        sa.bInheritHandle = TRUE;
        HANDLE hFile = CreateFile(TempFile.c_str(),
        	GENERIC_READ | GENERIC_WRITE,
   		FILE_SHARE_READ | FILE_SHARE_WRITE,
                &sa, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);

        // create process
        memset(&si, 0, sizeof(si));
        si.cb = sizeof(si);
        si.dwFlags = STARTF_USESTDHANDLES;
        si.hStdInput = stdin;
        si.hStdOutput = hFile;
        si.hStdError = hFile;
        memset(&pi, 0, sizeof(pi));
	int ret = CreateProcess(NULL, cmdline.c_str(), NULL, NULL, TRUE,
        	CREATE_NO_WINDOW, NULL,
                ExtractFilePath(SFCPeepPath).c_str(),
                &si, &pi);

	if (ret == 0) {
		Application->MessageBox("SFCPeep 実行エラー\n設定を確認してください", "エラー", MB_OK);
                CloseHandle(hFile);
                return -1;
        }

        WaitForSingleObject(pi.hProcess, INFINITE);
        CloseHandle(hFile);

        lines->LoadFromFile(TempFile);
        DeleteFile(TempFile);

        return 0;
}

#pragma package(smart_init)
