using System;
using System.Collections.Generic;

namespace Snowlight.Game.Misc
{
    public class MarketplaceFiltersCache
    {
        private int mMinPrice;
        private int mMaxPrice;
        private string mSearchQuery;
        private int mFilterMode;

        public int MinPrice
        {
            get
            {
                return mMinPrice;
            }
        }

        public int MaxPrice
        {
            get
            {
                return mMaxPrice;
            }
        }

        public string SearchQuery
        {
            get
            {
                return mSearchQuery;
            }
        }

        public int FilterMode
        {
            get
            {
                return mFilterMode;
            }
        }

        public MarketplaceFiltersCache()
        {
            mMinPrice = -1;
            mMaxPrice = -1;
            mSearchQuery = string.Empty;
            mFilterMode = 0;
        }

        public void FillCache(int MinPrice, int MaxPrice, string SearchQuery, int FilterMode)
        {
            mMinPrice = MinPrice;
            mMaxPrice = MaxPrice;
            mSearchQuery = SearchQuery;
            mFilterMode = FilterMode;
        }
    }
}
