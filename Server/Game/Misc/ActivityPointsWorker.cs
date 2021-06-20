using System;
using System.Threading;

using Snowlight.Game.Sessions;

using Snowlight.Communication.Outgoing;
using Snowlight.Storage;
using Snowlight.Specialized;
using System.Collections.Generic;
using Snowlight.Config;
using Snowlight.Util;

namespace Snowlight.Game.Misc
{
    public static class ActivityPointsWorker
    {
        private static Thread mWorkerThread;

        public static void Initialize()
        {
            if (!ServerSettings.ActivityPointsEnabled)
            {
                return;
            }

            mWorkerThread = new Thread(new ThreadStart(ProcessThread));
            mWorkerThread.Priority = ThreadPriority.Lowest;
            mWorkerThread.Name = "ActivityPointsWorkerThread";
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
                int Interval = ServerSettings.ActivityPointsInterval;
                int CreditsAmount = ServerSettings.ActivityPointsCreditsAmount;
                int PixelsAmount = ServerSettings.ActivityPointsPixelsAmount;

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
                                if (!Session.Authenticated || Session.CharacterInfo.TimeSinceLastActivityPointsUpdate <= Interval)
                                {
                                    continue;
                                }

                                if (ServerSettings.MoreActivityPointsForVipUsers && Session.HasRight("club_vip"))
                                {
                                    CreditsAmount += ServerSettings.MoreActivityPointsCreditsAmount;
                                    PixelsAmount += ServerSettings.MoreActivityPointsPixelsAmount;
                                }

                                if (CreditsAmount > 0)
                                {
                                    Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, CreditsAmount);
                                    Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                                }

                                if (PixelsAmount > 0)
                                {
                                    Session.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, PixelsAmount);
                                    Session.SendData(ActivityPointsBalanceComposer.Compose(Session.CharacterInfo.ActivityPointsBalance, PixelsAmount));
                                }

                                Session.CharacterInfo.SetLastActivityPointsUpdate(MySqlClient);
                            }
                        }
                    }

                    Thread.Sleep((Interval / 5) * 1000);
                }
            }
            catch (ThreadAbortException) { }
        }
    }
}
