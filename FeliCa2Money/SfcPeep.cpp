//---------------------------------------------------------------------------
#include <vcl.h>
#pragma hdrstop

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
void SFCPeep::Execute(AnsiString arg)
{
	AnsiString cmdline;

	cmdline.sprintf("\"%s\" %s > %s", SFCPeepPath.c_str(), arg.c_str(), TempFile.c_str());
	system(cmdline.c_str());

        lines->LoadFromFile(TempFile);

        DeleteFile(TempFile);
}

#pragma package(smart_init)
