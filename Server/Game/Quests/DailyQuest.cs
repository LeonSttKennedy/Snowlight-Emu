using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Snowlight.Storage;

namespace Snowlight.Game.Catalog
{
    public class DailyQuest
    {
        private uint mId;
        private double mTimestampLastRotation;
        private int mLastIndex;

        public uint Id
        {
            get
            {
                return mId;
            }

            set
            {
                mId = value;
            }
        }

        public double TimestampLastRotation
        {
            get
            {
                return mTimestampLastRotation;
            }

            set
            {
                mTimestampLastRotation = value;
            }
        }

        public int LastIndex
        {
            get
            {
                return mLastIndex;
            }

            set
            {
                mLastIndex = value;
            }
        }

        public DateTime NextRotation
        {
            get
            {
                DateTime DT = UnixTimestamp.GetDateTimeFromUnixTimestamp(mTimestampLastRotation).AddDays(1);
                return new DateTime(DT.Year, DT.Month, DT.Day, 0, 0, 0);
            }
        }

        public TimeSpan NextQuestAvailable
        {
            get
            {
                TimeSpan TS = NextRotation - DateTime.Now;
                return TS;
            }
        }

        public bool ExecuteRotation
        {
            get
            {
                bool Execute = false;
                int Compare = DateTime.Compare(DateTime.Now, NextRotation);

                if (Compare > -1)
                {
                    Execute = true;
                }

                return Execute || mTimestampLastRotation == 0;
            }
        }

        public DailyQuest(uint Id, double TimestampLastRotation, int LastIndex)
        {
            mId = Id;
            mTimestampLastRotation = TimestampLastRotation;
            mLastIndex = LastIndex;
        }

        public void UpdateInDatabase()
        {
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                MySqlClient.SetParameter("id", mId);
                MySqlClient.SetParameter("timestamp", UnixTimestamp.GetCurrent());
                MySqlClient.SetParameter("lastindex", mLastIndex);
                MySqlClient.ExecuteNonQuery("UPDATE quests_daily_rotation SET timestamp_last_rotation = @timestamp, last_index = @lastindex WHERE id = @id LIMIT 1");
            }
        }

        public void DisableInDatabase()
        {
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                MySqlClient.SetParameter("id", mId);
                MySqlClient.SetParameter("disabled", "0");
                MySqlClient.ExecuteNonQuery("UPDATE quests_daily_rotation SET timestamp_last_rotation = '0', last_index = '-1', enabled = @disabled LIMIT 1");

                MySqlClient.SetParameter("disabled", "0");
                MySqlClient.ExecuteNonQuery("UPDATE quests SET enabled = @disabled WHERE category = 'daily'");
            }
        }
    }
}
