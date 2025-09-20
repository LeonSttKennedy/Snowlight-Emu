using Microsoft.VisualBasic.ApplicationServices;
using Snowlight.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Game.Misc
{
    public class HolidayGiftsCache : IDisposable
    {
        private uint mUserId;
        private Dictionary<int, List<uint>> mInner;
        private object mSyncRoot;

        public Dictionary<int, List<uint>> GiftsReceived
        {
            get
            {
                lock (mSyncRoot)
                {
                    Dictionary<int, List<uint>> Copy = new Dictionary<int, List<uint>>();

                    foreach (KeyValuePair<int, List<uint>> Data in mInner)
                    {
                        Copy.Add(Data.Key, Data.Value);
                    }

                    return new Dictionary<int, List<uint>>(Copy);
                }
            }
        }

        public HolidayGiftsCache(SqlDatabaseClient MySqlClient, uint UserId)
        {
            mUserId = UserId;
            mInner = new Dictionary<int, List<uint>>();
            mSyncRoot = new object();

            ReloadCache(MySqlClient);
        }

        public void ReloadCache(SqlDatabaseClient MySqlClient)
        {
            lock (mSyncRoot)
            {
                mInner.Clear();

                MySqlClient.SetParameter("user_id", mUserId);
                DataRow Row = MySqlClient.ExecuteQueryRow("SELECT * FROM user_holiday_gifts WHERE user_id = @user_id");

                if (Row != null)
                {
                    string[] ReceivedGiftsString = Row["gifts_received"].ToString().Split(';');

                    foreach (string ResultString in ReceivedGiftsString)
                    {
                        if (!string.IsNullOrWhiteSpace(ResultString))
                        {
                            string[] YearStrings = ResultString.Split(':');

                            int Year = int.Parse(YearStrings[0]);

                            if (!mInner.ContainsKey(Year))
                            {
                                mInner.Add(Year, new List<uint>());

                                string[] ItemIds = YearStrings[1].Split('|');
                                foreach (string Id in ItemIds)
                                {
                                    if (uint.TryParse(Id, out uint uintId))
                                    {
                                        if (!mInner[Year].Contains(uintId))
                                        {
                                            mInner[Year].Add(uintId);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void AddGift(uint Id)
        {
            lock (mSyncRoot)
            {
                int CurrentYear = DateTime.Now.Year;
                if (!mInner.ContainsKey(CurrentYear))
                {
                    mInner.Add(DateTime.Now.Year, new List<uint>());
                }

                mInner[CurrentYear].Add(Id);

                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    MySqlClient.SetParameter("userid", mUserId);
                    bool CreateNewRecord = (MySqlClient.ExecuteScalar("SELECT null FROM user_holiday_gifts WHERE user_id = @userid LIMIT 1") == null);

                    MySqlClient.SetParameter("userid", mUserId);
                    MySqlClient.SetParameter("giftlist", YearlyGiftsToString());

                    if (CreateNewRecord)
                    {
                        MySqlClient.ExecuteNonQuery("INSERT INTO user_holiday_gifts (user_id,gifts_received) VALUES (@userid,@giftlist)");
                    }
                    else
                    {
                        MySqlClient.ExecuteNonQuery("UPDATE user_holiday_gifts SET gifts_received = @giftlist WHERE user_id = @userid LIMIT 1");
                    }
                }
            }
        }

        public string YearlyGiftsToString()
        {
            return string.Join(";",
                mInner.Select(kvp => $"{kvp.Key}:{string.Join("|", kvp.Value)}"));
        }

        public void Dispose()
        {
            lock (mSyncRoot)
            {
                if (mInner != null)
                {
                    mInner.Clear();
                    mInner = null;
                }
            }
        }
    }
}
