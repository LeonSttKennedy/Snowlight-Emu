using Snowlight.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;


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
        private static bool mActivityPointsEnabled;
        private static bool mMoreActivityPointsForVipUsers;
        private static int mActivityPointsInterval;
        private static int mActivityPointsCreditsAmount;
        private static int mMoreActivityPointsCreditsAmount;
        private static int mActivityPointsPixelsAmount;
        private static int mMoreActivityPointsPixelsAmount;
        private static bool mGiftingSystemEnabled;
        private static InfobusStatus mInfobusStatus;
        private static bool mLoginBadgeEnabled;
        private static uint mLoginBadgeId;
        private static bool mMarketplaceEnabled;
        private static int mMarketplaceTax;
        private static int mMarketplaceTokensPrice;
        private static int mMarketplacePremiumTokens;
        private static int mMarketplaceDefaultTokens;
        private static int mMarketplaceMinPrice;
        private static int mMarketplaceMaxPrice;
        private static int mMarketplaceOfferTime;
        private static int mMarketplaceAverageOfferDays;
        private static int mMaxFavoritesPerUser;
        private static int mMaxFurniPerRoom;
        private static int mMaxFurniStacking;
        private static int mMaxPetsPerRoom;
        private static int mMaxRoomsPerUser;
        private static bool mModerationActionLogs;
        private static bool mModerationChatLogs;
        private static bool mModerationRoomLogs;
        private static bool mMotdEnabled;
        private static MotdType mMotdType;
        private static List<string> mMotdText;
        private static int mWordFilterMaximumCount;
        private static int mWordFilterTimeToMute;

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
        public static int ActivityPointsPixelsAmount
        {
            get
            {
                return mActivityPointsPixelsAmount;
            }

            set
            {
                mActivityPointsPixelsAmount = value;
            }
        }
        public static int MoreActivityPointsPixelsAmount
        {
            get
            {
                return mMoreActivityPointsPixelsAmount;
            }

            set
            {
                mMoreActivityPointsPixelsAmount = value;
            }
        }
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
        public static uint LoginBadgeId
        {
            get
            {
                return mLoginBadgeId;
            }

            set
            {
                mLoginBadgeId = value;
            }
        }
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

            ActivityPointsEnabled = (Row["activitypoints_enabled"].ToString() == "1");
            MoreActivityPointsForVipUsers = (Row["more_activitypoints_for_vip_users"].ToString() == "1");
            ActivityPointsInterval = (int)Row["activitypoints_interval"];
            ActivityPointsCreditsAmount = (int)Row["activitypoints_credits_amount"];
            MoreActivityPointsCreditsAmount = (int)Row["more_activitypoints_credits_amount"];
            ActivityPointsPixelsAmount = (int)Row["activitypoints_pixels_amount"];
            MoreActivityPointsPixelsAmount = (int)Row["more_activitypoints_pixels_amount"];
            GiftingSystemEnabled = (Row["gifting_system_enabled"].ToString() == "1");
            InfobusStatus = Infobus_Status;
            LoginBadgeEnabled = (Row["login_badge_enabled"].ToString() == "1");
            LoginBadgeId = (uint)Row["login_badge_id"];
            MarketplaceEnabled = (Row["marketplace_enabled"].ToString() == "1");
            MarketplaceTax = (int)Row["marketplace_tax"];
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
            ModerationRoomLogs = (Row["moderation_roomlogs_enabled"].ToString() == "1");
            MotdEnabled = (Row["motd_enabled"].ToString() == "1");
            MotdType = Motd_Type;
            MotdText = Motd;
            WordFilterMaximumCount = (int)Row["wordfilter_maximum_count"];
            WordFilterTimeToMute = (int)Row["wordfilter_time_muted"];
        }

        public static void UpdateInfobusStatus(SqlDatabaseClient MySql)
        {
            MySql.SetParameter("infobus_status", InfobusStatus.ToString().ToLower());
            MySql.ExecuteNonQuery("UPDATE server_settings SET infobus_status = @infobus_status");
        }
    }
}
