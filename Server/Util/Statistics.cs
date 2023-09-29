using System;
using System.Linq;
using System.Data;
using System.Threading;
using System.Collections.Generic;

using Snowlight.Storage;
using Snowlight.Game.Rooms;
using Snowlight.Game.Catalog;
using Snowlight.Game.Sessions;

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
                    // Getting the true users online count
                    List<string> OnlineUsers = SessionManager.ConnectedUserData.Values.ToList();
                    int TotalUsersOnline = OnlineUsers.Count;

                    // Getting all time players peak
                    string alltimeplayerspeak = MySqlClient.ExecuteScalar("SELECT all_time_player_peak FROM server_statistics LIMIT 1").ToString();
                    int.TryParse(alltimeplayerspeak, out int alltime);
                    
                    // Getting daily player peak
                    string dailyplayerpeak = MySqlClient.ExecuteScalar("SELECT daily_player_peak FROM server_statistics LIMIT 1").ToString();
                    int.TryParse(dailyplayerpeak, out int daily);

                    // Update the number of users online and rooms loaded
                    MySqlClient.SetParameter("totalusersonline", TotalUsersOnline);
                    MySqlClient.SetParameter("totalroomsloaded", RoomManager.RoomInstances.Count);
                    MySqlClient.ExecuteNonQuery("UPDATE server_statistics SET active_connections = @totalusersonline, rooms_loaded = @totalroomsloaded LIMIT 1");

                    // Checking if the total value of active connections is greater than the value registered in the db
                    if (TotalUsersOnline > daily)
                    {
                        // If it's true, lets update
                        MySqlClient.SetParameter("sval", TotalUsersOnline);
                        MySqlClient.ExecuteNonQuery("UPDATE server_statistics SET daily_player_peak = @sval LIMIT 1");
                    }

                    // Checking if the total value of active connections is greater than the value registered in the db
                    if (TotalUsersOnline > alltime) 
                    {
                        // If it's true, lets update
                        MySqlClient.SetParameter("sval", TotalUsersOnline);
                        MySqlClient.ExecuteNonQuery("UPDATE server_statistics SET all_time_player_peak = @sval LIMIT 1");
                    }

                    if (Program.CurrentDay != DateTime.Today)
                    {
                        // If the day changes, this will restart the daily players count with the current number of connected players.
                        MySqlClient.SetParameter("sval", TotalUsersOnline);
                        MySqlClient.ExecuteNonQuery("UPDATE server_statistics SET daily_player_peak = @sval LIMIT 1");

                        foreach(KeyValuePair<uint, List<MarketplaceAvarage>> Var in CatalogManager.MarketplaceAvarages)
                        {
                            foreach(MarketplaceAvarage Avarage in Var.Value.OrderByDescending(L => L.SoldTimeStamp).ToList())
                            {
                                TimeSpan TS = DateTime.Now - Avarage.SoldTimeStamp;

                                if(TS.TotalDays >= 30)
                                {
                                    MySqlClient.SetParameter("id", Avarage.Id);
                                    MySqlClient.ExecuteNonQuery("DELETE FROM catalog_marketplace_data WHERE id = @id LIMIT 1");
                                }
                            }
                        }

                        Program.CurrentDay = DateTime.Today;
                    }
                }

                Thread.Sleep(60 * 1000);
            }
        }
    }
}
