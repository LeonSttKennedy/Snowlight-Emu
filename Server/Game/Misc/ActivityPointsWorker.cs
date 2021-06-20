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


                                if (ToIncrease(Session)[0] > 0)
                                {
                                    Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, ToIncrease(Session)[0]);
                                    Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                                }

                                if (ToIncrease(Session)[1] > 0)
                                {
                                    Session.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, ToIncrease(Session)[1]);
                                    Session.SendData(ActivityPointsBalanceComposer.Compose(Session.CharacterInfo.ActivityPointsBalance, ToIncrease(Session)[1]));
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

        private static List<int> ToIncrease(Session Session)
        {
            List<int> ToReturn = new List<int>();

            int CreditsAmount = ServerSettings.ActivityPointsCreditsAmount;
            int PixelsAmount = ServerSettings.ActivityPointsPixelsAmount;
            
            if (Session.HasRight("club_vip"))
            {
                CreditsAmount += ServerSettings.MoreActivityPointsCreditsAmount;
                PixelsAmount += ServerSettings.MoreActivityPointsPixelsAmount;
            }

            ToReturn.Add(CreditsAmount);
            ToReturn.Add(PixelsAmount);

            return ToReturn;
        }
    }
}
