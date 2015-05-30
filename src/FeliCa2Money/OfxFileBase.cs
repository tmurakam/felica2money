/*
 * FeliCa2Money
 *
 * Copyright (C) 2001-2011 Takuya Murakami
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
    /// OFXファイル生成: 基底クラス
    /// </summary>
    abstract public class OfxFile
    {
        private const int ERROR_NO_ASSOCIATION = 1155; // Win32 エラーコード

        private string mOfxFilePath;

        /// <summary>
        /// OFXファイルインスタンス生成
        /// </summary>
        /// <param name="version">バージョン</param>
        /// <returns>OfxFile</returns>
        public static OfxFile newOfxFile(int version)
        {
            switch (version)
            {
                case 1:
                    return new OfxFileV1();
                case 2:
                    return new OfxFileV2();
            }
            return null;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public OfxFile()
        {
        }

        public string ofxFilePath
        {
            get { return mOfxFilePath; }
            set { mOfxFilePath = value; }
        }

        /// <summary>
        /// OFXファイル書き出し
        /// </summary>
        /// <param name="accounts">アカウントリスト</param>
        public abstract void WriteFile(List<Account> accounts);

        /// <summary>
        /// OFXファイル書き出し (backward compat)
        /// </summary>
        /// <param name="account">アカウント</param>
        public void WriteFile(Account account)
        {
            List<Account> accounts = new List<Account>();
            accounts.Add(account);
            WriteFile(accounts);
        }

        /// <summary>
        /// OFX ファイルをアプリケーションで開く
        /// </summary>
        public void Execute()
        {
            String errorMessage = null;

            try
            {
                // throw new System.ComponentModel.Win32Exception(1155); // for test
                System.Diagnostics.Process.Start(mOfxFilePath);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                if (ex.NativeErrorCode == ERROR_NO_ASSOCIATION)
                {
                    errorMessage = Properties.Resources.NoOfxAssociation;
                }
                else
                {
                    errorMessage = ex.Message;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            if (errorMessage != null)
            {
                MessageBox.Show(errorMessage, Properties.Resources.Error);
            }
        }
    }
}
