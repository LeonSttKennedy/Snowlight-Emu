using System;
using System.Net;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Runtime.InteropServices;

using Snowlight.Communication.Incoming;
using Snowlight.Config;
using Snowlight.Network;
using Snowlight.Storage;
//using Snowlight.Plugins;
using Snowlight.Util;

using Snowlight.Game;
using Snowlight.Game.Sessions;
using Snowlight.Game.Misc;
using Snowlight.Game.Handlers;
using Snowlight.Game.Moderation;
using Snowlight.Game.Messenger;
using Snowlight.Game.Characters;
using Snowlight.Game.Catalog;
using Snowlight.Game.Items;
using Snowlight.Game.Navigation;
using Snowlight.Game.Rooms;
using Snowlight.Game.Advertisements;
using Snowlight.Game.Rights;
using Snowlight.Game.Bots;
using Snowlight.Game.Infobus;
using Snowlight.Game.Achievements;
using Snowlight.Game.Quests;
using Snowlight.Game.Recycler;
using Snowlight.Game.Pets;
using Snowlight.Game.Music;
using Snowlight.Game.Rooms.Trading;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Groups;
using Snowlight.Game.FriendStream;

namespace Snowlight
{
    public static class Program
    {

        private static bool mAlive;
        private static SnowTcpListener mServer;

        private static DateTime mCurrentDay;
        internal static DateTime ServerStarted;
        // Event Command ;)
        private static DateTime mLastEventHosted;

        /// <summary>
        /// Should be used by all non-worker threads to check if they should remain alive, allowing for safe termination.
        /// </summary>
        public static bool Alive
        {
            get
            {
                return (!Environment.HasShutdownStarted && mAlive);
            }
        }
        public static string PrettyVersion
        {
            get
            {
                return "Snowlight Emulator v1.0-dev (Build " + Constants.ConsoleBuild + ")";
            }
        }
        public static DateTime CurrentDay
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

        // Event Command ;)
        public static DateTime LastEventHosted
        {
            get
            {
                return mLastEventHosted;
            }

            set
            {
                mLastEventHosted = value;
            }
        }

        private delegate bool ConsoleCtrlHandlerDelegate(int sig);

        [DllImport("Kernel32")]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]

        private static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandlerDelegate handler, bool add);

        static ConsoleCtrlHandlerDelegate _consoleCtrlHandler;

        public static void Main(string[] args)
        {
            _consoleCtrlHandler += s =>
            {
                Stop();
                return false;
            };
            SetConsoleCtrlHandler(_consoleCtrlHandler, true);

            mAlive = true;
            ServerStarted = DateTime.Now;

            // Set up basic output, configuration, etc
            ConfigManager.Initialize(Constants.DataFileDirectory + "\\server-main.cfg");

            Output.InitializeStream(true, (OutputLevel)ConfigManager.GetValue("output.verbositylevel"));
            Output.WriteLine("Initializing Snowlight...");

            // Process args
            foreach (string arg in args)
            {
                Output.WriteLine("Command line argument: " + arg);
                Input.ProcessInput(arg.Split(' '));
            }

            try
            {
                // Initialize and test database
                Output.WriteLine("Initializing MySQL manager...");
                SqlDatabaseManager.Initialize();

                // Initialize network components
                Output.WriteLine("Setting up server listener on port " + (int)ConfigManager.GetValue("net.bind.port") + "...");
                mServer = new SnowTcpListener(new IPEndPoint(IPAddress.Any, (int)ConfigManager.GetValue("net.bind.port")),
                    (int)ConfigManager.GetValue("net.backlog"), new OnNewConnectionCallback(
                        SessionManager.HandleIncomingConnection));

                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    PerformDatabaseCleanup(MySqlClient, 1);

                    Output.WriteLine("Initializing game components and workers...");

                    // Some settings on database
                    ServerSettings.Initialize(MySqlClient);
                    ExternalTexts.Initialize(MySqlClient);

                    // Core
                    DataRouter.Initialize();

                    // Sessions, characters
                    Handshake.Initialize();
                    GlobalHandler.Initialize();
                    SessionManager.Initialize();
                    CharacterInfoLoader.Initialize();
                    RightsManager.Initialize(MySqlClient);
                    SingleSignOnAuthenticator.Initialize();

                    // Room management and navigator
                    RoomManager.Initialize(MySqlClient);
                    RoomInfoLoader.Initialize();
                    RoomHandler.Initialize();
                    RoomItemHandler.Initialize();
                    Navigator.Initialize(MySqlClient);

                    // Help and moderation
                    HelpTool.Initialize(MySqlClient);
                    ModerationPresets.Initialize(MySqlClient);
                    ModerationTicketManager.Initialize(MySqlClient);
                    ModerationHandler.Initialize();
                    ModerationBanManager.Initialize(MySqlClient);

                    // Catalog, pets and items
                    ItemDefinitionManager.Initialize(MySqlClient);
                    CatalogManager.Initialize(MySqlClient);
                    VoucherManager.Initialize(MySqlClient);
                    CatalogSubGifts.Initialize(MySqlClient);
                    CatalogPurchaseHandler.Initialize();
                    Inventory.Initialize();
                    ItemEventDispatcher.Initialize();
                    PetDataManager.Initialize(MySqlClient);

                    // Messenger
                    MessengerHandler.Initialize();

                    // Friend Stream
                    FriendStreamHandler.Initialize();

                    // Achievements and quests
                    AchievementManager.Initialize(MySqlClient);
                    QuestManager.Initialize(MySqlClient);

                    // Misc/extras
                    CrossdomainPolicy.Initialize("Data\\crossdomain.xml");
                    GroupManager.Initialize(MySqlClient);
                    InfobusManager.Initialize();
                    RoomPollManager.Initialize(MySqlClient);
                    UserStuffWorker.Initialize();
                    BotManager.Initialize(MySqlClient);
                    InterstitialManager.Initialize(MySqlClient);
                    ChatEmotions.Initialize();
                    ChatWordFilter.Initialize(MySqlClient);
                    CommandManager.Initialize();
                    EffectsCacheWorker.Initialize();
                    RecyclerManager.Initialize(MySqlClient);
                    DrinkSetManager.Initialize(MySqlClient);
                    SongManager.Initialize();
                    TradeHandler.Initialize();
                    RandomGenerator.Initialize();
                    DailyStuffWorker.Initialize();
                    StatisticsSyncUtil.Initialize();
                    ShutdownCommandWorker.Initialize();
                    SubscriptionOfferManager.Initialize(MySqlClient);

                    // Polish
                    WarningSurpressors.Initialize();
                }
            }
            catch (Exception e)
            {
                HandleFatalError("Could not initialize Snowlight: " + e.Message + "\n" + e.StackTrace);
                return;
            }

            // Init complete
            TimeSpan TimeSpent = DateTime.Now - ServerStarted;

            Output.WriteLine("The server has initialized successfully (" + Math.Round(TimeSpent.TotalSeconds, 2) + " seconds). Ready for connections.", OutputLevel.Notification);
            Output.WriteLine("Press the ENTER key for command input. After type 'HELP' for command list.", OutputLevel.Notification);

            Console.Beep();
            Input.Listen(); // This will make the main thread process console while Program.Alive.
        }

        private static void PerformDatabaseCleanup(SqlDatabaseClient MySqlClient, int ServerStatus)
        {
            Output.WriteLine((ServerStatus == 1 ? "Resetting database counters and statistics..." : "Performed database cleanup..."));

            MySqlClient.ExecuteNonQuery("UPDATE rooms SET current_users = 0");

            MySqlClient.SetParameter("timestamp", UnixTimestamp.GetCurrent());
            MySqlClient.ExecuteNonQuery("UPDATE room_visits SET timestamp_left = @timestamp WHERE timestamp_left = 0");
            
            MySqlClient.ExecuteNonQuery("UPDATE characters SET auth_ticket = '', online = '0'");
            
            MySqlClient.SetParameter("status", ServerStatus.ToString());
            MySqlClient.SetParameter("version", PrettyVersion);
            MySqlClient.ExecuteNonQuery("UPDATE server_statistics SET server_status = @status, active_connections = '0', rooms_loaded = '0', server_ver = @version LIMIT 1");
        }
        public static void HandleFatalError(string Message)
        {
            Output.WriteLine(Message, OutputLevel.CriticalError);
            Output.WriteLine("Cannot proceed; press any key to stop the server.", OutputLevel.CriticalError);

            Console.ReadKey(true);

            Stop();
        }

        public static void Stop()
        {
            Output.WriteLine("Stopping Snowlight...");

            SessionManager.BroadcastPacket(NotificationMessageComposer.Compose(ExternalTexts.GetValue("server_shutdown_message")));
            Thread.Sleep(2500);

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                PerformDatabaseCleanup(MySqlClient, 0);
            }

            mAlive = false; // Will destroy any threads looping for Program.Alive.

            SqlDatabaseManager.Uninitialize();

            mServer.Dispose();
            mServer = null;

            Output.WriteLine("Bye!");

            Thread.Sleep(1250);
            Environment.Exit(0);
        }
    }
}
