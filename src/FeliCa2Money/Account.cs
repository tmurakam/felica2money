/*
 * FeliCa2Money
 *
 * Copyright (C) 2001-2015 Takuya Murakami
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

using System;
using System.Collections.Generic;
using System.Text;
using FelicaLib;

namespace FeliCa2Money
{
    /// <summary>
    /// アカウントクラス
    /// </summary>
    public abstract class Account
    {
        private string _branchId = "0";   // 支店番号
        private string _accountId = "";   // 口座番号

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected Account()
        {
            IsCreditCard = false;
            Balance = 0;
            HasBalance = false;
            BankId = "Felica2Money";
            Transactions = new TransactionList();
        }

        public abstract void ReadTransactions();

        /// <summary>
        /// 識別子
        /// </summary>
        public string Ident { get; set; }

        /// <summary>
        /// 銀行ID
        /// </summary>
        public string BankId { get; set; }

        /// <summary>
        /// 支店ID
        /// </summary>
        public string BranchId
        {
            get {
                if (_branchId == "") return "0";
                return _branchId;
            }
            set { _branchId = value; }
        }

        /// <summary>
        /// 口座名
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 口座ID
        /// </summary>
        public string AccountId
        {
            get {
                if (_accountId == "") return "0";
                return _accountId;
            }
            set { _accountId = value; }
        }

        /// <summary>
        /// 残高有無
        /// </summary>
        public bool HasBalance { set; get; }

        /// <summary>
        /// 残高
        /// </summary>
        public int Balance { set; get; }

        /// <summary>
        /// クレジットカードならtrue
        /// </summary>
        public bool IsCreditCard { set; get; }

        /// <summary>
        /// 取引リスト
        /// </summary>
        public TransactionList Transactions { get; protected set; }

        // タブ区切りの分解 (SFCPeep用)
#if false
        protected string[] ParseLine(string line)
        {
            return line.Split('\t');
        }
#endif
    }
}
