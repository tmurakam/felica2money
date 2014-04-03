using System;
using System.Collections.Generic;
using System.Text;

namespace FeliCa2Money
{
    class CsvReadException : Exception
    {
        public CsvReadException(String message)
            : base(message)
        {
        }
    }
}
