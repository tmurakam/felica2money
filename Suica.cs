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

//  Suica/PASMO/ICOCA/PiTaPa/Toica など交通系カード処理

using System;
using System.Collections.Generic;
using System.Text;
using FelicaLib;

namespace FeliCa2Money
{
    class Suica : CardWithFelicaLib
    {
        // 物販エリアコード
        public const int AreaSuica = 1;
        public const int AreaIcoca = 2;
        public const int AreaIruca = 4;

        private StationCode stCode;

        public Suica()
        {
            ident       = "Suica";
            cardName    = "Suica";

            systemCode  = (int)SystemCode.Suica;
            serviceCode = 0x090f;       // 履歴エリアのサービスコード
            needReverse = true;

            stCode = new StationCode();
        }

        public override void Dispose()
        {
            base.Dispose();
            stCode.Dispose();
        }

        // カード ID を取得
        // Suica では IDm を用いる
        public override void analyzeCardId(Felica f)
        {
            byte[] data = f.IDm();
            if (data == null)
            {
                throw new Exception("IDm を読み取れません");
            }
            
            cardId = "";
            for (int i = 0; i < 8; i++) {
                cardId += data[i].ToString("X2");
            }
        }

        // 後処理
        //  Suica の場合、履歴には残高しか記録されていない。
        //  ここでは、残高の差額から各トランザクションの金額を計算する
        //  (このため、最も古いエントリは金額を計算できない)
        protected override void PostProcess(List<Transaction> list)
        {
            int prevBalance = 0;

            foreach (Transaction t in list)
            {
                t.value = t.balance - prevBalance;
                prevBalance = t.balance;
            }
            list.RemoveAt(0);   // 最古のエントリは捨てる
        }

        // トランザクション解析
        public override bool analyzeTransaction(Transaction t, byte[] data)
        {
            int ctype = data[0];    // 端末種
            int proc = data[1];     // 処理
            int date = (data[4] << 8) | data[5]; // 日付
            int balance = (data[11] << 8) | data[10];   // 残高(little endian)
            int seq = (data[12] << 16) | (data[13] << 8) | data[14]; // 連番
            int region = data[15];      // リージョン

            // 処理
            t.desc = procType(proc);

            // 残高
            t.balance = balance;

            // 金額は PostProcess で計算する
            t.value = 0;

            // 日付
            int yy = (date >> 9) + 2000;
            int mm = (date >> 5) & 0xf;
            int dd = date & 0x1f;
            try
            {
                t.date = new DateTime(yy, mm, dd, 0, 0, 0);
            }
            catch
            {
                // 日付異常(おそらく空エントリ)
                return false;
            }

            // ID
            t.id = seq;

            // 駅名/店舗名などを調べる
            int in_line = -1;
            int in_sta = -1;
            int out_line, out_sta;
            string[] in_name = null, out_name = null;
            int area;

            switch (ctype)
            {
                case CT_SHOP:
                case CT_VEND:
                    // 物販/自販機
                    area = Properties.Settings.Default.ShopAreaPriority;

                    //time = (data[6] << 8) | data[7];
                    out_line = data[8];
                    out_sta = data[9];

                    // 優先エリアで検索
                    out_name = stCode.getShopName(area, ctype, out_line, out_sta);
                    if (out_name == null)
                    {
                        // 全エリアで検索
                        out_name = stCode.getShopName(-1, ctype, out_line, out_sta);
                    }
                    break;

                case CT_CAR:
                    // 車載端末(バス)
                    out_line = (data[6] << 8) | data[7];
                    out_sta = (data[8] << 8) | data[9];
                    out_name = stCode.getBusName(out_line, out_sta);
                    break;

                default:
                    // それ以外(運賃、チャージなど)
                    in_line = data[6];
                    in_sta = data[7];
                    out_line = data[8];
                    out_sta = data[9];
                    if (in_line == 0 && in_sta == 0 && out_line == 0 && out_sta == 0)
                    {
                        break;
                    }

                    // エリアを求める
                    if (region >= 1) {
                        area = 2; // 関西公営・私鉄
                    }
                    else if (in_line >= 0x80)
                    {
                        area = 1; // 関東公営・私鉄
                    }
                    else
                    {
                        area = 0; // JR
                    }

                    in_name = stCode.getStationName(area, in_line, in_sta);
                    out_name = stCode.getStationName(area, out_line, out_sta);
                    break;
            }

            // 備考の先頭には端末種を入れる
            t.memo = consoleType(ctype);

            switch (ctype) 
            {
                case CT_SHOP:
                case CT_VEND:
                    if (out_name != null)
                    {
                        // 適用に店舗名を入れる
                        t.desc += " " + out_name[0] + " " + out_name[1];
                    }
                    else
                    {
                        // 店舗名が不明の場合、適用には出線区/出駅順コードをそのまま付与する。
                        // こうしないと Money が過去の履歴から誤って店舗名を補完してしまい
                        // 都合がわるいため
                        t.desc += " 店舗コード:" + out_line.ToString("X02") + out_sta.ToString("X02");
                    }
                    break;

                case CT_CAR:
                    if (out_name != null)
                    {
                        // 適用にバス会社名、備考に停留所名を入れる
                        t.desc += " " + out_name[0];
                        t.memo += " " + out_name[1];
                    }
                    break;

                default:
                    if (in_line == 0 && in_sta == 0 & out_line == 0 && out_sta == 0)
                    {
                        // チャージなどの場合は、何も追加しない
                        break;
                    }

                    // 適用に入会社または出会社を追加
                    if (in_name != null)
                    {
                        t.desc += " " + in_name[0];
                    }
                    else if (out_name != null)
                    {
                        t.desc += " " + out_name[0];
                    }

                    // 備考に入出会社/駅名を記載
                    if (in_name != null) {
                        t.memo += " " + in_name[0] + "(" + in_name[1] + ")";
                    } else {
                        t.memo += " 未登録";
                    }
                    t.memo += " - ";

                    if (out_name != null) {
                        t.memo += out_name[0] + "(" + out_name[1] + ")";
                    } else {
                        t.memo += "未登録";
                    }
                    break;
            }

            return true;
        }

        private const int CT_SHOP = 0xc7;  // 物販端末
        private const int CT_VEND = 0xc8;  // 自販機
        private const int CT_CAR = 0x05;   // 車載端末(バス)

        // 端末種文字列を返す
        private string consoleType(int ctype)
        {
            switch (ctype) {
            case CT_SHOP: return "物販端末";
            case CT_VEND: return "自販機";
            case CT_CAR: return "車載端末";
            case 0x03: return "清算機";
            case 0x08: return "券売機";
            case 0x12: return "券売機";
            case 0x16: return "改札機";
            case 0x17: return "簡易改札機";
            case 0x18: return "窓口端末";
            case 0x1a: return "改札端末";
            case 0x1b: return "携帯電話";
            case 0x1c: return "乗継清算機";
            case 0x1d: return "連絡改札機";
            }
            return "不明";
        }

        // 処理種別文字列を返す
        private string procType(int proc)
        {
            switch (proc) {
            case 0x01: return "運賃";
            case 0x02: return "チャージ";
            case 0x03: return "券購";
            case 0x04: return "清算";
            case 0x07: return "新規";
            case 0x0d: return "バス";
            case 0x0f: return "バス";
            case 0x14: return "オートチャージ";
            case 0x46: return "物販";
            case 0x49: return "サンクスチャージ";
            case 0x4a: return "物販取消";
            case 0xc6: return "物販(現金併用)";
            }
            return "不明";
        }
    }
}
