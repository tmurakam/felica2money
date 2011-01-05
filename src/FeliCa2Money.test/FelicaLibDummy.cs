// -*-  Mode:C++; c-basic-offset:4; tab-width:4; indent-tabs-mode:t -*-

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FelicaLib
{
    public class DummyFelica : IFelica
    {
        private Hashtable dataBufs = new Hashtable();
        private int systemCode;

        public DummyFelica()
        {
            // do nothing
        }

        public void Dispose()
        {
            dataBufs = null;
        }

        public void Polling(int s)
        {
            systemCode = s;
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
            byte[] data = (byte[])dataBufs[systemCode << 16 | sv];

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

        public void SetSystemCode(int s)
        {
            systemCode = s;
        }

        public void SetTestData(int sv, byte[] data)
        {
            dataBufs[systemCode << 16 | sv] = data;
        }

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
