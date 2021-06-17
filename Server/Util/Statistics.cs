using System;
using System.Threading;

using Snowlight.Storage;
using Snowlight.Game.Sessions;
using System.Data;

namespace Snowlight.Util
{
    public static class StatisticsSyncUtil
    {
        public static void Initialize()
        {
            Thread Thread = new Thread(new ThreadStart(ProcessThread));
            Thread.Priority = ThreadPriority.Lowest;
            Thread.Name = "StatisticsDbSyncThread";
            Thread.Start();
        }

        private static void ProcessThread()
        {
            while (Program.Alive)
            {
                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    // Getting all time players peak
                    string alltimeplayerspeak = MySqlClient.ExecuteScalar("SELECT sval FROM server_statistics WHERE skey = 'all_time_player_peak' LIMIT 1").ToString();
                    int alltime = 0;
                    int.TryParse(alltimeplayerspeak, out alltime);
                    
                    // Getting daily player peak
                    string dailyplayerpeak = MySqlClient.ExecuteScalar("SELECT sval FROM server_statistics WHERE skey = 'daily_player_peak' LIMIT 1").ToString();
                    int daily = 0;
                    int.TryParse(dailyplayerpeak, out daily);

                    // Update the number of users online
                    MySqlClient.SetParameter("skey", "active_connections");
                    MySqlClient.SetParameter("sval", SessionManager.ActiveConnections);
                    MySqlClient.ExecuteNonQuery("UPDATE server_statistics SET sval = @sval WHERE skey = @skey LIMIT 1");

                    // Checking if the total value of active connections is greater than the value registered in the db
                    if (SessionManager.ActiveConnections > daily)
                    {
                        // If it's true, lets update
                        MySqlClient.SetParameter("skey", "daily_player_peak");
                        MySqlClient.SetParameter("sval", SessionManager.ActiveConnections);
                        MySqlClient.ExecuteNonQuery("UPDATE server_statistics SET sval = @sval WHERE skey = @skey LIMIT 1");
                    }

                    // Checking if the total value of active connections is greater than the value registered in the db
                    if (SessionManager.ActiveConnections > alltime) 
                    {
                        // If it's true, lets update
                        MySqlClient.SetParameter("skey", "all_time_player_peak");
                        MySqlClient.SetParameter("sval", SessionManager.ActiveConnections);
                        MySqlClient.ExecuteNonQuery("UPDATE server_statistics SET sval = @sval WHERE skey = @skey LIMIT 1");
                    }

                    // If day changed this will restart user daily count
                    if (Program.CurrentDay != DateTime.Today)
                    {
                        // If day's change, lets update
                        MySqlClient.SetParameter("skey", "daily_player_peak");
                        MySqlClient.SetParameter("sval", 0);
                        MySqlClient.ExecuteNonQuery("UPDATE server_statistics SET sval = @sval WHERE skey = @skey LIMIT 1");
                        Program.CurrentDay = DateTime.Today;
                    }
                }

                Thread.Sleep(60 * 1000);
            }
        }
    }
}
