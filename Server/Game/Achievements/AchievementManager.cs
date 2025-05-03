using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Snowlight.Storage;
using Snowlight.Communication;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Rights;
using Snowlight.Game.Rooms;
using Snowlight.Communication.Incoming;
using Snowlight.Util;
using Snowlight.Game.Messenger;
using Snowlight.Game.FriendStream;
using Snowlight.Game.Misc;

namespace Snowlight.Game.Achievements
{
    public static class AchievementManager
    {
        private static Dictionary<string, Achievement> mAchievements;
        private static object mSyncRoot;

        public static Dictionary<string, Achievement> Achievements
        {
            get
            {
                Dictionary<string, Achievement> Achievements = new Dictionary<string, Achievement>();

                lock (mAchievements)
                {
                    foreach (KeyValuePair<string, Achievement> Achievement in mAchievements)
                    {
                        Achievements.Add(Achievement.Key, Achievement.Value);
                    }
                }

                return Achievements;
            }
        }

        public static void Initialize(SqlDatabaseClient MySqlClient)
        {
            mAchievements = new Dictionary<string, Achievement>();
            mSyncRoot = new object();

            ReloadAchievements(MySqlClient);

            DataRouter.RegisterHandler(OpcodesIn.ACHIEVEMENTS_GET_LIST, new ProcessRequestCallback(GetList));
        }

        public static void ReloadAchievements(SqlDatabaseClient MySqlClient)
        {
            lock (mSyncRoot)
            {
                mAchievements.Clear();

                DataTable Table = MySqlClient.ExecuteQueryTable("SELECT * FROM achievements");

                foreach (DataRow Row in Table.Rows)
                {
                    string Group = (string)Row["group_name"];

                    if (!mAchievements.ContainsKey(Group))
                    {
                        mAchievements.Add(Group, new Achievement((uint)Row["id"], Group, (string)Row["category"]));
                    }

                    mAchievements[Group].AddLevel(new AchievementLevel((int)Row["level"], (int)Row["reward_activity_points"],
                        SeasonalCurrency.FromStringToEnum(Row["seasonal_currency"].ToString()),
                        (int)Row["reward_points"], (int)Row["progress_needed"]));

                }
            }
        }

        private static void GetList(Session Session, ClientMessage Message)
        {
            Session.SendData(AchievementListComposer.Compose(Session, mAchievements.Values.ToList().AsReadOnly()));
        }

        public static bool ProgressUserAchievement(SqlDatabaseClient MySqlClient, Session Session, string AchievementGroup, int ProgressAmount)
        {
            if (!mAchievements.ContainsKey(AchievementGroup))
            {
                return false;
            }

            Achievement AchievementData = null;

            lock (mSyncRoot)
            {
                AchievementData = mAchievements[AchievementGroup];
            }

            UserAchievement UserData = Session.AchievementCache.GetAchievementData(AchievementData.GroupName);

            int TotalLevels = AchievementData.Levels.Count;

            if (UserData != null && UserData.Level == TotalLevels)
            {
                return false; // done, no more.
            }

            int TargetLevel = (UserData != null ? UserData.Level + 1 : 1);

            if (TargetLevel > TotalLevels)
            {
                TargetLevel = TotalLevels;
            }

            AchievementLevel TargetLevelData = AchievementData.Levels[TargetLevel];

            int NewProgress = (UserData != null ? UserData.Progress + ProgressAmount : ProgressAmount);
            int NewLevel = (UserData != null ? UserData.Level : 0);
            int NewTarget = NewLevel + 1;

            if (NewTarget > TotalLevels)
            {
                NewTarget = TotalLevels;
            }

            if (NewProgress >= TargetLevelData.Requirement)
            {
                NewLevel++;
                NewTarget++;

                int ProgressRemainder = NewProgress - TargetLevelData.Requirement;

                NewProgress = TargetLevelData.Requirement; // = 0; We don't need reset the user progress

                BadgeDefinition BadgeData = RightsManager.GetBadgeDefinitionByCode(AchievementGroup + TargetLevel);

                if (NewTarget > TotalLevels)
                {
                    NewTarget = TotalLevels;
                }

                Session.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, TargetLevelData.SeasonalCurrency,
                    TargetLevelData.ActivityPointsReward);

                if (TargetLevelData.SeasonalCurrency == SeasonalCurrencyList.Pixels)
                {
                    Session.SendData(UpdatePixelsBalanceComposer.Compose(Session.CharacterInfo.ActivityPoints[0],
                        TargetLevelData.ActivityPointsReward));
                }

                Session.SendData(UserActivityPointsBalanceComposer.Compose(Session.CharacterInfo.ActivityPoints));

                Session.SendData(AchievementUnlockedComposer.Compose(AchievementData, TargetLevel, TargetLevelData.PointsReward,
                    (int)TargetLevelData.SeasonalCurrency, TargetLevelData.ActivityPointsReward));

                Session.AchievementCache.AddOrUpdateData(MySqlClient, AchievementGroup, NewLevel, NewProgress);

                Session.BadgeCache.UpdateAchievementBadge(MySqlClient, AchievementGroup, BadgeData, Session.AchievementCache);

                Session.SendData(UserBadgeInventoryComposer.Compose(Session.BadgeCache.Badges, Session.BadgeCache.EquippedBadges));

                InventoryBadge UserBadge = Session.BadgeCache.GetBadge(AchievementGroup + TargetLevel);
                Session.NewItemsCache.MarkNewItem(MySqlClient, NewItemsCategory.Badges, UserBadge.Id);
                Session.NewItemsCache.SendNewItems(Session);

                Session.CharacterInfo.UpdateScore(MySqlClient, TargetLevelData.PointsReward);
                Session.SendData(AchievementScoreUpdateComposer.Compose(Session.CharacterInfo.Score));

                AchievementLevel NewLevelData = AchievementData.Levels[NewTarget];
                Session.SendData(AchievementProgressComposer.Compose(AchievementData, NewTarget, NewLevelData,
                    TotalLevels, Session.AchievementCache.GetAchievementData(AchievementGroup)));

                Session.SendInfoUpdate();

                Session.MessengerFriendCache.BroadcastToFriends(MessengerFriendEventComposer.Compose(Session.CharacterId,
                    MessengerFriendEventType.AchievementUnlocked, BadgeData.Code));

                if (Session.CharacterInfo.AllowFriendStream)
                {
                    FriendStreamHandler.InsertNewEvent(Session.CharacterId, EventStreamType.AchievementEarned, BadgeData.Code);
                }

                RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

                if (Instance != null)
                {
                    Instance.BroadcastMessage(RoomUserBadgesComposer.Compose(Session.CharacterId,
                        Session.BadgeCache.EquippedBadges));
                }

                if (ProgressRemainder > 0)
                {
                    ProgressUserAchievement(MySqlClient, Session, AchievementGroup, ProgressRemainder);
                }

                return true;
            }

            Session.AchievementCache.AddOrUpdateData(MySqlClient, AchievementGroup, NewLevel, NewProgress);
            Session.SendData(AchievementProgressComposer.Compose(AchievementData, TargetLevel, TargetLevelData,
            TotalLevels, Session.AchievementCache.GetAchievementData(AchievementGroup)));
            return false;
        }

        public static void OfflineProgressUserAchievement(SqlDatabaseClient MySqlClient, uint UserId, string AchievementGroup, int ProgressAmount)
        {
            if (!mAchievements.ContainsKey(AchievementGroup))
            {
                return;
            }

            MySqlClient.SetParameter("uid", UserId);
            MySqlClient.SetParameter("gid", AchievementGroup);
            bool CreatNewRecord = (MySqlClient.ExecuteScalar("SELECT null FROM achievements_to_unlock WHERE user_id = @uid AND group_id = @gid LIMIT 1") == null);

            if (CreatNewRecord)
            {
                MySqlClient.SetParameter("uid", UserId);
                MySqlClient.SetParameter("gid", AchievementGroup);
                MySqlClient.SetParameter("progress", ProgressAmount);
                MySqlClient.ExecuteQueryTable("INSERT INTO achievements_to_unlock (user_id,group_id,progress) VALUES (@uid, @gid, @progress)");
            }
            else
            {
                MySqlClient.SetParameter("uid", UserId);
                MySqlClient.SetParameter("gid", AchievementGroup);
                MySqlClient.SetParameter("progressa", ProgressAmount);
                MySqlClient.ExecuteQueryTable("UPDATE achievements_to_unlock SET progress = progress + @progressa WHERE user_id = @uid AND group_id = @gid");
            }
        }
        public static void VerifyProgressUserAchievement(SqlDatabaseClient MySqlClient, Session Session) 
        {
            uint UserId = Session.CharacterId;

            MySqlClient.SetParameter("uid", UserId);
            DataTable Table = MySqlClient.ExecuteQueryTable("SELECT * FROM achievements_to_unlock WHERE user_id = @uid");

            foreach (DataRow Row in Table.Rows)
            {
                string GroupId = (string)Row["group_id"];
                int Progress = (int)Row["progress"];
                ProgressUserAchievement(MySqlClient, Session, GroupId, Progress);

                MySqlClient.SetParameter("uid", UserId);
                MySqlClient.SetParameter("gid", GroupId);
                MySqlClient.ExecuteNonQuery("DELETE FROM achievements_to_unlock WHERE user_id = @uid AND group_id = @gid LIMIT 1");
            }
        }

        public static Achievement GetAchievement(string AchievementGroup)
        {
            lock (mSyncRoot)
            {
                if (mAchievements.ContainsKey(AchievementGroup))
                {
                    return mAchievements[AchievementGroup];
                }
            }

            return null;
        }
    }
}
