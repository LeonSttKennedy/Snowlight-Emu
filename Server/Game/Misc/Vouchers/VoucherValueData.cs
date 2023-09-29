using Snowlight.Util;
using System;
using System.Collections.Generic;

namespace Snowlight.Game.Misc
{
    public class VoucherValueData
    {
        private int mValueCredits;
        private int mValueActivityPoints;
        private SeasonalCurrencyList mSeasonalCurrency;
        private List<uint> mValueFurni;

        public int ValueCredits
        {
            get
            {
                return mValueCredits;
            }
        }

        public int ValueActivityPoints
        {
            get
            {
                return mValueActivityPoints;
            }
        }
        public SeasonalCurrencyList SeasonalCurrency
        {
            get
            {
                return mSeasonalCurrency;
            }
        }
        public List<uint> ValueFurni
        {
            get
            {
                return mValueFurni;
            }
        }

        public VoucherValueData(int ValueCredits, int ValueActivityPoints, SeasonalCurrencyList SeasonalCurrency, List<uint> ValueFurni)
        {
            mValueCredits = ValueCredits;
            mValueActivityPoints = ValueActivityPoints;
            mSeasonalCurrency = SeasonalCurrency;
            mValueFurni = ValueFurni;
        }
    }
}
