using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Snowlight.Storage;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;


namespace Snowlight.Game.Misc
{
    class StatusCommand : IChatCommand
    {
        [DllImport("kernel32.dll")]
        private static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);
        private const int PROCESSOR_ARCHITECTURE_AMD64 = 9;
        private const int PROCESSOR_ARCHITECTURE_IA64 = 6;
        private const int PROCESSOR_ARCHITECTURE_INTEL = 0;
        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_INFO
        {
            public short wProcessorArchitecture;
            public short wReserved;
            public int dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public IntPtr dwActiveProcessorMask;
            public int dwNumberOfProcessors;
            public int dwProcessorType;
            public int dwAllocationGranularity;
            public short wProcessorLevel;
            public short wProcessorRevision;
        }
        public static ProcessorArchitecture GetProcessorArchitecture()
        {
            SYSTEM_INFO si = new SYSTEM_INFO();
            GetNativeSystemInfo(ref si);
            switch (si.wProcessorArchitecture)
            {
                case PROCESSOR_ARCHITECTURE_AMD64: return ProcessorArchitecture.Amd64;
                case PROCESSOR_ARCHITECTURE_IA64: return ProcessorArchitecture.IA64;
                case PROCESSOR_ARCHITECTURE_INTEL: return ProcessorArchitecture.X86;
                default:
                    return ProcessorArchitecture.None; // that's weird :-)
            }
        }
        static ulong GetTotalMemoryInBytes()
        {
            return new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
        }


        public string PermissionRequired
        {
            get { return "club_regular"; }
        }

        public string Parameters
        { 
            get { return ""; }
        }

        public string Description
        {
            get { return "Shows to you the stats from the server."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            // Total online players peak
            int alltime = 0;
            int daily = 0;
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                string alltimeplayerpeak = MySqlClient.ExecuteScalar("SELECT all_time_player_peak FROM server_statistics LIMIT 1").ToString();
                int.TryParse(alltimeplayerpeak, out alltime);

                string dailyplayerpeak = MySqlClient.ExecuteScalar("SELECT daily_player_peak FROM server_statistics LIMIT 1").ToString();
                int.TryParse(dailyplayerpeak, out daily);
            }

            // Total users online
            List<string> OnlineUsers = SessionManager.ConnectedUserData.Values.ToList();

            // Total server online time
            DateTime Now = DateTime.Now;
            TimeSpan Uptime = Now - Program.ServerStarted;

            string[] Args = { Uptime.Days.ToString(), Uptime.Hours.ToString(), Uptime.Minutes.ToString(), Uptime.Seconds.ToString(),
                            OnlineUsers.Count.ToString(), RoomManager.RoomInstances.Count.ToString(), daily.ToString(), alltime.ToString(),
                            GetProcessorArchitecture().ToString().ToLower(), Environment.ProcessorCount.ToString(),
                            Math.Round(GetTotalMemoryInBytes() / 1024d / 1024d / 1024d, 2).ToString()
                        };

            Session.SendData(NotificationMessageComposer.Compose(string.Concat(new object[]
            {
                            "[SERVER]",
                            "\nUptime: " + Args[0] + " day(s), " + Args[1] + " hour(s), " + Args[2] + " minute(s) and " + Args[3] + " second(s)",
                            "\nThere are " + Args[4]  + " user(s) online",
                            "\nThere are " + Args[5] + " room(s) loaded",
                            "\nDaily player peak: " + Args[6],
                            "\nAll time player peak: " + Args[7],
                            "\n\n",
                            "[SYSTEM]",
                            "\nCPU architecture: " + Args[8],
                            "\nCPU cores: "+ Args[9],
                            "\nMemory usage: " + Args[10] + " MB"
            })));
        }
    }
}
