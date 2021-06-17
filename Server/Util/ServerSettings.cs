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
        private static bool mMotdEnabled;
        private static MotdType mMotdType;
        private static string mMotdText;
        private static bool mLoginBadgeEnabled;
        private static uint mLoginBadgeId;

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
        public static string MotdText
        {
            get 
            { 
                return mMotdText; 
            }
            
            set 
            {
                mMotdText = value.Replace("\\n", "\n"); 
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

            MotdEnabled = (Row["motd_enabled"].ToString() == "1");
            MotdType = Motd_Type;
            MotdText = (string)Row["motd_text"];
            LoginBadgeEnabled = (Row["login_badge_enabled"].ToString() == "1");
            LoginBadgeId = (uint)Row["login_badge_id"];
        }
    }
}
