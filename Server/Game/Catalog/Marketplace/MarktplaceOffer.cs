using Snowlight.Game.Items;
using Snowlight.Game.Rooms;
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
        private uint mOfferId;
        private uint mUserId;
        private Item mItem;
        private int mOfferState;
        private int mItemType;
        private int mAskingPrice;
        private int mTotalPrice;
        private double mTimestamp;
        private int mLimitedNumber;
        private int mLimitedStack;


        public uint Id 
        {
            get 
            {
                return mOfferId;
            }
        }

        public uint UserId
        {
            get
            {
                return mUserId;
            }
        }

        public Item Item
        {
            get
            {
                return mItem;
            }
        }

        public uint DefinitionId
        {
            get
            {
                return Definition.Id;
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
                return Definition.SpriteId;
            }
        }

        public string ExtraData
        {
            get
            {
                return mItem.Flags;
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
                return mItem.Definition;
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

        public MarketplaceOffers(uint OfferID, uint UserID, uint ItemID, int State, int AskingPrice, int TotalPrice, int ItemType, double Timestamp, int LimitedNumber, int LimitedStack)
        {
            mOfferId = OfferID;
            mUserId = UserID;

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                MySqlClient.SetParameter("id", ItemID);
                DataRow ItemRow = MySqlClient.ExecuteQueryRow("SELECT * FROM items WHERE id = @id LIMIT 1");

                mItem = ItemFactory.CreateFromDatabaseRow(ItemRow);
            }

            mOfferState = State;
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

                    return Sprite;

                case 2:

                    return ExtraData != string.Empty ? uint.Parse(ExtraData) : Sprite;
            }
        }

        public int CountOffers()
        {
            int Count = 0;
            switch (mItemType)
            {
                default:
                case 1:
                    Count = CatalogManager.MarketplaceOffers.Count(c => c.Value.State == 1 && c.Value.Sprite == Sprite);
                    return Count;

                case 2:

                    Count = CatalogManager.MarketplaceOffers.Count(c => c.Value.State == 1 && c.Value.Sprite == Sprite && c.Value.ExtraData == ExtraData);
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
                        if (CatalogManager.MarketplaceAvarages.ContainsKey(Sprite))
                        {
                            foreach (MarketplaceAvarage Item in CatalogManager.MarketplaceAvarages[Sprite])
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
                        if (CatalogManager.MarketplaceAvarages.ContainsKey(Sprite))
                        {
                            foreach (MarketplaceAvarage Item in CatalogManager.MarketplaceAvarages[Sprite])
                            {
                                TimeSpan SoldTime = DateTime.Now - Item.SoldTimeStamp;

                                if (Item.ExtraData == ExtraData && SoldTime.TotalDays <= 1)
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
                MySqlClient.SetParameter("offerid", mOfferId);
                MySqlClient.SetParameter("state", mOfferState);
                MySqlClient.ExecuteNonQuery("UPDATE catalog_marketplace_offers SET state = @state WHERE offer_id = @offerid LIMIT 1");
            }
        }
    }
}