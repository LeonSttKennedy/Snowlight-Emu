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

                                if (Session.CharacterInfo.NeedsRespectUpdate)
                                {
                                    Session.CharacterInfo.RespectCreditHuman = 3;
                                    Session.CharacterInfo.RespectCreditPets = 3;
                                    Session.CharacterInfo.SetLastRespectUpdate(MySqlClient);
                                    Session.CharacterInfo.SynchronizeRespectData(MySqlClient);
                                }

                                Session.CharacterInfo.UpdateTimeOnline(MySqlClient, 1);
                                AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_AllTimeHotelPresence", 1);

                                HabboTags(Session, MySqlClient);

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

        private static void HabboTags(Session Session, SqlDatabaseClient MySqlClient)
        {
            Session.CharacterInfo.UpdateTags(MySqlClient);

            int UserTagsCount = Session.CharacterInfo.Tags.Count;
            UserAchievement AvatarTagsData = Session.AchievementCache.GetAchievementData("ACH_AvatarTags");
            int CacheTagsProgress = AvatarTagsData != null ? AvatarTagsData.Progress : 0;

            int AvatarTagsIncrease = UserTagsCount - CacheTagsProgress;

            if (AvatarTagsIncrease > 0)
            {
                AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_AvatarTags", AvatarTagsIncrease);
            }

            Session.SendData(RoomUserTagsComposer.Compose(Session.CharacterInfo.Id, Session.CharacterInfo.Tags));
        }
    }
}
