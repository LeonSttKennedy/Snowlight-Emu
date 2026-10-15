using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Util
{
    public static class ConversionUtil
    {
        public static BigInteger BinaryToInt(string Binario)
        {
            BigInteger decimalValue = BigInteger.Zero;

            foreach (char bit in Binario)
            {
                decimalValue = decimalValue * 2 + (bit - '0');
            }

            return decimalValue;
        }
    }
}
