using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Snowlight.Storage;
using Snowlight.Game.Achievements;

namespace Snowlight.Game.Catalog
{
    public static class CatalogStuffWorker
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
            mWorkerThread.Name = "CatalogStuffWorkerThread";
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
                        if (CurrentDay != DateTime.Today)
                        {
                            CatalogManager.RefreshCatalogData(MySqlClient, false);
                            CurrentDay = DateTime.Today;
                        }
                    }

                    Thread.Sleep(60000);
                }
            }
            catch (ThreadAbortException) { }
        }
    }
}
