using Snowlight.Game.Pets;
using Snowlight.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Game.Items
{
    public class CatalogItem
    {
        private uint mId;
        private int mDefinitionId;
        private string mDisplayName;
        private int mCostCredits;
        private int mCostActivityPoints;
        private SeasonalCurrencyList mSeasonalCurrency;
        private int mAmount;
        private string mPresetFlags;
        private int mClubRestriction;
        private string mBadgeCode;

        private List<PetRaceData> mPetRaceData;
        private List<CatalogItem> mDealItems;

        public uint Id
        {
            get
            {
                return mId;
            }
        }
        public int DefinitionId
        {
            get
            {
                return mDefinitionId;
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
        public string DisplayName
        {
            get
            {
                return mDisplayName;
            }
        }

        public int CostCredits
        {
            get
            {
                return mCostCredits;
            }
        }

        public int CostActivityPoints
        {
            get
            {
                return mCostActivityPoints;
            }
        }

        public SeasonalCurrencyList SeasonalCurrency
        {
            get
            {
                return mSeasonalCurrency;
            }
        }
        public int Amount
        {
            get
            {
                return (mAmount > 1 ? mAmount : 1);
            }
        }

        public string PresetFlags
        {
            get
            {
                return mPresetFlags;
            }
        }

        public int ClubRestriction
        {
            get
            {
                return mClubRestriction;
            }
        }
        public bool IsDeal
        {
            get
            {
                return mDefinitionId == -1 && mDealItems.Count > 1;
            }
        }
        public string BadgeCode
        {
            get
            {
                return mBadgeCode;
            }
        }

        public List<PetRaceData> DealRaces
        {
            get
            {
                return mPetRaceData;
            }
        }

        public List<CatalogItem> DealItems
        {
            get
            {
                return mDealItems;
            }
        }

        public CatalogItem(uint Id, int BaseId, string Name, int CostCredits, int CostActivityPoints, 
            SeasonalCurrencyList SeasonalCurrency, int Amount, string PresetFlags, int ClubRestriction, 
            string BadgeCode)
        {
            mId = Id;
            mDefinitionId = BaseId;
            mDisplayName = Name;
            mCostCredits = CostCredits;
            mCostActivityPoints = CostActivityPoints;
            mSeasonalCurrency = SeasonalCurrency;
            mAmount = Amount;
            mPresetFlags = PresetFlags;
            mClubRestriction = ClubRestriction;
            mBadgeCode = BadgeCode;

            mPetRaceData = new List<PetRaceData>();
            mDealItems = new List<CatalogItem>();
        }
        
        public void AddPetRaceData(PetRaceData PetData)
        {
            mPetRaceData.Add(PetData);
        }
        
        public void AddItem(CatalogItem Item)
        {
            mDealItems.Add(Item);
        }

        public bool ShowPresetFlags()
        {
            bool ShowPresetFlags = true;

            if (IsDeal)
            {
                foreach (CatalogItem Item in mDealItems)
                {
                    if (Item.Definition.Behavior.Equals(ItemBehavior.Moodlight))
                    {
                        ShowPresetFlags = false;
                        break;
                    }
                }
            }
            else
            {
                ShowPresetFlags = Definition.Behavior != ItemBehavior.Moodlight;
            }

            return ShowPresetFlags;
        }

        public bool CanGift()
        {
            bool CanGift = true;

            if (IsDeal)
            {
                foreach (CatalogItem Item in mDealItems)
                {
                    if (!Item.Definition.AllowGift || Item.Definition.Type.Equals(ItemType.AvatarEffect))
                    {
                        CanGift = false;
                        break;
                    }
                }
            }
            else
            {
                CanGift = Definition.AllowGift;
            }


            return CanGift;
        }
    }
}
