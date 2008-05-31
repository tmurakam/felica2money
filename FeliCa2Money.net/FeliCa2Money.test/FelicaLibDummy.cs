using System;
using System.Collections.Generic;
using System.Text;

namespace FelicaLib
{
    public class DummyFelica : IFelica
    {
        private byte[] dataBuf = null;
        private int pos = 0;

        public DummyFelica()
        {
            // do nothing
        }

        public void Dispose()
        {
            // do nothing
        }

        public void Polling(int systemcode)
        {
            pos = 0;
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

        public byte[] ReadWithoutEncryption(int servicecode, int addr)
        {
            byte[] data = new byte[16];

            if (pos > data.Length)
            {
                return null;
            }

            for (int i = 0; i < 16; i++)
            {
                data[i] = dataBuf[pos + i];
            }
            pos += 16;
            return data;
        }

        public void SetTestData(byte[] data)
        {
            dataBuf = data;
        }
    }
}
