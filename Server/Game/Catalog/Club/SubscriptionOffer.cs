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
        private ClubSubscriptionLevel mOffertedLevel;
        private List<uint> mUserIds;
        private double mDoubleTimestamp;
        private CatalogClubOffer mClubOffer;
        private int mPrice;

        public uint Id 
        {
            get
            {
                return mId;
            }
        }
        public int PriceDiscountPercentage
        {
            get
            {
                return mDiscountPercentage;
            }
        }
        public ClubSubscriptionLevel OffertedLevel 
        {
            get
            {
                return mOffertedLevel;
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
        public CatalogClubOffer ClubSubscriptionOffer
        {
            get
            {
                return mClubOffer;
            }
        }
        public int Price
        {
            get
            {
                return mPrice;
            }
        }
        public SubscriptionOffer(uint Id, int Discount, ClubSubscriptionLevel OffertedLevel,  List<uint> UserIds,
            double Timestamp)
        {
            mId = Id;
            mDiscountPercentage = Discount;
            mOffertedLevel = OffertedLevel;
            mDoubleTimestamp = Timestamp;
            
            mClubOffer = null;
            mUserIds = new List<uint>();
            mUserIds.AddRange(UserIds);
        }

        public int GetDiscountPrice()
        {
            double Double = mClubOffer.Price / 100.0;
            mPrice = mClubOffer.Price - Convert.ToInt32(Math.Ceiling(Double * mDiscountPercentage));
            return mPrice;
        }

        public void SetClubSubscriptionOffer(CatalogClubOffer CatalogClubSubOffer)
        {
            mClubOffer = CatalogClubSubOffer;
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
