using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

using Snowlight.Storage;
using Snowlight.Game.Achievements;
using Snowlight.Game.Sessions;
using Snowlight.Game.Rooms;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Rights
{
    public class BadgeCache
    {
        private uint mUserId;
        private object mSyncRoot;

        private Dictionary<int, InventoryBadge> mEquippedBadges;
        private List<InventoryBadge> mStaticBadges;
        private Dictionary<string, InventoryBadge> mAchievementBadges;
        private List<string> mIndexCache;
        private List<string> mRightsCache;

        public SortedDictionary<int, InventoryBadge> EquippedBadges
        {
            get
            {
                SortedDictionary<int, InventoryBadge> Copy = new SortedDictionary<int, InventoryBadge>();

                lock (mSyncRoot)
                {
                    foreach (KeyValuePair<int, InventoryBadge> Data in mEquippedBadges)
                    {
                        Copy.Add(Data.Key, Data.Value);
                    }
                }

                return Copy;
            }
        }

        public List<InventoryBadge> Badges
        {
            get
            {
                List<InventoryBadge> Copy = new List<InventoryBadge>();

                lock (mSyncRoot)
                {
                    Copy.AddRange(mStaticBadges);
                    Copy.AddRange(mAchievementBadges.Values.ToList());
                }

                return Copy.OrderBy(B => B.Id).ToList();
            }
        }

        public BadgeCache(SqlDatabaseClient MySqlClient, uint UserId, AchievementCache UserAchievementCache)
        {
            mUserId = UserId;
            mSyncRoot = new object();

            mEquippedBadges = new Dictionary<int, InventoryBadge>();
            mStaticBadges = new List<InventoryBadge>();
            mAchievementBadges = new Dictionary<string, InventoryBadge>();
            mIndexCache = new List<string>();
            mRightsCache = new List<string>();

            ReloadCache(MySqlClient, UserAchievementCache);
        }

        public void ReloadCache(SqlDatabaseClient MySqlClient, AchievementCache UserAchievementCache)
        {
            Dictionary<int, InventoryBadge> EquippedBadges = new Dictionary<int, InventoryBadge>();
            List<InventoryBadge> StaticBadges = new List<InventoryBadge>();
            Dictionary<string, InventoryBadge> AchievementBadges = new Dictionary<string, InventoryBadge>();
            List<string> IndexCache = new List<string>();

            MySqlClient.SetParameter("userid", mUserId);
            DataTable Table = MySqlClient.ExecuteQueryTable("SELECT id,badge_id,slot_id,source_type,source_data FROM badges WHERE user_id = @userid");

            foreach (DataRow Row in Table.Rows)
            {
                BadgeDefinition Badge = RightsManager.GetBadgeDefinitionById((uint)Row["badge_id"]);

                if (Badge == null)
                {
                    continue;
                }

                uint Id = (uint)Row["id"];
                string SourceType = Row["source_type"].ToString();
                string SourceData = Row["source_data"].ToString();

                InventoryBadge BadgeToEquip = null;

                if (SourceType == "static")
                {
                    BadgeToEquip = new InventoryBadge(Id, Badge);
                    StaticBadges.Add(BadgeToEquip);
                }
                else if (SourceType == "achievement")
                {
                    if (AchievementBadges.ContainsKey(SourceData))
                    {
                        continue;
                    }

                    UserAchievement UserAchievement = UserAchievementCache.GetAchievementData(SourceData);

                    if (UserAchievement == null || UserAchievement.Level < 1)
                    {
                        MySqlClient.SetParameter("userid", mUserId);
                        MySqlClient.SetParameter("badgeid", Badge.Id);
                        MySqlClient.ExecuteNonQuery("DELETE FROM badges WHERE user_id = @userid AND badge_id = @badgeid");
                        continue;
                    }

                    string Code = UserAchievement.GetBadgeCodeForLevel();

                    BadgeToEquip = new InventoryBadge(Id, (Badge.Code == Code ? Badge : RightsManager.GetBadgeDefinitionByCode(Code)));
                    AchievementBadges.Add(SourceData, BadgeToEquip);
                }

                if (BadgeToEquip != null)
                {
                    int SlotId = (int)Row["slot_id"];

                    if (!EquippedBadges.ContainsKey(SlotId) && SlotId >= 1 && SlotId <= 5)
                    {
                        EquippedBadges.Add(SlotId, BadgeToEquip);
                    }

                    IndexCache.Add(BadgeToEquip.Definition.Code);
                }
            }

            lock (mSyncRoot)
            {
                mEquippedBadges = EquippedBadges;
                mStaticBadges = StaticBadges;
                mAchievementBadges = AchievementBadges;
                mRightsCache = RegenerateRights();
                mIndexCache = IndexCache;
            }
        }

        public List<string> RegenerateRights()
        {
            List<string> Rights = new List<string>();
            Rights.AddRange(RightsManager.GetDefaultRights());

            lock (mSyncRoot)
            {
                foreach (InventoryBadge Badge in mStaticBadges)
                {
                    Rights.AddRange(RightsManager.GetRightsForBadge(Badge.Definition));
                }

                foreach (InventoryBadge Badge in mAchievementBadges.Values)
                {
                    Rights.AddRange(RightsManager.GetRightsForBadge(Badge.Definition));
                }
            }

            return Rights;
        }

        public bool ContainsCode(string BadgeCode)
        {
            lock (mSyncRoot)
            {
                return mIndexCache.Contains(BadgeCode);
            }
        }

        public bool HasRight(string Right)
        {
            lock (mSyncRoot)
            {
                return mRightsCache.Contains(Right);
            }
        }

        public void UpdateAchievementBadge(SqlDatabaseClient MySqlClient, string AchievementGroup, BadgeDefinition NewBadge, AchievementCache UserAchievementCache, string SourceType = "achievement")
        {
            MySqlClient.SetParameter("userid", mUserId);
            MySqlClient.SetParameter("sourcetype", SourceType);
            MySqlClient.SetParameter("sourcedata", AchievementGroup);
            MySqlClient.SetParameter("badgeid", NewBadge.Id);

            lock (mSyncRoot)
            {
                if (mAchievementBadges.ContainsKey(AchievementGroup))
                {
                    BadgeDefinition OldBadge = mAchievementBadges[AchievementGroup].Definition;

                    if (OldBadge == NewBadge)
                    {
                        MySqlClient.ClearParameters();
                        return;
                    }

                    mIndexCache.Remove(OldBadge.Code);

                    uint Id = mAchievementBadges[AchievementGroup].Id;
                    InventoryBadge NewInventoryBadge = new InventoryBadge(Id, NewBadge);

                    mAchievementBadges[AchievementGroup] = NewInventoryBadge;

                    MySqlClient.ExecuteNonQuery("UPDATE badges SET badge_id = @badgeid WHERE user_id = @userid AND source_type = @sourcetype AND source_data = @sourcedata LIMIT 1");

                    foreach (KeyValuePair<int, InventoryBadge> Badge in mEquippedBadges)
                    {
                        if (Badge.Value.Id == OldBadge.Id)
                        {
                            mEquippedBadges[Badge.Key] = NewInventoryBadge;
                            break;
                        }
                    }
                }
                else
                {
                    string RawId = MySqlClient.ExecuteScalar("INSERT INTO badges (user_id,badge_id,source_type,source_data) VALUES (@userid,@badgeid,@sourcetype,@sourcedata); SELECT LAST_INSERT_ID();").ToString(); ;
                    uint.TryParse(RawId, out uint Id);
                    InventoryBadge NewInventoryBadge = new InventoryBadge(Id, NewBadge);

                    mAchievementBadges.Add(AchievementGroup, NewInventoryBadge);
                }

                mRightsCache = RegenerateRights();
                mIndexCache.Add(NewBadge.Code);
                ReloadCache(MySqlClient, UserAchievementCache);
            }
        }

        public void UpdateBadgeOrder(SqlDatabaseClient MySqlClient, Dictionary<int, InventoryBadge> NewSettings)
        {
            MySqlClient.SetParameter("userid", mUserId);
            MySqlClient.ExecuteNonQuery("UPDATE badges SET slot_id = 0 WHERE user_id = @userid");

            foreach (KeyValuePair<int, InventoryBadge> EquippedBadge in NewSettings)
            {
                MySqlClient.SetParameter("userid", mUserId);
                MySqlClient.SetParameter("slotid", EquippedBadge.Key);
                MySqlClient.SetParameter("badgeid", EquippedBadge.Value.Id);
                MySqlClient.ExecuteNonQuery("UPDATE badges SET slot_id = @slotid WHERE user_id = @userid AND badge_id = @badgeid LIMIT 1");
            }

            lock (mSyncRoot)
            {
                mEquippedBadges = NewSettings;
            }
        }

        public void DisableSubscriptionBadge(string BadgeCodePrefix)
        {
            lock (mSyncRoot)
            {
                foreach (KeyValuePair<string, InventoryBadge> Data in mAchievementBadges)
                {
                    if (Data.Value.Definition.Code.StartsWith(BadgeCodePrefix))
                    {
                        mIndexCache.Remove(Data.Value.Definition.Code);
                        mAchievementBadges.Remove(Data.Key);

                        foreach (KeyValuePair<int, InventoryBadge> EquipData in mEquippedBadges)
                        {
                            if (EquipData.Value.Definition.Code.StartsWith(BadgeCodePrefix))
                            {
                                mEquippedBadges.Remove(EquipData.Key);
                                break;
                            }
                        }

                        break;
                    }
                }

                mRightsCache = RegenerateRights();
            }
        }

        public bool ContainsCodeWith(string Code)
        {
            lock (mSyncRoot)
            {
                foreach (string Item in mIndexCache)
                {
                    if (Item.StartsWith(Code))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public InventoryBadge GetBadge(string Code)
        {
            InventoryBadge UserBadge = null;

            lock (mSyncRoot)
            {
                foreach (InventoryBadge Badge in Badges)
                {
                    if (Badge.Definition.Code == Code)
                    {
                        UserBadge = Badge;
                        break;
                    }
                }
            }

            return UserBadge;
        }
    }
}