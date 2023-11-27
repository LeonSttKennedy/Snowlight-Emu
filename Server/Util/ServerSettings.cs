using Snowlight.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.ModelBinding;

namespace Snowlight.Util
{
    public enum MotdType
    {
        NotificationMessageComposer = 0,
        MessageOfTheDayComposer = 1
    }

    public enum InfobusStatus
    {
        Closed = 0,
        Open = 1
    }

    public static class ServerSettings
    {
        #region Activity Points Settings
        private static bool mActivityPointsEnabled;
        private static SeasonalCurrencyList mActivityPointsType;
        private static bool mMoreActivityPointsForVipUsers;
        private static int mActivityPointsInterval;
        private static int mActivityPointsCreditsAmount;
        private static int mMoreActivityPointsCreditsAmount;
        private static int mActivityPointsAmount;
        private static int mMoreActivityPointsAmount;

        public static bool ActivityPointsEnabled
        {
            get
            {
                return mActivityPointsEnabled;
            }

            set
            {
                mActivityPointsEnabled = value;
            }
        }
        public static SeasonalCurrencyList ActivityPointsType
        {
            get
            {
                return mActivityPointsType;
            }

            set
            {
                mActivityPointsType = value;
            }
        }
        public static bool MoreActivityPointsForVipUsers
        {
            get
            {
                return mMoreActivityPointsForVipUsers;
            }

            set
            {
                mMoreActivityPointsForVipUsers = value;
            }
        }
        public static int ActivityPointsInterval
        {
            get
            {
                return mActivityPointsInterval;
            }

            set
            {
                mActivityPointsInterval = value;
            }
        }
        public static int ActivityPointsCreditsAmount
        {
            get
            {
                return mActivityPointsCreditsAmount;
            }

            set
            {
                mActivityPointsCreditsAmount = value;
            }
        }
        public static int MoreActivityPointsCreditsAmount
        {
            get
            {
                return mMoreActivityPointsCreditsAmount;
            }

            set
            {
                mMoreActivityPointsCreditsAmount = value;
            }
        }
        public static int ActivityPointsAmount
        {
            get
            {
                return mActivityPointsAmount;
            }

            set
            {
                mActivityPointsAmount = value;
            }
        }
        public static int MoreActivityPointsAmount
        {
            get
            {
                return mMoreActivityPointsAmount;
            }

            set
            {
                mMoreActivityPointsAmount = value;
            }
        }
        #endregion

        #region Daily Reward Settings
        private static bool mDailyRewardEnabled;
        private static int mDailyRewardWaitTime;
        private static SeasonalCurrencyList mDailyActivityPointsType;
        private static int mDailyRewardActivityPointAmount;
        private static int mDailyRewardCreditsAmount;

        public static bool DailyRewardEnabled
        {
            get
            {
                return mDailyRewardEnabled;
            }

            set
            {
                mDailyRewardEnabled = value;
            }
        }
        public static int DailyRewardWaitTime
        {
            get
            {
                return mDailyRewardWaitTime;
            }
            set
            {
                mDailyRewardWaitTime = value;
            }
        }
        public static SeasonalCurrencyList DailyActivityPointsType
        {
            get
            {
                return mDailyActivityPointsType;
            }

            set
            {
                mDailyActivityPointsType = value;
            }
        }
        public static int DailyRewardActivityPointAmount
        {
            get 
            {
                return mDailyRewardActivityPointAmount; 
            }
            
            set 
            {
                mDailyRewardActivityPointAmount =  value; 
            }
        }
        public static int DailyRewardCreditsAmount
        {
            get
            {
                return mDailyRewardCreditsAmount;
            }

            set
            {
                mDailyRewardCreditsAmount = value;
            }
        }
        #endregion

        #region Gifting Items Settings
        private static bool mGiftingSystemEnabled;
        private static bool mNewGiftingSystem;
        private static int mGiftingSystemPrice;
        private static List<uint> mGiftingSystemSpriteIds;
        private static int mGiftingSystemBoxCount;
        private static int mGiftingSystemRibbonCount;

        public static bool GiftingSystemEnabled
        {
            get 
            { 
                return mGiftingSystemEnabled; 
            }

            set 
            { 
                mGiftingSystemEnabled = value; 
            }
        }
        public static bool NewGiftingSystem
        {
            get
            {
                return mNewGiftingSystem;
            }

            set
            {
                mNewGiftingSystem = value;
            }
        }
        public static int GiftingSystemPrice
        {
            get
            {
                return mGiftingSystemPrice;
            }

            set
            {
                mGiftingSystemPrice = value;
            }
        }
        public static List<uint> GiftingSystemSpriteIds
        {
            get
            {
                return mGiftingSystemSpriteIds;
            }

            set
            {
                mGiftingSystemSpriteIds = value;
            }
        }
        public static int GiftingSystemBoxCount
        {
            get
            {
                return mGiftingSystemBoxCount;
            }

            set
            {
                mGiftingSystemBoxCount = value;
            }
        }
        public static int GiftingSystemRibbonCount
        {
            get
            {
                return mGiftingSystemRibbonCount;
            }

            set
            {
                mGiftingSystemRibbonCount = value;
            }
        }
        #endregion

        #region Infobus Settings
        private static InfobusStatus mInfobusStatus;

        public static InfobusStatus InfobusStatus
        {
            get
            {
                return mInfobusStatus;
            }
            set
            {
                mInfobusStatus = value;
            }
        }
        #endregion

        #region Badge When Login Reward Settings
        private static bool mLoginBadgeEnabled;
        private static string mLoginBadgeCode;

        public static bool LoginBadgeEnabled
        {
            get
            {
                return mLoginBadgeEnabled;
            }

            set
            {
                mLoginBadgeEnabled = value;
            }
        }
        public static string LoginBadgeCode
        {
            get
            {
                return mLoginBadgeCode;
            }

            set
            {
                mLoginBadgeCode = value;
            }
        }
        #endregion

        #region Marketplace Settings
        private static bool mMarketplaceEnabled;
        private static int mMarketplaceTax;
        private static bool mMarketplaceTokensBuyEnabled;
        private static int mMarketplaceTokensPrice;
        private static int mMarketplacePremiumTokens;
        private static int mMarketplaceDefaultTokens;
        private static int mMarketplaceMinPrice;
        private static int mMarketplaceMaxPrice;
        private static int mMarketplaceOfferTime;
        private static int mMarketplaceAverageOfferDays;

        public static bool MarketplaceEnabled
        {
            get
            {
                return mMarketplaceEnabled;
            }

            set
            {
                mMarketplaceEnabled = value;
            }
        }
        public static int MarketplaceTax
        {
            get
            {
                return mMarketplaceTax;
            }

            set
            {
                mMarketplaceTax = value;
            }
        }
        public static bool MarketplaceTokensBuyEnabled
        {
            get
            {
                return mMarketplaceTokensBuyEnabled;
            }

            set
            {
                mMarketplaceTokensBuyEnabled = value;
            }
        }
        public static int MarketplaceTokensPrice
        {
            get
            {
                return mMarketplaceTokensPrice;
            }

            set
            {
                mMarketplaceTokensPrice = value;
            }
        }
        public static int MarketplacePremiumTokens
        {
            get
            {
                return mMarketplacePremiumTokens;
            }

            set
            {
                mMarketplacePremiumTokens = value;
            }
        }
        public static int MarketplaceNormalTokens
        {
            get
            {
                return mMarketplaceDefaultTokens;
            }

            set
            {
                mMarketplaceDefaultTokens = value;
            }
        }
        public static int MarketplaceMinPrice
        {
            get
            {
                return mMarketplaceMinPrice;
            }

            set
            {
                mMarketplaceMinPrice = value;
            }
        }
        public static int MarketplaceMaxPrice
        {
            get
            {
                return mMarketplaceMaxPrice;
            }

            set
            {
                mMarketplaceMaxPrice = value;
            }
        }
        public static int MarketplaceOfferTotalHours
        {
            get
            {
                return mMarketplaceOfferTime;
            }

            set
            {
                mMarketplaceOfferTime = value;
            }
        }
        public static int MarketplaceAvarageDays
        {
            get
            {
                return mMarketplaceAverageOfferDays;
            }

            set
            {
                mMarketplaceAverageOfferDays = value;
            }
        }
        #endregion

        #region User Limits Settings
        private static int mMaxFavoritesPerUser;
        private static int mMaxFurniPerRoom;
        private static int mMaxFurniStacking;
        private static int mMaxPetsPerRoom;
        private static int mMaxRoomsPerUser;

        private static int mNormalUserFriendListSize;
        private static int mHcUserFriendListSize;
        private static int mVipUserFriendListSize;

        private static int mNameChangeWaitDays;

        public static int MaxFavoritesPerUser
        {
            get
            {
                return mMaxFavoritesPerUser;
            }

            set
            {
                mMaxFavoritesPerUser = value;
            }
        }
        public static int MaxFurniPerRoom
        {
            get
            {
                return mMaxFurniPerRoom;
            }

            set
            {
                mMaxFurniPerRoom = value;
            }
        }
        public static int MaxFurniStacking
        {
            get
            {
                return mMaxFurniStacking;
            }

            set
            {
                mMaxFurniStacking = value;
            }
        }
        public static int MaxPetsPerRoom
        {
            get
            {
                return mMaxPetsPerRoom;
            }

            set
            {
                mMaxPetsPerRoom = value;
            }
        }
        public static int MaxRoomsPerUser
        {
            get
            {
                return mMaxRoomsPerUser;
            }

            set
            {
                mMaxRoomsPerUser = value;
            }
        }
        public static int NormalUserFriendListSize
        {
            get
            {
                return mNormalUserFriendListSize;
            }

            set
            {
                mNormalUserFriendListSize = value;
            }

        }
        public static int HcUserFriendListSize
        {
            get
            {
                return mHcUserFriendListSize;
            }

            set
            {
                mHcUserFriendListSize = value;
            }

        }
        public static int VipUserFriendListSize
        {
            get
            {
                return mVipUserFriendListSize;
            }

            set
            {
                mVipUserFriendListSize = value;
            }

        }

        public static int NameChangeWaitDays
        {
            get
            {
                return mNameChangeWaitDays;
            }

            set
            {
                mNameChangeWaitDays = value;
            }
        }
        #endregion

        #region Moderation Settings
        private static bool mModerationActionLogs;
        private static bool mModerationConsoleChatLogs;
        private static bool mModerationChatLogs;
        private static bool mModerationRoomLogs;

        public static bool ModerationActionLogs
        {
            get
            {
                return mModerationActionLogs;
            }

            set
            {
                mModerationActionLogs = value;
            }
        }
        public static bool ModerationConsoleChatLogs
        {
            get
            {
                return mModerationConsoleChatLogs;
            }

            set
            {
                mModerationConsoleChatLogs = value;
            }
        }
        public static bool ModerationChatLogs
        {
            get
            {
                return mModerationChatLogs;
            }

            set
            {
                mModerationChatLogs = value;
            }
        }
        public static bool ModerationRoomLogs
        {
            get
            {
                return mModerationRoomLogs;
            }

            set
            {
                mModerationRoomLogs = value;
            }
        }
        #endregion

        #region Message Of The Day Settings
        private static bool mMotdEnabled;
        private static MotdType mMotdType;
        private static List<string> mMotdText;

        public static bool MotdEnabled
        {
            get
            {
                return mMotdEnabled;
            }

            set
            {
                mMotdEnabled = value;
            }
        }
        public static MotdType MotdType
        {
            get
            {
                return mMotdType;
            }
            set
            {
                mMotdType = value;
            }
        }
        public static List<string> MotdText
        {
            get
            {
                return mMotdText;
            }

            set
            {
                mMotdText = value;
            }
        }
        #endregion

        #region Pet Settings
        private static bool mEnablePets;
        private static bool mPetScratchingAccountDaysOldEnabled;
        private static int mPetScratchingAccountDaysOld;

        public static bool PetsEnabled
        {
            get
            {
                return mEnablePets;
            }

            set
            {
                mEnablePets = value;
            }
        }
        public static bool PetScratchingAccountDaysOldEnabled
        {
            get
            {
                return mPetScratchingAccountDaysOldEnabled;
            }

            set
            {
                mPetScratchingAccountDaysOldEnabled = value;
            }
        }
        public static int PetScratchingAccountDaysOld
        {
            get
            {
                return mPetScratchingAccountDaysOld;
            }

            set
            {
                mPetScratchingAccountDaysOld = value;
            }
        }
        #endregion

        #region Word Filter Settings
        private static int mWordFilterMaximumCount;
        private static int mWordFilterTimeToMute;

        public static int WordFilterMaximumCount
        {
            get
            {
                return mWordFilterMaximumCount;
            }

            set
            {
                mWordFilterMaximumCount = value;
            }

        }
        public static int WordFilterTimeToMute
        {
            get
            {
                return mWordFilterTimeToMute;
            }

            set
            {
                mWordFilterTimeToMute = value;
            }

        }
        #endregion

        public static void Initialize(SqlDatabaseClient MySqlClient)
        {
            Output.WriteLine("Loading server settings in database...", OutputLevel.Informational);
            DataRow Row = MySqlClient.ExecuteQueryRow("SELECT * FROM server_settings LIMIT 1");

            InfobusStatus Infobus_Status = InfobusStatus.Closed;
            switch ((string)Row["infobus_status"])
            {
                case "closed":
                    Infobus_Status = InfobusStatus.Closed;
                    break;

                case "open":
                    Infobus_Status = InfobusStatus.Open;
                    break;
            }

            MotdType Motd_Type = MotdType.MessageOfTheDayComposer;
            switch((string)Row["motd_type"])
            {
                case "NotificationMessageComposer":
                    Motd_Type = MotdType.NotificationMessageComposer;
                    break;

                case "MessageOfTheDayComposer":
                    Motd_Type = MotdType.MessageOfTheDayComposer;
                    break;
            }

            List<string> Motd = new List<string>();

            foreach (string MotdString in Row["motd_text"].ToString().Replace("\\n", "\n").Split('|')) Motd.Add(MotdString);

            List<uint> GiftSpriteIds = new List<uint>();

            foreach (string SpriteId in Row["gifting_system_spriteids"].ToString().Split('|'))
            {
                if (uint.TryParse(SpriteId, out uint GiftSpriteId))
                {
                    if (!GiftSpriteIds.Contains(GiftSpriteId))
                    {
                        GiftSpriteIds.Add(GiftSpriteId);
                    }
                }
            }

            ActivityPointsEnabled = (Row["activitypoints_enabled"].ToString() == "1");
            ActivityPointsType = SeasonalCurrency.FromStringToEnum(Row["activitypoints_type"].ToString());
            MoreActivityPointsForVipUsers = (Row["more_activitypoints_for_vip_users"].ToString() == "1");
            ActivityPointsInterval = (int)Row["activitypoints_interval"];
            ActivityPointsCreditsAmount = (int)Row["activitypoints_credits_amount"];
            MoreActivityPointsCreditsAmount = (int)Row["more_activitypoints_credits_amount"];
            ActivityPointsAmount = (int)Row["activitypoints_amount"];
            MoreActivityPointsAmount = (int)Row["more_activitypoints_amount"];
            
            DailyRewardEnabled = (Row["daily_reward_enabled"].ToString() == "1");
            DailyRewardWaitTime = (int)Row["daily_reward_wait_time"];
            DailyActivityPointsType = SeasonalCurrency.FromStringToEnum(Row["daily_activitypoints_type"].ToString());
            DailyRewardActivityPointAmount = (int)Row["daily_activitypoints_amount"];
            DailyRewardCreditsAmount = (int)Row["daily_credits_amount"]; 
            
            GiftingSystemEnabled = (Row["gifting_system_enabled"].ToString() == "1");
            NewGiftingSystem = (Row["new_gifting_system"].ToString() == "1");
            GiftingSystemPrice = (int)Row["gifting_system_price"];
            GiftingSystemSpriteIds = GiftSpriteIds;
            GiftingSystemBoxCount = (int)Row["gifting_system_box_count"];
            GiftingSystemRibbonCount = (int)Row["gifting_system_ribbon_count"];
            
            InfobusStatus = Infobus_Status;
            
            LoginBadgeEnabled = (Row["login_badge_enabled"].ToString() == "1");
            LoginBadgeCode = (string)Row["login_badge_code"];
            
            MarketplaceEnabled = (Row["marketplace_enabled"].ToString() == "1");
            MarketplaceTax = (int)Row["marketplace_tax"];
            MarketplaceTokensBuyEnabled = (Row["marketplace_tokens_buy_enabled"].ToString() == "1");
            MarketplaceTokensPrice = (int)Row["marketplace_tokens_price"];
            MarketplacePremiumTokens = (int)Row["marketplace_premium_tokens"];
            MarketplaceNormalTokens = (int)Row["marketplace_default_tokens"];
            MarketplaceMinPrice = (int)Row["marketplace_min_price"];
            MarketplaceMaxPrice = (int)Row["marketplace_max_price"];
            MarketplaceOfferTotalHours = (int)Row["marketplace_offer_hours"];
            MarketplaceAvarageDays = (int)Row["marketplace_avarage_days"];
            
            MaxFavoritesPerUser = (int)Row["max_favorites_per_user"];
            MaxFurniPerRoom = (int)Row["max_furni_per_room"];
            MaxFurniStacking = (int)Row["max_furni_stacking"];
            MaxPetsPerRoom = (int)Row["max_pets_per_room"];
            MaxRoomsPerUser = (int)Row["max_rooms_per_user"];
            
            ModerationActionLogs = (Row["moderation_actionlogs_enabled"].ToString() == "1");
            ModerationChatLogs = (Row["moderation_chatlogs_enabled"].ToString() == "1");
            ModerationConsoleChatLogs = (Row["moderation_console_chatlogs_enabled"].ToString() == "1");
            ModerationRoomLogs = (Row["moderation_roomlogs_enabled"].ToString() == "1");
            
            MotdEnabled = (Row["motd_enabled"].ToString() == "1");
            MotdType = Motd_Type;
            MotdText = Motd;
            
            NameChangeWaitDays = (int)Row["name_change_wait_days"];
            
            PetsEnabled = (Row["enable_pets"].ToString() == "1");
            PetScratchingAccountDaysOldEnabled = (Row["pet_scratching_account_days_old_enabled"].ToString() == "1");
            PetScratchingAccountDaysOld = (int)Row["pet_scratching_account_days_old"];
            
            NormalUserFriendListSize = (int)Row["normal_user_friend_list_size"];
            HcUserFriendListSize = (int)Row["hc_user_friend_list_size"];
            VipUserFriendListSize = (int)Row["vip_user_friend_list_size"];
            
            WordFilterMaximumCount = (int)Row["wordfilter_maximum_count"];
            WordFilterTimeToMute = (int)Row["wordfilter_time_muted"];
        }

        public static void UpdateInfobusStatus(SqlDatabaseClient MySqlClient)
        {
            MySqlClient.SetParameter("infobus_status", InfobusStatus.ToString().ToLower());
            MySqlClient.ExecuteNonQuery("UPDATE server_settings SET infobus_status = @infobus_status");
        }
    }
}
