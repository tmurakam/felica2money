// -*-  Mode:C++; c-basic-offset:4; tab-width:4; indent-tabs-mode:nil -*-

// felicalib のダミーライブラリ

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FelicaLib
{
    public class DummyFelica : IFelica
    {
        // データバッファ
        // キーは、(システムコード<<16 | サービスコード)。
        // 値は、サービスコードに対応するバイトの配列。要素数は16の倍数。
        private Dictionary<int, byte[]> _dataBufs = new Dictionary<int, byte[]>();

        // システムコード(16bit)
        private int _systemCode;

        public DummyFelica()
        {
            // do nothing
        }

        public void Dispose()
        {
            _dataBufs = null;
        }

        public void Polling(int s)
        {
            _systemCode = s;
        }

        public byte[] IDm()
        {
            byte[] buf = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                buf[i] = (byte)i;
            }
            return buf;
        }    

        public byte[] PMm()
        {
            byte[] buf = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                buf[i] = (byte)(7 - i);
            }
            return buf;
        }    

        public byte[] ReadWithoutEncryption(int sv, int addr)
        {
            byte[] ret = new byte[16];
            byte[] data = _dataBufs[_systemCode << 16 | sv];

            if (data == null || data.Length < (addr + 1) * 16)
            {
                return null;
            }

            for (int i = 0; i < 16; i++)
            {
                ret[i] = data[addr * 16 + i];
            }
            return ret;
        }

        // set data

        // テスト用システムコードをセットする
        public void SetSystemCode(int s)
        {
            _systemCode = s;
        }

        /// <summary>
        /// テスト用データをセットする
        /// <param name="sv">サービスコード</param>
        /// <param name="data">データ配列</param>
        /// </summary>
        public void SetTestData(int sv, byte[] data)
        {
            _dataBufs[_systemCode << 16 | sv] = data;
        }

        /// <summary>
        /// テスト用データをセットする
        /// データは文字列の配列で渡す。各文字列は、１６バイトのデータを
        /// 各バイトを16進数表記し、スペースまたはタブ区切りで並べたもの。
        /// <param name="sv">サービスコード</param>
        /// <param name="data">データ : string 配列</param>
        /// </summary>
        public void SetTestDataFromStrings(int sv, string[] ss)
        {
            byte[] buf = new byte[ss.Length * 16];

            for (int i = 0; i < ss.Length; i++)
            {
                string[] cols = ss[i].Split(new char[] { ' ', '\t' });
                for (int j = 0; j < 16; j++)
                {
                    buf[i * 16 + j] = (byte)int.Parse(cols[j + 1], System.Globalization.NumberStyles.HexNumber);
                }
            }

            SetTestData(sv, buf);
        }
    }
}
