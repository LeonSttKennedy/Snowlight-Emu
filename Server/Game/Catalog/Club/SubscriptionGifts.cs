using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snowlight.Game.Items;
using Snowlight.Storage;

namespace Snowlight.Game.Catalog
{
    public class SubscriptionGifts
    {
        private uint mId;
        private int mDefinitionId;
        private string mItemName;
        private string mPresetFlags;
        private int mAmount;
        private int mDaysNeed;
        private bool mIsVip;

        private List<SubscriptionGifts> mDealGifts;

        public uint Id
        {
            get
            {
                return mId;
            }

            set
            {
                mId = value;
            }
        }
        public int DefinitionId
        {
            get
            {
                return mDefinitionId;
            }

            set
            {
                mDefinitionId = value;
            }
        }

        public string ItemName
        {
            get
            {
                return mItemName;
            }

            set 
            {
                mItemName = value;
            }
        }

        public bool ShowPresetFlags
        {
            get
            {
                return mPresetFlags.Length > 0;
            }
        }   

        public string PresetFlags
        {
            get
            {
                return mPresetFlags;
            }

            set
            {
                mPresetFlags = value;
            }
        }

        public int Amount
        {
            get
            {
                return mAmount;
            }

            set
            {
                mAmount = value;
            }
        }

        public ItemDefinition Definition
        {
            get
            {
                uint DefId = uint.Parse(mDefinitionId == -1 ? "0" : mDefinitionId.ToString());
                return ItemDefinitionManager.GetDefinition(DefId);
            }
        }

        public int DaysNeed
        {
            get
            {
                return mDaysNeed;
            }

            set
            {
                mDaysNeed = value;
            }
        }

        public bool IsVip
        {
            get
            {
                return mIsVip;
            }

            set
            {
                mIsVip = value;
            }
        }

        public List<SubscriptionGifts> DealItems
        {
            get
            {
                return mDealGifts;
            }
        }

        public bool IsDeal
        {
            get
            {
                return mDefinitionId == -1 && mDealGifts.Count > 1;
            }
        }

        public SubscriptionGifts(uint Id, int DefinitionId, string ItemName, string PresetFlags, int Amount, int DaysNeed, bool IsVip)
        {
            mId = Id;
            mDefinitionId = DefinitionId;
            mItemName = ItemName;
            mPresetFlags = PresetFlags;
            mAmount = Amount;
            mDaysNeed = DaysNeed;
            mIsVip = IsVip;

            mDealGifts = new List<SubscriptionGifts>();
        }

        public void AddItem(SubscriptionGifts Gift)
        {
            mDealGifts.Add(Gift);
        }
    }
}
