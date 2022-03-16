using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Snowlight.Storage;
using Snowlight.Game.Sessions;

namespace Snowlight.Game.Misc
{
    public static class DailyStuffWorker
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
            mWorkerThread.Name = "DailyStuffWorkerThread";
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
                                if (!Session.Authenticated || !Session.Stopped)
                                {
                                    continue;
                                }

                                // First check if account needs a repect update
                                if (Session.CharacterInfo.NeedsRespectUpdate)
                                {
                                    Session.CharacterInfo.RespectCreditHuman = 3;
                                    Session.CharacterInfo.RespectCreditPets = 3;
                                    Session.CharacterInfo.SetLastRespectUpdate(MySqlClient);
                                    Session.CharacterInfo.SynchronizeRespectData(MySqlClient);
                                }

                                if (CurrentDay != DateTime.Today)
                                {
                                    CurrentDay = DateTime.Today;
                                }
                            }
                        }
                    }

                    Thread.Sleep((1800 / 5) * 1000);
                }
            }
            catch (ThreadAbortException) { }
        }
    }
}
