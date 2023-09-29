using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Snowlight.Storage;
using System.Data;

namespace Snowlight.Game.Items.Wired
{
    public class WiredData
    {
        private uint mItemId;
        private int mType;

        private string mData1;
        private int mData2;
        private int mData3;
        private int mData4;
        private string mData5;
        private int mTime;

        public uint ItemId
        {
            get
            {
                return mItemId;
            }
        }


        public int Type
        {
            get
            {
                return mType;
            }
        }

        public string Data1
        {
            get
            {
                return mData1;
            }

            set
            {
                mData1 = value;
            }
        }

        public int Data2
        {
            get
            {
                return mData2;
            }

            set
            {
                mData2 = value;
            }
        }

        public int Data3
        {
            get
            {
                return mData3;
            }

            set
            {
                mData3 = value;
            }
        }

        public int Data4
        {
            get
            {
                return mData4;
            }

            set
            {
                mData4 = value;
            }
        }

        public string Data5
        {
            get
            {
                return mData5;
            }

            set
            {
                mData5 = value;
            }
        }

        public int Time
        {
            get
            {
                return mTime;
            }

            set
            {
                mTime = value;
            }
        }

        public WiredData(uint ItemId, int Type)
        {
            mItemId = ItemId;
            mType = Type;

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                MySqlClient.SetParameter("id", ItemId);
                DataRow Row = MySqlClient.ExecuteQueryRow("SELECT * FROM wired_items WHERE item_id = @id LIMIT 1");

                if (Row != null)
                {
                    GenerateWiredFromRow(Row);
                }
                else
                {
                    MySqlClient.SetParameter("id", ItemId);
                    MySqlClient.ExecuteNonQuery("INSERT INTO wired_items (item_id, data1, data2, data3, data4, data5, time) VALUES (@id, '','0','0', '0','', '0')");
                    mData1 = "";
                    mData2 = 0;
                    mData3 = 0;
                    mData4 = 0;
                    mData5 = "";
                    mTime = 0;
                }
            }
        }

        public void GenerateWiredFromRow(DataRow Row)
        {
            mData1 = (string)Row["data1"];
            mData2 = (int)Row["data2"];
            mData3 = (int)Row["data3"];
            mData4 = (int)Row["data4"];
            mData5 = (string)Row["data5"];
            mTime = (int)Row["time"];
        }

        public void SynchronizeDatabase(SqlDatabaseClient MySqlClient)
        {
            MySqlClient.SetParameter("id", mItemId);
            MySqlClient.SetParameter("data1", mData1);
            MySqlClient.SetParameter("data2", mData2);
            MySqlClient.SetParameter("data3", mData3);
            MySqlClient.SetParameter("data4", mData4);
            MySqlClient.SetParameter("data5", mData5);
            MySqlClient.SetParameter("time", mTime);

            MySqlClient.ExecuteNonQuery("UPDATE wired_items SET data1 = @data1, data2 = @data2, data3 = @data3, data4 = @data4, data5 = @data5, time = @time WHERE item_id = @id LIMIT 1");
        }
    }
}