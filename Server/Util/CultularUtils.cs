﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Utils
{
    public class CultularUtils
    {
        public readonly static NumberFormatInfo NumberFormatInfo = new NumberFormatInfo();

        static CultularUtils()
        {
            CultularUtils.NumberFormatInfo.NumberDecimalSeparator = ".";
        }
    }
}
