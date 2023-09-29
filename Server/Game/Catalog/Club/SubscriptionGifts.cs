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
        private uint mDefinitionId;
        private string mItemName;
        private string mPresetFlags;
        private int mDaysNeed;
        private bool mIsVip;

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
        public uint DefinitionId
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

        public ItemDefinition Definition
        {
            get
            {
                return ItemDefinitionManager.GetDefinition(mDefinitionId);
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

        public SubscriptionGifts(uint Id, uint DefinitionId, string ItemName, string PresetFlags, int DaysNeed, bool IsVip)
        {
            mId = Id;
            mDefinitionId = DefinitionId;
            mItemName = ItemName;
            mPresetFlags = PresetFlags;
            mDaysNeed = DaysNeed;
            mIsVip = IsVip;
        }
    }
}
