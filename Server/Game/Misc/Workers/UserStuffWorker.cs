using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Snowlight.Storage;
using Snowlight.Game.Sessions;
using Snowlight.Game.Achievements;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Characters;
using Snowlight.Game.Rooms;
using Snowlight.Util;
using Snowlight.Game.Rights;
using Snowlight.Game.Catalog;
using System.Collections.ObjectModel;

namespace Snowlight.Game.Misc
{
    public static class UserStuffWorker
    {
        private static Thread mWorkerThread;
        private static DateTime mCurrentDay;

        private static DateTime CurrentDay
        {
            get 
            {
                return mCurrentDay;
            }

            set
            {
                mCurrentDay = value;
            }
        }

        public static void Initialize()
        {
            CurrentDay = DateTime.Today;

            mWorkerThread = new Thread(new ThreadStart(ProcessThread));
            mWorkerThread.Priority = ThreadPriority.Lowest;
            mWorkerThread.Name = "UserStuffWorkerThread";
            mWorkerThread.Start();
        }

        public static void Stop()
        {
            if (mWorkerThread != null)
            {
                mWorkerThread.Abort();
                mWorkerThread = null;
            }
        }

        private static void ProcessThread()
        {
            try
            {
                Thread.Sleep(60000);

                while (Program.Alive)
                {
                    Dictionary<uint, Session> Sessions = SessionManager.Sessions;

                    if (Sessions.Count > 0)
                    {
                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            foreach (Session Session in Sessions.Values)
                            {
                                if (!Session.Authenticated || Session.Stopped || !Session.InRoom)
                                {
                                    continue;
                                }

                                #region Delivery Users stuff
                                if (Session.CharacterInfo.NeedsRespectUpdate)
                                {
                                    Session.CharacterInfo.RespectCreditHuman = 3;
                                    Session.CharacterInfo.RespectCreditPets = 3;
                                    Session.CharacterInfo.SynchronizeRespectData(MySqlClient);
                                    Session.CharacterInfo.SetLastRespectUpdate(MySqlClient);
                                    Session.SendData(UserObjectComposer.Compose(Session));
                                }

                                Session.SubscriptionManager.UpdateGiftPoints(true);
                                #endregion

                                #region Delivery Activity Points and Credits
                                if (ServerSettings.ActivityPointsEnabled)
                                {
                                    DeliveryStuff(Session, MySqlClient);
                                }
                                #endregion

                                #region Delivery Some ACH FROM HERE
                                TimeOnline(Session, MySqlClient);
                                HabboTags(Session, MySqlClient);
                                TradePass(Session, MySqlClient);
                                Session.SubscriptionManager.UpdateUserBadge();
                                #endregion

                                if (CurrentDay != DateTime.Today)
                                {
                                    CurrentDay = DateTime.Today;
                                }
                            }
                        }
                    }

                    Thread.Sleep(60000);
                }
            }
            catch (ThreadAbortException) { }
        }

        #region Activity Points and Credits
        private static void DeliveryStuff(Session Session, SqlDatabaseClient MySqlClient)
        {
            TimeSpan TS = UnixTimestamp.ElapsedTime(Session.CharacterInfo.TimeSinceLastActivityPointsUpdate);

            int CreditsAmount = ServerSettings.ActivityPointsCreditsAmount;
            int ActivityPointsAmount = ServerSettings.ActivityPointsPixelsAmount;

            if (ServerSettings.MoreActivityPointsForVipUsers && Session.HasRight("club_vip"))
            {
                CreditsAmount += ServerSettings.MoreActivityPointsCreditsAmount;
                ActivityPointsAmount += ServerSettings.MoreActivityPointsPixelsAmount;
            }

            if (Session.InRoom && TS.TotalMinutes >= ServerSettings.ActivityPointsInterval)
            {
                if (CreditsAmount > 0)
                {
                    Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, CreditsAmount);
                    Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                }

                if (ActivityPointsAmount > 0)
                {
                    Session.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, ServerSettings.ActivityPointsType, ActivityPointsAmount);
                    if (ServerSettings.ActivityPointsType == SeasonalCurrencyList.Pixels)
                    {
                        Session.SendData(UpdatePixelsBalanceComposer.Compose(Session.CharacterInfo.ActivityPoints[0], ActivityPointsAmount));
                    }
                    Session.SendData(UserActivityPointsBalanceComposer.Compose(Session.CharacterInfo.ActivityPoints));
                }

                Session.CharacterInfo.SetLastActivityPointsUpdate(MySqlClient);
            }
        }
        #endregion

        #region Other Stuff
        private static void TimeOnline(Session Session, SqlDatabaseClient MySqlClient)
        {
            Session.CharacterInfo.UpdateTimeOnline(MySqlClient, 1);

            int UserOnlineTimeCount = Session.CharacterInfo.TimeOnline;
            UserAchievement OnlineTimeData = Session.AchievementCache.GetAchievementData("ACH_AllTimeHotelPresence");
            int CacheTime = OnlineTimeData != null ? OnlineTimeData.Progress : 0;

            int TimeOnlineIncrease = UserOnlineTimeCount - CacheTime;

            if(TimeOnlineIncrease > 0)
            {
                AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_AllTimeHotelPresence", TimeOnlineIncrease);
            }
        }
        private static void TradePass(Session Session, SqlDatabaseClient MySqlClient)
        {
            TimeSpan TotalDaysRegistered = DateTime.Now - UnixTimestamp.GetDateTimeFromUnixTimestamp(Session.CharacterInfo.TimestampRegistered);
            if (TotalDaysRegistered.TotalDays >= 1 /*&& Session.CharacterInfo.VerifyedAccount*/)
            {
                AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_TraderPass", 1);
            }
        }
        private static void HabboTags(Session Session, SqlDatabaseClient MySqlClient)
        {
            IList<string> HandleOldUserTags = Session.CharacterInfo.Tags;
            Session.CharacterInfo.UpdateTags(MySqlClient);
            
            bool IsEqual = Enumerable.SequenceEqual(HandleOldUserTags, Session.CharacterInfo.Tags);
            if (!IsEqual)
            {
                RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

                if (Instance != null)
                {
                    Instance.BroadcastMessage(RoomUserTagsComposer.Compose(Session.CharacterInfo.Id, Session.CharacterInfo.Tags));
                }
            }

            int UserTagsCount = Session.CharacterInfo.Tags.Count;
            UserAchievement AvatarTagsData = Session.AchievementCache.GetAchievementData("ACH_AvatarTags");
            int CacheTagsProgress = AvatarTagsData != null ? AvatarTagsData.Progress : 0;

            int AvatarTagsIncrease = UserTagsCount - CacheTagsProgress;

            if (AvatarTagsIncrease > 0)
            {
                AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_AvatarTags", AvatarTagsIncrease);
            }
        }
        #endregion
    }
}
