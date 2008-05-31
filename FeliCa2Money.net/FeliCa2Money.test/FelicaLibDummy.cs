using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FelicaLib
{
    public class DummyFelica : IFelica
    {
        private Hashtable dataBufs = new Hashtable();
        private int pos = 0;
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
            pos += 16;
            return data;
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
    }
}
