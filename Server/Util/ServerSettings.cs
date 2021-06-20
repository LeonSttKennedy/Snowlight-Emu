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

    public static class ServerSettings
    {
        private static bool mActivityPointsEnabled;
        private static bool mMoreActivityPointsForVipUsers;
        private static int mActivityPointsInterval;
        private static int mActivityPointsCreditsAmount;
        private static int mMoreActivityPointsCreditsAmount;
        private static int mActivityPointsPixelsAmount;
        private static int mMoreActivityPointsPixelsAmount;
        private static bool mLoginBadgeEnabled;
        private static uint mLoginBadgeId;
        private static int mMarketplaceTax;
        private static int mMarketplaceMaxPrice;
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

        public static void Initialize(SqlDatabaseClient MySqlClient)
        {
            DataRow Row = MySqlClient.ExecuteQueryRow("SELECT * FROM server_settings LIMIT 1");
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
            LoginBadgeEnabled = (Row["login_badge_enabled"].ToString() == "1");
            LoginBadgeId = (uint)Row["login_badge_id"];
            MarketplaceTax = (int)Row["marketplace_tax"];
            MarketplaceMaxPrice = (int)Row["marketplace_max_price"];
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
        }
    }
}
