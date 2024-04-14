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
        private bool mCanReedemInCatalog;

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

        public bool CanReedemInCatalog
        {
            get
            {
                return mCanReedemInCatalog;
            }
        }

        public VoucherValueData(int ValueCredits, int ValueActivityPoints, SeasonalCurrencyList SeasonalCurrency, List<uint> ValueFurni, bool CanReedemInCatalog)
        {
            mValueCredits = ValueCredits;
            mValueActivityPoints = ValueActivityPoints;
            mSeasonalCurrency = SeasonalCurrency;
            mValueFurni = ValueFurni;
            mCanReedemInCatalog = CanReedemInCatalog;

        }
    }
}
