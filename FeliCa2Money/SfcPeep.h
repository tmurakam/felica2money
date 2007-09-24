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

#ifndef SfcPeepH
#define SfcPeepH
//---------------------------------------------------------------------------
/**
   @brief SFCPeep を制御するクラス
*/
class SFCPeep {
private:
	AnsiString	SFCPeepPath;	///< SFCPeep のフルパス名
	AnsiString	TempFile;	///< テンポラリファイル名
        TStringList	*lines;		///< SFCPeep の各出力行

public:
	SFCPeep(void);
	~SFCPeep();

	/// SFCPeep のパスを設定
	void SetSfcPeepPath(AnsiString path) { SFCPeepPath = path; }

	/// テンポラリファイルパス名を設定
	void SetTempFile(AnsiString path)	{ TempFile = path; }

	int Execute(AnsiString arg);

	/// lines を返す
        TStringList * Lines(void) { return lines; }
};

extern class SFCPeep *SfcPeep;

#endif
