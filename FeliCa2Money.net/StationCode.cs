/*
 * FeliCa2Money
 *
 * Copyright (C) 2001-2008 Takuya Murakami
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

// stationcode.mdb アクセスクラス (試作中)

using System;
using System.Data.OleDb;

namespace FeliCa2Money
{
    class StationCode : IDisposable
    {
        private OleDbConnection conn;

        public StationCode()
        {
            conn = new System.Data.OleDb.OleDbConnection();
            conn.ConnectionString = Properties.Settings.Default.StationCodeConnectionString;
            conn.Open();
        }

        public void Dispose()
        {
            conn.Close();
        }

        private string[] doQuery(string sql)
        {
            OleDbCommand cmd;
            OleDbDataReader dr;

            cmd = new OleDbCommand(sql, conn);
            dr = cmd.ExecuteReader();

            string[] result = null;
            if (dr.Read())
            {
                result = new string[2];
                if (dr.IsDBNull(0))
                {
                    result[0] = "";
                }
                else
                {
                    result[0] = dr.GetString(0);
                }

                if (dr.IsDBNull(1))
                {
                    result[1] = "";
                }
                else
                {
                    result[1] = dr.GetString(1);
                }
            }
            dr.Close();
            return result;
        }

        // 駅名を検索する
        public string[] getStationName(int area, int line, int station)
        {
            string sql = string.Format("SELECT CompanyName,StationName FROM StationCode WHERE"
                + " AreaCode={0} AND LineCode={1} AND StationCode={2}", area, line, station);
            return doQuery(sql);
        }

        // 店舗名を検索する
        // area = -1 として検索すると、area 指定なしとみなす
        public string[] getShopName(int area, int terminal, int line, int station)
        {
            string sql = string.Format("SELECT CompanyName,ShopName FROM ShopCode WHERE"
                + " TerminalCode={0} AND LineCode={1} AND StationCode={2}", terminal, line, station);
            if (area >= 0)
            {
                sql += " AND AreaCode=" + area.ToString();
            }
            return doQuery(sql);
        }

        // バス停留所名を検索する
        public string[] getBusName(int line, int station)
        {
            string sql = string.Format("SELECT BusCompanyName,BusStationName FROM BusCode WHERE"
                + " BusLineCode={0} AND BusStationCode={1}", line, station);
            return doQuery(sql);
        }
    }
}
