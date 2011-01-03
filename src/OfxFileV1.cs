/*
 * FeliCa2Money
 *
 * Copyright (C) 2001-2010 Takuya Murakami
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

/*
   OFX ファイル生成
   ファイルバージョンは 1.0.2 (SGML)
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows.Forms;

namespace FeliCa2Money
{
    /// <summary>
    /// OFXファイルバージョン1
    /// </summary>
    class OfxFileV1 : OfxFile
    {
        /// <summary>
        /// OFXファイル書き出し
        /// </summary>
        /// <param name="accounts">アカウントリスト</param>
        public override void WriteFile(List<Account> accounts)
        {
            Ofx ofx = new Ofx();

            // OFX 要素を生成する
            ofx.genOfx(accounts);

            StreamWriter w = new StreamWriter(this.ofxFilePath, false); //, Encoding.UTF8);
            w.NewLine = "\n";

            // SGMLヘッダ出力
            w.WriteLine("OFXHEADER:100");
            w.WriteLine("DATA:OFXSGML");
            w.WriteLine("VERSION:102");
            w.WriteLine("SECURITY:NONE");
            w.WriteLine("ENCODING:UTF-8");
            w.WriteLine("CHARSET:CSUNICODE");
            w.WriteLine("COMPRESSION:NONE");
            w.WriteLine("OLDFILEUID:NONE");
            w.WriteLine("NEWFILEUID:NONE");
            w.WriteLine("");

            // OFX 要素出力
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            XmlTextWriter xw = new XmlTextWriter(sw);
            xw.Formatting = Formatting.Indented;

            ofx.doc.WriteTo(xw);
            w.Write(sb);

            xw.Close();
            sw.Close();
            w.Close();
        }
    }
}
