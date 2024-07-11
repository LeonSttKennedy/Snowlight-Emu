using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Rooms;
using Snowlight.Game.Rights;
using Snowlight.Game.Catalog;
using Snowlight.Game.Sessions;
using Snowlight.Game.Characters;
using Snowlight.Game.Achievements;
using System.Collections.ObjectModel;
using Snowlight.Communication.Outgoing;

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

                                Session.SubscriptionManager.AddGiftPoints(true);
                                #endregion

                                #region Delivery Activity Points and Credits
                                DeliveryStuff(Session, MySqlClient);
                                #endregion

                                #region Delivery Some ACH FROM HERE
                                TimeOnline(Session, MySqlClient);
                                HabboTags(Session, MySqlClient);
                                TradePass(Session, MySqlClient);
                                Session.SubscriptionManager.UpdateUserBadge();
                                #endregion

                                #region Check User Subscription
                                CheckSubStatus(Session);
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

        #region Subscription
        private static void CheckSubStatus(Session Session)
        {
            // if subscription isn't active
            if (!Session.SubscriptionManager.IsActive)
            {
                // and if subscription level isn't changed
                if (Session.SubscriptionManager.SubscriptionLevel != ClubSubscriptionLevel.None)
                {
                    // we can expire user subscription
                    Session.SubscriptionManager.Expire();

                    // and send a new data to client
                    Session.SendData(SubscriptionStatusComposer.Compose(Session.SubscriptionManager));
                }
            }
        }
        #endregion

        #region Activity Points and Credits
        private static void DeliveryStuff(Session Session, SqlDatabaseClient MySqlClient)
        {
            TimeSpan TS = UnixTimestamp.ElapsedTime(Session.CharacterInfo.LastActivityPointsUpdate);
            TimeSpan TSLastLogin = UnixTimestamp.ElapsedTime(Session.CharacterInfo.TimestampLastOnline);

            int CreditsAmount = 0;
            Dictionary<SeasonalCurrencyList, int> ActivityPointsToDelivery = new Dictionary<SeasonalCurrencyList, int>();

            #region Daily login reward
            if (ServerSettings.DailyRewardEnabled && !Session.CharacterInfo.ReceivedDailyReward)
            {
                if (Session.InRoom && TSLastLogin.TotalMinutes >= ServerSettings.DailyRewardWaitTime)
                {
                    if (ServerSettings.DailyRewardCreditsAmount > 0)
                    {
                        CreditsAmount += ServerSettings.DailyRewardCreditsAmount;
                    }

                    if (ServerSettings.DailyRewardActivityPointAmount > 0)
                    {
                        if (ActivityPointsToDelivery.ContainsKey(ServerSettings.ActivityPointsType))
                        {
                            ActivityPointsToDelivery[ServerSettings.DailyActivityPointsType] += ServerSettings.DailyRewardActivityPointAmount;
                        }
                        else
                        {
                            ActivityPointsToDelivery.Add(ServerSettings.DailyActivityPointsType, ServerSettings.DailyRewardActivityPointAmount);
                        }
                    }

                    Session.CharacterInfo.ReceivedDailyReward = true;
                    Session.CharacterInfo.UpdateReceivedDailyReward(MySqlClient);
                }
            }
            #endregion

            #region Activity points reward
            if (ServerSettings.ActivityPointsEnabled)
            {
                if (Session.InRoom && TS.TotalMinutes >= ServerSettings.ActivityPointsInterval)
                {
                    if (ServerSettings.ActivityPointsCreditsAmount > 0)
                    {
                        CreditsAmount += ServerSettings.ActivityPointsCreditsAmount;
                    }

                    if (ServerSettings.ActivityPointsAmount > 0)
                    {
                        if (ActivityPointsToDelivery.ContainsKey(ServerSettings.ActivityPointsType))
                        {
                            ActivityPointsToDelivery[ServerSettings.ActivityPointsType] += ServerSettings.ActivityPointsAmount;
                        }
                        else
                        {
                            ActivityPointsToDelivery.Add(ServerSettings.ActivityPointsType, ServerSettings.ActivityPointsAmount);
                        }
                    }

                    if (ServerSettings.MoreActivityPointsForVipUsers && Session.HasRight("club_vip"))
                    {
                        CreditsAmount += ServerSettings.MoreActivityPointsCreditsAmount;

                        if (ActivityPointsToDelivery.ContainsKey(ServerSettings.ActivityPointsType))
                        {
                            ActivityPointsToDelivery[ServerSettings.ActivityPointsType] += ServerSettings.MoreActivityPointsAmount;
                        }
                        else
                        {
                            ActivityPointsToDelivery.Add(ServerSettings.ActivityPointsType, ServerSettings.MoreActivityPointsAmount);
                        }
                    }
                }

                Session.CharacterInfo.SetLastActivityPointsUpdate(MySqlClient);
            }
            #endregion

            if (CreditsAmount > 0)
            {
                Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, CreditsAmount);
                Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
            }

            if (ActivityPointsToDelivery.Count > 0)
            {
                foreach (KeyValuePair<SeasonalCurrencyList, int> Ap in ActivityPointsToDelivery)
                {
                    Session.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, Ap.Key, Ap.Value);
                    if (Ap.Key == SeasonalCurrencyList.Pixels)
                    {
                        Session.SendData(UpdatePixelsBalanceComposer.Compose(Session.CharacterInfo.ActivityPoints[0], Ap.Value));
                    }
                    Session.SendData(UserActivityPointsBalanceComposer.Compose(Session.CharacterInfo.ActivityPoints));
                }
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
