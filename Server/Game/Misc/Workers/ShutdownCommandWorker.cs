using Snowlight.Communication.Outgoing;
using Snowlight.Game.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Snowlight.Game.Misc
{
    public static class ShutdownCommandWorker
    {
        private static Thread mWorkerThread;
        private static TimeSpan mCloseHour;
        private static bool mShutdown;
        private static bool mTenMinToClose;

        public static TimeSpan CloseHour
        {
            get
            {
                return mCloseHour;
            }

            set
            {
                mCloseHour = value;
            }
        }

        public static bool Shutdown
        {
            get
            {
                return mShutdown;
            }

            set
            {
                mShutdown = value;
            }
        }

        public static bool TenMinToCloseAdvise
        {
            get
            {
                return mTenMinToClose;
            }

            set
            {
                mTenMinToClose = value;
            }
        }

        public static TimeSpan Comparassion
        {
            get
            {
                return mCloseHour - TimeSpan.Parse(DateTime.Now.ToShortTimeString());
            }
        }

        public static void Initialize()
        {
            mWorkerThread = new Thread(new ThreadStart(ProcessThread));
            mWorkerThread.Priority = ThreadPriority.Lowest;
            mWorkerThread.Name = "ShutdownWorkerThread";
            mWorkerThread.Start();
            mTenMinToClose = false;
            mShutdown = false;
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
                    if (mShutdown)
                    {
                        if (((Comparassion.Minutes <= 10) && !mTenMinToClose) && (SessionManager.Sessions.Count > 0))
                        {
                            SessionManager.BroadcastPacket(HotelClosingMessageComposer.Compose(Comparassion.Minutes));
                            mTenMinToClose = true;
                        }

                        if (TimeSpan.Parse(DateTime.Now.ToShortTimeString()) == CloseHour)
                        {
                            Program.Stop();
                        }
                    }

                    Thread.Sleep(60000);
                }
            }
            catch (ThreadAbortException) { }
        }
    }
}
