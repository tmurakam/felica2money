using System;
using System.Collections.Generic;
using System.Text;

namespace FeliCa2Money
{
    public class CsvReadException : System.Exception
    {
        public CsvReadException(String message)
            : base(message)
        {
        }
    }
}
