using Snowlight.Game.Achievements;
using Snowlight.Game.Catalog;
using Snowlight.Game.Characters;
using Snowlight.Game.Quests;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Snowlight.Game.Misc
{
    public static class RoomVisitorWorker
    {
        private static Thread mWorkerThread;

        public static void Initialize()
        {
            mWorkerThread = new Thread(new ThreadStart(ProcessThread));
            mWorkerThread.Priority = ThreadPriority.Lowest;
            mWorkerThread.Name = "RoomVisitorWorkerThread";
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
                    using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                    {
                        List<RoomInstance> CurrentInstanceList = RoomManager.RoomInstances.Values.ToList();

                        foreach (RoomInstance RoomInstance in CurrentInstanceList)
                        {
                            CharacterInfo OwnerInfo = CharacterInfoLoader.GetCharacterInfo(MySqlClient, RoomInstance.Info.OwnerId);

                            bool OwnerIsInRoom = RoomInstance.GetActorByReferenceId(OwnerInfo.Id) != null;

                            int UserCount = RoomInstance.Info.CurrentUsers;

                            if (OwnerIsInRoom)
                            {
                                UserCount--;
                            }

                            if(UserCount > 0)
                            {
                                if (OwnerInfo.HasLinkedSession)
                                {
                                    Session OwnerSession = SessionManager.GetSessionByCharacterId(OwnerInfo.Id);

                                    AchievementManager.ProgressUserAchievement(MySqlClient, OwnerSession, "ACH_RoomDecoHosting", UserCount);

                                    if (RoomInstance.MusicController.IsPlaying)
                                    {
                                        AchievementManager.ProgressUserAchievement(MySqlClient, OwnerSession, "ACH_MusicPlayer", UserCount);
                                    }
                                }
                                else
                                {
                                    AchievementManager.OfflineProgressUserAchievement(MySqlClient, OwnerInfo.Id, "ACH_RoomDecoHosting", UserCount);

                                    if (RoomInstance.MusicController.IsPlaying)
                                    {
                                        AchievementManager.OfflineProgressUserAchievement(MySqlClient, OwnerInfo.Id, "ACH_MusicPlayer", UserCount);
                                    }
                                }
                            }
                        }
                    }

                    Thread.Sleep(60000);
                }
            }
            catch (ThreadAbortException) { }
        }
    }
}
