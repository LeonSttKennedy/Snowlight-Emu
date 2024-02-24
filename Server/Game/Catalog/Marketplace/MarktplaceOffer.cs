using Snowlight.Game.Items;
using Snowlight.Storage;
using Snowlight.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Snowlight.Game.Catalog
{
    public class MarketplaceOffers
    {
        private uint mId;
        private uint mUserId;
        private uint mDefinitionId;
        private int mOfferState;
        private int mItemType;
        private uint mSprite;
        private string mExtraData;
        private int mAskingPrice;
        private int mTotalPrice;
        private double mTimestamp;
        private int mLimitedNumber;
        private int mLimitedStack;


        public uint Id 
        {
            get 
            {
                return mId;
            }
        }
        public uint UserId
        {
            get
            {
                return mUserId;
            }
        }
        public uint DefinitionId
        {
            get
            {
                return mDefinitionId;
            }
        }
        public int State
        {
            get
            {
                return mOfferState;
            }
        }
        public int ItemType
        {
            get
            {
                return mItemType;
            }
        }
        public uint Sprite
        {
            get
            {
                return mSprite;
            }
        }
        public string ExtraData
        {
            get
            {
                return mExtraData;
            }
        }
        public int AskingPrice
        {
            get
            {
                return mAskingPrice;
            }
        }
        public int TotalPrice
        {
            get
            {
                return mTotalPrice;
            }
        }
        public double Timestamp
        {
            get
            {
                return mTimestamp;
            }
        }
        public int LimitedNumber
        {
            get
            {
                return mLimitedNumber;
            }
        }
        public int LimitedStack
        {
            get
            {
                return mLimitedStack;
            }
        }

        public ItemDefinition Definition
        {
            get
            {
                return ItemDefinitionManager.GetDefinition(mDefinitionId);
            }
        }

        private DateTime Future
        {
            get
            {
                double TimeLeft = mTimestamp + (ServerSettings.MarketplaceOfferTotalHours * 60 * 60);
                DateTime Future = UnixTimestamp.GetDateTimeFromUnixTimestamp(TimeLeft);
                
                return Future;
            }
        }

        public int MinutesLeft
        {
            get
            {
                TimeSpan MinLef = Future - DateTime.Now;

                return Compare() ? 0 : (int)MinLef.TotalMinutes;
            }
        }

        public MarketplaceOffers(uint OfferID, uint UserID, uint ItemID, int State, uint Sprite, string ExtraData, int AskingPrice, int TotalPrice, int ItemType, double Timestamp, int LimitedNumber, int LimitedStack)
        {
            mId = OfferID;
            mUserId = UserID;
            mDefinitionId = ItemID;
            mOfferState = State;
            mSprite = Sprite;
            mExtraData = ExtraData;
            mItemType = ItemType;
            mTimestamp = Timestamp;
            mAskingPrice = AskingPrice;
            mTotalPrice = TotalPrice;
            mLimitedNumber = LimitedNumber;
            mLimitedStack = LimitedStack;
        }

        public uint GroupBy()
        {
            switch (mItemType)
            {
                default:
                case 1:

                    return mSprite;

                case 2:

                    return mExtraData != string.Empty ? uint.Parse(mExtraData) : mSprite;
            }
        }

        public int CountOffers()
        {
            int Count = 0;
            switch (mItemType)
            {
                default:
                case 1:
                    Count = CatalogManager.MarketplaceOffers.Count(c => c.Value.State == 1 && c.Value.Sprite == mSprite);
                    return Count;

                case 2:

                    Count = CatalogManager.MarketplaceOffers.Count(c => c.Value.State == 1 && c.Value.Sprite == mSprite && c.Value.ExtraData == mExtraData);
                    return Count;
            }
        }

        public int GetTotalSoldsToday()
        {
            int TotalSolds = 0;

            switch (mItemType)
            {
                default:
                case 1:
                    {
                        if (CatalogManager.MarketplaceAvarages.ContainsKey(mSprite))
                        {
                            foreach (MarketplaceAvarage Item in CatalogManager.MarketplaceAvarages[mSprite])
                            {
                                TimeSpan SoldTime = DateTime.Now - Item.SoldTimeStamp;

                                if (SoldTime.TotalDays <= 1)
                                {
                                    TotalSolds += 1;
                                }
                            }
                        }

                        return TotalSolds;
                    }

                case 2:
                    {
                        if (CatalogManager.MarketplaceAvarages.ContainsKey(mSprite))
                        {
                            foreach (MarketplaceAvarage Item in CatalogManager.MarketplaceAvarages[mSprite])
                            {
                                TimeSpan SoldTime = DateTime.Now - Item.SoldTimeStamp;

                                if (Item.ExtraData == mExtraData && SoldTime.TotalDays <= 1)
                                {
                                    TotalSolds += 1;
                                }
                            }
                        }

                        return TotalSolds;
                    }
            }
        }

        public bool Compare()
        {
            int Compare = DateTime.Compare(DateTime.Now, Future);

            return Compare > -1;
        }

        public void CheckForExpiracy(SqlDatabaseClient MySqlClient)
        {
            if (Compare() && mOfferState != 2)
            {
                mOfferState = 3;
                MySqlClient.SetParameter("offerid", mId);
                MySqlClient.SetParameter("state", mOfferState);
                MySqlClient.ExecuteNonQuery("UPDATE catalog_marketplace_offers SET state = @state WHERE offer_id = @offerid LIMIT 1");
            }
        }
    }
}