using Snowlight.Game.Items;
using Snowlight.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Snowlight.Game.Catalog
{
    public class MarketplaceAvarage
    {
        private uint mId;
        private uint mSprite;
        private string mExtraData;
        private int mTotalPrice;
        private int mType;
        private DateTime mSoldTimestamp;

        public uint Id
        {
            get
            {
                return mId;
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
        public int TotalPrice
        {
            get
            {
                return mTotalPrice;
            }
        }
        public int Type
        {
            get
            {
                return mType;
            }
        }
        public DateTime SoldTimeStamp
        {
            get
            {
                return mSoldTimestamp;
            }
        }

        public MarketplaceAvarage(uint Id, uint Sprite, string ExtraData, int TotalPrice, int ItemType, DateTime soldTimeStamp)
        {
            mId = Id;
            mSprite = Sprite;
            mExtraData = ExtraData;
            mTotalPrice = TotalPrice;
            mType = ItemType;
            mSoldTimestamp = soldTimeStamp;
        }
    }
}