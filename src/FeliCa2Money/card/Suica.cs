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
    public class Suica : FelicaCard
    {
        // 物販エリアコード
        public const int AreaSuica = 1;
        public const int AreaIcoca = 2;
        public const int AreaIruca = 4;

        private readonly StationCode _stCode;

        public Suica()
        {
            Ident       = "Suica";
            AccountName    = "Suica";

            _systemCode  = (int)SystemCode.Suica;
            _serviceCode = 0x090f;       // 履歴エリアのサービスコード
            _needReverse = true;
            _needCalcValue = true;

            _stCode = new StationCode();
        }

        public override void Dispose()
        {
            base.Dispose();
            _stCode.Dispose();
        }

        // トランザクション解析
        public override bool AnalyzeTransaction(Transaction t, byte[] data)
        {
            int ctype = data[0];    // 端末種
            int proc = data[1];     // 処理
            int date = Read2B(data, 4); // 日付
            int balance = Read2L(data, 10);   // 残高(little endian)
            int seq = Read3B(data, 12); // 連番
            int region = data[15];      // リージョン

            // 処理
            t.Desc = ProcType(proc);

            // 残高
            t.Balance = balance;

            // 金額は CalcValueFromBalance() で計算する
            t.Value = 0;

            // 日付
            int yy = (date >> 9) + 2000;
            int mm = (date >> 5) & 0xf;
            int dd = date & 0x1f;
            try
            {
                t.Date = new DateTime(yy, mm, dd, 0, 0, 0);
            }
            catch
            {
                // 日付異常(おそらく空エントリ)
                return false;
            }

            // ID
            t.Id = seq;

            // 駅名/店舗名などを調べる
            int in_line = -1;
            int in_sta = -1;
            int out_line, out_sta;
            StationCode.Names in_name = null, out_name = null;
            int in_area = 0, out_area = 0;

            switch (ctype)
            {
                case CtShop:
                case CtVend:
                    // 物販/自販機
                    out_area = Properties.Settings.Default.ShopAreaPriority;

                    //time = (data[6] << 8) | data[7];
                    out_line = data[8];
                    out_sta = data[9];

                    // 優先エリアで検索
                    out_name = _stCode.GetShopName(out_area, ctype, out_line, out_sta);
                    if (out_name == null)
                    {
                        // 全エリアで検索
                        out_name = _stCode.GetShopName(-1, ctype, out_line, out_sta);
                    }
                    break;

                case CtCar:
                    // 車載端末(バス)
                    out_line = Read2B(data, 6);
                    out_sta = Read2B(data, 8);
                    out_name = _stCode.GetBusName(out_line, out_sta);
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
                    in_area  = GetAreaCode(in_line, region);
                    out_area = GetAreaCode(out_line, region);

                    in_name  = _stCode.GetStationName(in_area, in_line, in_sta);
                    out_name = _stCode.GetStationName(out_area, out_line, out_sta);
                    break;
            }

            // 備考の先頭には端末種を入れる
            t.Memo = ConsoleType(ctype);

            switch (ctype) 
            {
                case CtShop:
                case CtVend:
                    if (out_name != null)
                    {
                        // "物販" の場合は、"物販" は消して店舗名だけにする
                        if (t.Desc == "物販")
                        {
                            t.Desc = "";
                        }
                        else
                        {
                            t.Desc += " ";
                        }
                        // 店舗名追加
                        t.Desc += out_name.r1 + " " + out_name.r2;
                    }
                    else
                    {
                        // 店舗名が不明の場合、適用には出線区/出駅順コードをそのまま付与する。
                        // こうしないと Money が過去の履歴から誤って店舗名を補完してしまい
                        // 都合がわるいため
                        t.Desc += " 店舗コード:" + out_line.ToString("X02") + out_sta.ToString("X02");
                    }
                    break;

                case CtCar:
                    if (out_name != null)
                    {
                        // 適用にバス会社名、備考に停留所名を入れる
                        t.Desc += " " + out_name.r1;
                        t.Memo += " " + out_name.r2;
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
                        t.Desc += " " + in_name.r1;
                    }
                    else if (out_name != null)
                    {
                        t.Desc += " " + out_name.r1;
                    }

                    // 備考に入出会社/駅名を記載
                    t.Memo += " ";
                    if (in_name != null) {
                        t.Memo += in_name.r1 + "(" + in_name.r2 + ")";
                    } else {
                        t.Memo += string.Format("未登録({0}:{1}:{2})", in_area, in_line, in_sta);
                    }
                    t.Memo += " - ";

                    if (out_name != null) {
                        t.Memo += out_name.r1 + "(" + out_name.r2 + ")";
                    } else {
                        t.Memo += string.Format("未登録({0}:{1}:{2})", out_area, out_sta, region);
                    }
                    break;
            }

            return true;
        }

        // エリアコードを求める
        private int GetAreaCode(int line, int region)
        {
            if (line < 0x80)
            {
                return 0; // JR
            }
            else if (region == 0)
            {
                return 1; // 関東公営・私鉄
            }
            else
            {
                return 2;
            }
        }

        private const int CtShop = 199;  // 物販端末
        private const int CtVend = 200;  // 自販機
        private const int CtCar = 5;   // 車載端末(バス)

        // 端末種文字列を返す
        private string ConsoleType(int ctype)
        {
            switch (ctype) {
            case CtShop: return "物販端末";
            case CtVend: return "自販機";
            case CtCar: return "車載端末";
            case 3: return "清算機";
            case 4: return "携帯型端末";
            case 8: return "券売機";
            case 9: return "入金機";
            case 18: return "券売機";
            case 20:
            case 21: return "券売機等";
            case 22: return "改札機";
            case 23: return "簡易改札機";
            case 24:
            case 25: return "窓口端末";
            case 26: return "改札端末";
            case 27: return "携帯電話";
            case 28: return "乗継精算機";
            case 29: return "連絡改札機";
            case 31: return "簡易入金機";
            case 70:
            case 72: return "VIEW ALTTE";
            }
            return "不明";
        }

        // 処理種別文字列を返す
        private string ProcType(int proc)
        {
            switch (proc) {
            case 1: return "運賃";
            case 2: return "チャージ";
            case 3: return "券購";
            case 4: 
            case 5: return "清算";
            case 6: return "窓出";
            case 7: return "新規";
            case 8: return "控除";
            case 13: return "バス"; //PiTaPa系
            case 15: return "バス"; //IruCa系
            case 17: return "再発行";
            case 19: return "支払";
            case 20:
            case 21: return "オートチャージ";
            case 31: return "バスチャージ";
            case 35: return "券購";
            case 70: return "物販";
            case 72: return "特典";
            case 73: return "入金";
            case 74: return "物販取消";
            case 75: return "入場物販";
            case 132: return "精算(他社)";
            case 133: return "精算(他社入場)";
            case 198: return "物販(現金併用)";
            case 203: return "物販(入場現金併用)";
            }
            return "不明";
        }
    }
}
