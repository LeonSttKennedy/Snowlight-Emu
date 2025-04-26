using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Snowlight.Storage;
using Snowlight.Game.Rooms;
using Snowlight.Game.Catalog;
using Snowlight.Game.Sessions;
using Snowlight.Game.Achievements;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Rights
{
    public enum ClubSubscriptionLevel
    {
        None = 0,
        BasicClub = 1,
        VipClub = 2
    }

    public class ClubSubscription
    {
        private uint mUserId;
        private ClubSubscriptionLevel mBaseLevel;
        private double mTimestampCreated;
        private double mTimestampExpire;
        private double mTimestampLastGiftPoint;
        private double mHcTime;
        private double mVipTime;
        private int mGiftPoints;
        private List<uint> mOneTimeGiftsRedeem;

        public uint UserId
        {
            get 
            {
                return mUserId; 
            }
        }
        public bool IsActive
        {
            get
            {
                return UnixTimestamp.GetCurrent() < mTimestampExpire;
            }
        }

        public ClubSubscriptionLevel SubscriptionLevel
        {
            get
            {
                return (IsActive ? mBaseLevel : ClubSubscriptionLevel.None);
            }
        }

        public DateTime DateTimeCreated
        {
            get
            {
                return UnixTimestamp.GetDateTimeFromUnixTimestamp(mTimestampCreated);
            }
        }

        public DateTime ExpireDateTime
        {
            get
            {
                return UnixTimestamp.GetDateTimeFromUnixTimestamp(mTimestampExpire);
            }
        }

        public double PastHcTime
        {
            get
            {
                double Time = mHcTime;

                double CorrectCount = UnixTimestamp.GetCurrent() >= mTimestampExpire ? mTimestampExpire : UnixTimestamp.GetCurrent();

                if (mBaseLevel == ClubSubscriptionLevel.BasicClub)
                {
                    Time += (CorrectCount - mTimestampCreated);
                }

                return Time;
            }
        }

        public double PastVipTime
        {
            get
            {
                double Time = mVipTime;

                double CorrectCount = UnixTimestamp.GetCurrent() >= mTimestampExpire ? mTimestampExpire : UnixTimestamp.GetCurrent();

                if (mBaseLevel == ClubSubscriptionLevel.VipClub)
                {
                    Time += (CorrectCount - mTimestampCreated);
                }

                return Time;
            }
        }

        public int PastHcTimeInDays
        {
            get
            {
                return (int)(PastHcTime / 86400);
            }
        }

        public int PastVipTimeInDays
        {
            get
            {
                return (int)(PastVipTime / 86400);
            }
        }

        public double TimeLeft
        {
            get
            {
                return (mTimestampExpire - UnixTimestamp.GetCurrent());
            }
        }

        public int DaysLeft
        {
            get
            {
                return (int)Math.Ceiling(TimeLeft / 86400.0);
            }
        }
        public double TimestampCreated
        {
            get
            {
                return mTimestampCreated;
            }
        }
        public double TimestampExpire
        {
            get
            {
                return mTimestampExpire;
            }
        }

        #region CLUB GIFTS SYSTEM
        public DateTime LastPointDelivery
        {
            get
            {
                return UnixTimestamp.GetDateTimeFromUnixTimestamp(mTimestampLastGiftPoint);
            }
        }

        public DateTime NextGiftPointDateTime
        {
            get
            {
                return UnixTimestamp.GetDateTimeFromUnixTimestamp(mTimestampLastGiftPoint).AddDays(31);
            }
        }

        public bool GivePoints
        {
            get
            {
                int Compare = DateTime.Compare(DateTime.Now, NextGiftPointDateTime);

                return IsActive && Compare >= 0;
            }
        }

        public int NextGiftPoint
        {
            get
            {
                TimeSpan NextPoint = NextGiftPointDateTime - DateTime.Now;
                return NextPoint.Days;
            }
        }

        public int GiftPoints
        {
            get
            {
                return mGiftPoints;
            }

            set
            {
                mGiftPoints = value;
            }
        }

        public ReadOnlyCollection<uint> OneTimeGiftsRedeem
        {
            get
            {
                List<uint> Copy = new List<uint>();
                Copy.AddRange(mOneTimeGiftsRedeem);
                return Copy.AsReadOnly();
            }
        }
        #endregion

        public ClubSubscription(uint UserId, ClubSubscriptionLevel BaseLevel, double TimestampCreated, double TimestampExpired,
            double TimestampLastGiftPoint, double HcTime, double VipTime, int GiftPoints, List<uint> OneTimeGiftRedeem)
        {
            mUserId = UserId;
            mBaseLevel = BaseLevel;
            mTimestampCreated = TimestampCreated;
            mTimestampExpire = TimestampExpired;
            mTimestampLastGiftPoint = TimestampLastGiftPoint;
            mHcTime = HcTime;
            mVipTime = VipTime;
            mGiftPoints = GiftPoints;
            mOneTimeGiftsRedeem = OneTimeGiftRedeem;

            // If user has to win points give it to him
            AddGiftPoints(true);

            // Clear catalog cache for user
            CatalogManager.ClearCacheGroup(mUserId);

            if (!IsActive)
            {
                if (mBaseLevel != ClubSubscriptionLevel.None)
                {
                    Expire();
                }
            }
        }

        public void Expire()
        {
            mHcTime = PastHcTime;
            mVipTime = PastVipTime;
            mBaseLevel = ClubSubscriptionLevel.None;
            mTimestampCreated = 0;
            mTimestampExpire = 0;

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                MySqlClient.SetParameter("userid", mUserId);
                MySqlClient.SetParameter("hctime", mHcTime);
                MySqlClient.SetParameter("viptime", mVipTime);
                MySqlClient.ExecuteNonQuery("UPDATE user_subscriptions SET subscription_level = '0', timestamp_created = 0, timestamp_expire = 0, timestamp_last_gift_point = 0, past_time_hc = @hctime, past_time_vip = @viptime WHERE user_id = @userid LIMIT 1");
            }
        }

        public void AddOrExtend(int Level, double ExtensionTime)
        {
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                if (!IsActive)
                {
                    mTimestampCreated = UnixTimestamp.GetCurrent();
                    mTimestampExpire = UnixTimestamp.GetCurrent();
                    mTimestampLastGiftPoint = UnixTimestamp.GetCurrent();
                }

                bool IsUpgrade = IsActive && (SubscriptionLevel.Equals(ClubSubscriptionLevel.BasicClub) 
                    && (ClubSubscriptionLevel)Level == ClubSubscriptionLevel.VipClub); 

                mTimestampExpire += ExtensionTime;
                mBaseLevel = (ClubSubscriptionLevel)Level;

                MySqlClient.SetParameter("userid", mUserId);
                bool CreateNewRecord = (MySqlClient.ExecuteScalar("SELECT null FROM user_subscriptions WHERE user_id = @userid LIMIT 1") == null);

                MySqlClient.SetParameter("userid", mUserId);
                MySqlClient.SetParameter("expirestamp", mTimestampExpire);
                MySqlClient.SetParameter("level", ((int)mBaseLevel).ToString());

                if (CreateNewRecord)
                {
                    mGiftPoints = 0; // New club users starts with 0 club gift points :)

                    mTimestampLastGiftPoint = UnixTimestamp.GetCurrent();

                    MySqlClient.SetParameter("lastupdate", mTimestampLastGiftPoint);
                    MySqlClient.SetParameter("giftpoints", mGiftPoints);
                    MySqlClient.SetParameter("createstamp", UnixTimestamp.GetCurrent());
                    MySqlClient.ExecuteNonQuery("INSERT INTO user_subscriptions (user_id,subscription_level,timestamp_created,timestamp_expire,timestamp_last_gift_point,gift_points) VALUES (@userid,@level,@createstamp,@expirestamp,@lastupdate,@giftpoints)");
                }
                else
                {
                    mGiftPoints += IsUpgrade ? 1 : (GivePoints ? (int)SubscriptionLevel : 0);
                    MySqlClient.SetParameter("lastupdate", mTimestampLastGiftPoint);
                    MySqlClient.SetParameter("createstamp", mTimestampCreated);
                    MySqlClient.SetParameter("giftpoints", mGiftPoints);
                    MySqlClient.ExecuteNonQuery("UPDATE user_subscriptions SET subscription_level = @level, timestamp_created = @createstamp, timestamp_expire = @expirestamp, timestamp_last_gift_point = @lastupdate, gift_points = @giftpoints WHERE user_id = @userid LIMIT 1");
                }
            }
        }

        public bool HasUserEverBeenMember()
        {
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                MySqlClient.SetParameter("userid", mUserId);
                return (MySqlClient.ExecuteScalar("SELECT null FROM user_subscriptions WHERE user_id = @userid LIMIT 1") != null);
            }
        }

        #region Club Gifts

        #region Points

        #region Automatically addiction
        public void AddGiftPoints(bool AddPoints = false)
        {
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                if (AddPoints)
                {
                    if (GivePoints)
                    {
                        TimeSpan TimeSpan = DateTime.Now - LastPointDelivery;
                        int TotalMonths = (int)TimeSpan.TotalDays / 31;

                        mGiftPoints += TotalMonths > 1 ? TotalMonths * (int)mBaseLevel : (int)mBaseLevel;

                        mTimestampLastGiftPoint = IsActive ? UnixTimestamp.ConvertToUnixTimestamp(LastPointDelivery.AddDays(31 * TotalMonths)) : 0;

                        if (IsActive && mGiftPoints > 0)
                        {
                            Session Session = SessionManager.GetSessionByCharacterId(mUserId);
                            if (Session != null)
                            {
                                Session.SendData(ClubGiftReadyComposer.Compose(mGiftPoints));
                            }
                        }

                        MySqlClient.SetParameter("userid", mUserId);
                        MySqlClient.SetParameter("giftpoints", mGiftPoints);
                        MySqlClient.SetParameter("lastupdate", mTimestampLastGiftPoint);
                        MySqlClient.ExecuteNonQuery("UPDATE user_subscriptions SET gift_points = @giftpoints, timestamp_last_gift_point = @lastupdate WHERE user_id = @userid LIMIT 1");
                    }
                }
                else
                {
                    mGiftPoints--;
                    MySqlClient.SetParameter("userid", mUserId);
                    MySqlClient.ExecuteNonQuery("UPDATE user_subscriptions SET gift_points = gift_points - 1 WHERE user_id = @userid LIMIT 1");
                }
            }
        }
        #endregion

        #region Manualy addiction
        public void GiveGiftPoints(int Quantity)
        {
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                MySqlClient.SetParameter("userid", mUserId);
                bool CreateNewRecord = (MySqlClient.ExecuteScalar("SELECT null FROM user_subscriptions WHERE user_id = @userid LIMIT 1") == null);

                mGiftPoints += Quantity;

                MySqlClient.SetParameter("userid", mUserId);
                MySqlClient.SetParameter("giftpoints", mGiftPoints);
                if(CreateNewRecord)
                {
                    MySqlClient.SetParameter("createstamp", mTimestampCreated);
                    MySqlClient.SetParameter("expirestamp", mTimestampExpire);
                    MySqlClient.SetParameter("lastupdate", mTimestampLastGiftPoint);
                    MySqlClient.SetParameter("level", ((int)mBaseLevel).ToString());

                    MySqlClient.ExecuteNonQuery("INSERT INTO user_subscriptions (user_id,subscription_level,timestamp_created,timestamp_expire,timestamp_last_gift_point,gift_points) VALUES (@userid,@level,@createstamp,@expirestamp,@lastupdate,@giftpoints)");
                }
                else
                {
                    MySqlClient.ExecuteNonQuery("UPDATE user_subscriptions SET gift_points = @giftpoints WHERE user_id = @userid LIMIT 1");
                }
            }
        }
        #endregion

        #endregion

        #region One Time Only Redeem

        public void AddGiftRedeem(SqlDatabaseClient MySqlClient, uint DefinitionId)
        {
            if (WasGiftRedeemed(DefinitionId))
            {
                return;
            }

            mOneTimeGiftsRedeem.Add(DefinitionId);

            MySqlClient.SetParameter("userid", mUserId);
            MySqlClient.SetParameter("onetimelist", string.Join("|", mOneTimeGiftsRedeem));
            MySqlClient.ExecuteNonQuery("UPDATE user_subscriptions SET one_time_gifts_redeem = @onetimelist WHERE user_id = @userid LIMIT 1");
        }

        public bool WasGiftRedeemed(uint DefinitionId)
        {
            return mOneTimeGiftsRedeem.Contains(DefinitionId);
        }

        #endregion
        
        #endregion

        #region Club Badges
        public bool NeedsUpdateProgress()
        {
            Session Session = SessionManager.GetSessionByCharacterId(mUserId);

            if(Session == null) // ??
            {
                return false;
            }

            string HC_Badge = "ACH_BasicClub";
            string VIP_Badge = "ACH_VipClub";

            // USER DATA
            UserAchievement HC_DATA = Session.AchievementCache.GetAchievementData(HC_Badge);
            UserAchievement VIP_DATA = Session.AchievementCache.GetAchievementData(VIP_Badge);
            
            int HC_Progress = HC_DATA != null ? HC_DATA.Progress : 0;
            int VIP_Progress = VIP_DATA != null ? VIP_DATA.Progress : 0;
            
            bool ToReturn = false;
            switch (mBaseLevel)
            {
                case ClubSubscriptionLevel.VipClub: // Vip Sub unlock HC and VIP badge

                    if (IsActive)
                    {
                        if (PastVipTimeInDays == 0) // First Unlock
                        {
                            ToReturn = true;
                        }

                        if (PastVipTimeInDays > 0)
                        {
                            if ((Math.Floor((double)(PastVipTimeInDays / 31)) - VIP_Progress) > 0)
                            {
                                ToReturn = true;
                            }
                        }
                    }

                    return ToReturn;

                case ClubSubscriptionLevel.BasicClub:

                    if (IsActive)
                    {
                        if (PastHcTimeInDays == 0) // First Unlock
                        {
                            ToReturn = true;
                        }

                        if (PastHcTimeInDays > 0)
                        {
                            if ((Math.Floor((double)(PastHcTimeInDays / 31)) - HC_Progress) > 0)
                            {
                                ToReturn = true;
                            }
                        }
                    }

                    return ToReturn;

                default:
                case ClubSubscriptionLevel.None:

                    return ToReturn;
            }
        }

        public void UpdateUserBadge()
        {
            Session Session = SessionManager.GetSessionByCharacterId(mUserId);

            string HC_Badge = "ACH_BasicClub";
            string VIP_Badge = "ACH_VipClub";

            if (Session != null)
            {
                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    // Disable any badges if they still aren't valid
                    if (SubscriptionLevel < ClubSubscriptionLevel.VipClub)
                    {
                        Session.BadgeCache.DisableSubscriptionBadge(VIP_Badge);
                    }

                    if (SubscriptionLevel < ClubSubscriptionLevel.BasicClub)
                    {
                        Session.BadgeCache.DisableSubscriptionBadge(HC_Badge);
                    }

                    if (NeedsUpdateProgress())
                    {
                        UserAchievement HC_DATA = Session.AchievementCache.GetAchievementData(HC_Badge);
                        UserAchievement VIP_DATA = Session.AchievementCache.GetAchievementData(VIP_Badge);

                        // USER DATA
                        int HC_CachedProgress = HC_DATA != null ? HC_DATA.Progress : 0;
                        int VIP_CachedProgress = VIP_DATA != null ? VIP_DATA.Progress : 0;

                        int IncreaseVIP = mBaseLevel == ClubSubscriptionLevel.VipClub ? 
                            (PastVipTimeInDays / 31) - VIP_CachedProgress : 0;
                        
                        int IncreaseHC = IncreaseVIP > 0 ? IncreaseVIP : 
                            mBaseLevel == ClubSubscriptionLevel.BasicClub ? (PastHcTimeInDays / 31) - HC_CachedProgress : 0;

                        if (PastHcTime < 1 && mBaseLevel == ClubSubscriptionLevel.BasicClub)
                        {
                            IncreaseHC++;
                        }

                        if (PastVipTime < 1 && mBaseLevel == ClubSubscriptionLevel.VipClub)
                        {
                            IncreaseVIP++;
                            IncreaseHC = IncreaseVIP;
                        }

                        if (IncreaseHC > 0)
                        {
                            AchievementManager.ProgressUserAchievement(MySqlClient, Session, HC_Badge, IncreaseHC);
                        }

                        if (IncreaseVIP > 0)
                        {
                            AchievementManager.ProgressUserAchievement(MySqlClient, Session, VIP_Badge, IncreaseVIP);
                        }

                        if (IncreaseHC > 0 || IncreaseVIP > 0)
                        {
                            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

                            if (Instance != null)
                            {
                                Instance.BroadcastMessage(RoomUserBadgesComposer.Compose(Session.CharacterId,
                                    Session.BadgeCache.EquippedBadges));
                            }

                            Session.BadgeCache.ReloadCache(MySqlClient, Session.AchievementCache);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
