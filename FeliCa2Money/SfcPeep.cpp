//---------------------------------------------------------------------------
#include <vcl.h>
#pragma hdrstop
#include <stdio.h>

#include "SfcPeep.h"

//---------------------------------------------------------------------------
class SFCPeep *SfcPeep = NULL;

//---------------------------------------------------------------------------
// constructor
SFCPeep::SFCPeep(void)
{
	lines = new TStringList;
}
//---------------------------------------------------------------------------
// destructor
SFCPeep::~SFCPeep()
{
	delete lines;
}

//---------------------------------------------------------------------------
// execute
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
