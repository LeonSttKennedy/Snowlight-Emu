using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Snowlight.Util;

namespace Snowlight.Game.Misc
{
    public class HolidayGift
    {
        private uint mId;

        private List<uint> mDefinitionIds;
        private List<int> mItemsAmount;

        private int mCreditsAmount;
        private int mActivityPointsAmount;
        private SeasonalCurrencyList mActivityPointsType;

        private string mBadgeCode;

        private DateTime mDeliveryDate;
        private string mGiftMessage;

        private List<uint> mRequiredGiftsToUnlock;

        private string mUserAlertMessage;

        public uint Id
        {
            get 
            {
                return mId; 
            }
        }

        public List<uint> DefinitionIds
        {
            get
            {
                return mDefinitionIds;
            }
        }

        public List<int> ItemsAmount
        {
            get
            {
                return mItemsAmount;
            }
        }

        public int CreditsAmount
        {
            get
            {
                return mCreditsAmount;
            }
        }

        public int ActivityPointsAmount
        {
            get
            {
                return mActivityPointsAmount;
            }
        }

        public SeasonalCurrencyList ActivityPointsType
        {
            get
            {
                return mActivityPointsType;
            }
        }

        public string BadgeCode
        {
            get
            {
                return mBadgeCode;
            }
        }

        public DateTime DeliveryDate
        {
            get
            {
                return mDeliveryDate;
            }
        }

        public string GiftMessage
        {
            get
            {
                return mGiftMessage;
            }
        }

        public ReadOnlyCollection<uint> RequiredGiftsToUnlock
        {
            get
            {
                lock (mRequiredGiftsToUnlock)
                {
                    List<uint> Copy = new List<uint>();
                    Copy.AddRange(mRequiredGiftsToUnlock);
                    return Copy.AsReadOnly();
                }
            }
        }

        public string UserAlertMessage
        {
            get
            {
                return mUserAlertMessage;
            }
        }

        public HolidayGift(uint Id, string DefinitionId, string ItemsAmount, int CreditsAmount, int ActivityPointsAmount,
            SeasonalCurrencyList ActivityPointsType, string BadgeCode, string DeliveryDate, string GiftMessage,
            string RequiredGiftsToUnlock, string UserAlertMessage)
        {
            mId = Id;

            List<uint> Items = new List<uint>();
            string[] DefinitionIds = DefinitionId.Split('|');
            foreach (string Defs in DefinitionIds)
            {
                if (!string.IsNullOrEmpty(Defs) &&
                    uint.TryParse(Defs, out uint DefId) &&
                    !Items.Contains(DefId))
                {
                    Items.Add(DefId);
                }
            }

            mDefinitionIds = Items;

            List<int> ListAmounts = new List<int>();
            string[] ItemsAmounts = ItemsAmount.Split('|');
            foreach (string Amounts in ItemsAmounts)
            {
                if (!string.IsNullOrEmpty(Amounts) &&
                    int.TryParse(Amounts, out int ItemAmount) &&
                    !ListAmounts.Contains(ItemAmount))
                {
                    ListAmounts.Add(ItemAmount);
                }
            }
            mItemsAmount = ListAmounts;

            mCreditsAmount = CreditsAmount;
            mActivityPointsAmount = ActivityPointsAmount;
            mActivityPointsType = ActivityPointsType;

            mBadgeCode = BadgeCode;

            string[] Date = DeliveryDate.Split('-');
            int Day = int.Parse(Date[1]);
            int Month = int.Parse(Date[0]);

            mDeliveryDate = new DateTime(DateTime.Now.Year, Month, Day);
            mGiftMessage = GiftMessage;

            mRequiredGiftsToUnlock = new List<uint>();

            mUserAlertMessage = UserAlertMessage;

            if (!string.IsNullOrEmpty(RequiredGiftsToUnlock))
            {
                foreach (string StringId in RequiredGiftsToUnlock.Split('|'))
                {
                    if (uint.TryParse(StringId, out uint _Id))
                    {
                        mRequiredGiftsToUnlock.Add(_Id);
                    }
                }
            }
        }
    }
}
