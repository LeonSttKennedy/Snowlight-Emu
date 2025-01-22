using Snowlight.Game.Rights;
using Snowlight.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Game.Catalog
{
    public class SubscriptionOffer
    {
        private uint mId;
        private int mDiscountPercentage;
        private List<uint> mUserIds;
        private double mDoubleTimestamp;
        private uint mBaseOfferId;
        private bool mCatalogEnabled;
        private bool mBasicSubscriptionReminder;
        private bool mShowExtendNotification;
        private bool mOnlyForNeverBeenMember;

        public uint Id 
        {
            get
            {
                return mId;
            }
        }

        public ReadOnlyCollection<uint> UserIds
        {
            get
            {
                List<uint> Copy = new List<uint>();
                Copy.AddRange(mUserIds);
                return Copy.AsReadOnly();
            }
        }

        public double Timestamp 
        {
            get
            {
                return mDoubleTimestamp;
            }
        }

        public DateTime TimestampExpire
        {
            get
            {
                return UnixTimestamp.GetDateTimeFromUnixTimestamp(mDoubleTimestamp);
            }
        }

        public int Price
        {
            get
            {
                double Percentage = BaseOffer.Price / 100.0;
                return BaseOffer.Price - Convert.ToInt32(Math.Ceiling(Percentage * mDiscountPercentage));
            }
        }

        public CatalogClubOffer BaseOffer
        {
            get
            {
                return CatalogManager.ClubOffers.Values.Where(O => O.Id == mBaseOfferId).FirstOrDefault();
            }
        }
        public bool CatalogEnabled
        {
            get
            {
                return mCatalogEnabled;
            }
        }

        public bool BasicSubscriptionReminder
        {
            get
            {
                return mBasicSubscriptionReminder;
            }
        }

        public bool ShowExtendNotification
        {
            get
            {
                return mShowExtendNotification;
            }
        }


        public bool OnlyForNeverBeenMember
        {
            get
            {
                return mOnlyForNeverBeenMember;
            }
        }

        public bool Enabled
        {
            get
            {
                return UnixTimestamp.GetCurrent() < mDoubleTimestamp;
            }
        }

        public SubscriptionOffer(uint Id, int Discount, List<uint> UserIds, double Timestamp,
             uint OfferId, bool CatalogEnabled, bool BasicSubscriptionReminder,
             bool ShowExtendNotification, bool OnlyForNeverBeenMember)
        {
            mId = Id;
            mDiscountPercentage = Discount;
            mDoubleTimestamp = Timestamp;

            mUserIds = new List<uint>();
            mUserIds.AddRange(UserIds);

            mBaseOfferId = OfferId;
            mCatalogEnabled = CatalogEnabled;
            mBasicSubscriptionReminder = BasicSubscriptionReminder;
            mShowExtendNotification = ShowExtendNotification;
            mOnlyForNeverBeenMember = OnlyForNeverBeenMember;
        }

        public void UpdateUserIdList(SqlDatabaseClient MySqlClient, uint UserId)
        {
            if(mUserIds.Contains(UserId))
            {
                return;
            }

            mUserIds.Add(UserId);

            MySqlClient.SetParameter("offerid", mId);
            MySqlClient.SetParameter("userids", string.Join("|", mUserIds));
            MySqlClient.ExecuteNonQuery("UPDATE subscription_offers SET user_ids_list = @userids WHERE id = @offerid LIMIT 1");
        }
    }
}
