//---------------------------------------------------------------------------

#ifndef SfcPeepH
#define SfcPeepH
//---------------------------------------------------------------------------
class SFCPeep {
private:
	AnsiString	SFCPeepPath;
	AnsiString	TempFile;
        TStringList	*lines;;

public:
	SFCPeep(void);
	~SFCPeep();
	void SetSfcPeepPath(AnsiString path) { SFCPeepPath = path; }
	void SetTempFile(AnsiString path)	{ TempFile = path; }

	int Execute(AnsiString arg);
        TStringList * Lines(void) { return lines; }
};

extern class SFCPeep *SfcPeep;

#endif
