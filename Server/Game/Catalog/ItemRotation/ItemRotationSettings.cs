using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Snowlight.Storage;

namespace Snowlight.Game.Catalog
{
    public enum RotationType
    {
        Daily = 0,
        Monthly = 1,
    }

    public class ItemRotationSettings
    {
        private uint mId;
        private int mPageId;
        private int mPageToCopy;
        private int mWaitTime;
        private RotationType mType;
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
        public int PageId
        {
            get
            {
                return mPageId;
            }

            set
            {
                mPageId = value;
            }
        }
        public int PageToCopy
        {
            get
            {
                return mPageToCopy;
            }

            set
            {
                mPageToCopy = value;
            }
        }
        public int WaitTime
        {
            get
            {
                return mWaitTime;
            }

            set
            {
                mWaitTime = value;
            }
        }

        public RotationType Type
        {
            get
            {
                return mType;
            }

            set
            {
                mType = value;
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
                DateTime DT;

                switch (mType)
                {
                    default:
                    case RotationType.Daily:
                        
                        DT = UnixTimestamp.GetDateTimeFromUnixTimestamp(mTimestampLastRotation).AddDays(mWaitTime);
                        return new DateTime(DT.Year, DT.Month, DT.Day, 0, 0, 0);

                    case RotationType.Monthly:
                        
                        DT = UnixTimestamp.GetDateTimeFromUnixTimestamp(mTimestampLastRotation).AddMonths(mWaitTime);
                        return new DateTime(DT.Year, DT.Month, DT.Day, 0, 0, 0);
                }
            }
        }

        public bool ExecuteRotation
        {
            get
            {
                bool Execute = false;
                int Compare = DateTime.Compare(DateTime.Now, NextRotation);
                
                if(Compare > -1)
                {
                    Execute = true;
                }

                return Execute || mTimestampLastRotation == 0;
            }
        }

        public ItemRotationSettings(uint Id, int PageId, int PageToCopy, int WaitTime, RotationType Type,
            double TimestampLastRotation, int LastIndex)
        {
            mId = Id;
            mPageId = PageId;
            mPageToCopy = PageToCopy;
            mWaitTime = WaitTime;
            mType = Type;
            mTimestampLastRotation = TimestampLastRotation;
            mLastIndex = LastIndex;
        }

        public void UpdateInDatabase()
        {
            using(SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                MySqlClient.SetParameter("pageid", mPageId);
                MySqlClient.SetParameter("timestamp", UnixTimestamp.GetCurrent());
                MySqlClient.SetParameter("lastindex", mLastIndex);
                MySqlClient.ExecuteNonQuery("UPDATE catalog_items_rotation_settings SET timestamp_last_rotation = @timestamp, last_index = @lastindex WHERE page_id = @pageid LIMIT 1");
            }
        }
    }
}
