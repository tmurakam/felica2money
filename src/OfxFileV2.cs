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

// OFX ver 2.0 (XML)

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows.Forms;

namespace FeliCa2Money
{
    /// <summary>
    /// OFXファイルバージョン2
    /// </summary>
    class OfxFileV2 : OfxFile
    {
        /// <summary>
        /// OFX V2 ファイルを生成する
        /// </summary>
        /// <param name="accounts">アカウント</param>
        public override void WriteFile(List<Account> accounts)
        {
            mDoc = new XmlDocument();

            XmlDeclaration decl = mDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            mDoc.AppendChild(decl);

            // OFX 宣言
            XmlProcessingInstruction pi = mDoc.CreateProcessingInstruction("OFX",
                "OFXHEADER=\"200\" VERSION=\"200\" SECURITY=\"NONE\" OLDFILEUID=\"NONE\" NEWFILEUID=\"NONE\"");
            mDoc.AppendChild(pi);

            genOfxElement(mDoc, accounts);

            mDoc.Save(mOfxFilePath);
        }
    }
}
