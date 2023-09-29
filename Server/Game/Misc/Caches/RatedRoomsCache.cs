using System;
using System.Data;
using System.Collections.Generic;

using Snowlight.Storage;
using Snowlight.Game.Sessions;
using Snowlight.Game.FriendStream;
using System.Linq;

namespace Snowlight.Game.Misc
{
    public class RatedRoomsCache : IDisposable
    {
        private List<uint> mInner;
        private uint mUserId;

        public RatedRoomsCache(SqlDatabaseClient MySqlClient, uint CharacterId)
        {
            mInner = new List<uint>();
            mUserId = CharacterId;

            ReloadCache(MySqlClient);
        }

        public void ReloadCache(SqlDatabaseClient MySqlClient)
        {
            lock (mInner)
            {
                mInner.Clear();

                MySqlClient.SetParameter("userid", mUserId);
                DataTable RatedRooms = MySqlClient.ExecuteQueryTable("SELECT * FROM user_rated_rooms WHERE user_id = @userid LIMIT 1");

                foreach (DataRow Row in RatedRooms.Rows)
                {
                    string[] RoomList = Row["rated_room_ids"].ToString().Split('|');

                    foreach (string StringRoomId in RoomList)
                    {
                        if (uint.TryParse(StringRoomId, out uint RoomId))
                        {
                            if (!mInner.Contains(RoomId))
                            {
                                mInner.Add(RoomId);
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            if (mInner != null)
            {
                mInner.Clear();
                mInner = null;
            }
        }

        public void MarkRoomRated(uint RoomId)
        {
            lock (mInner)
            {
                if (!mInner.Contains(RoomId))
                {
                    mInner.Add(RoomId);
                }

                UpdateInDatabase();
            }
        }

        public bool HasRatedRoom(uint RoomId)
        {
            lock (mInner)
            {
                return mInner.Contains(RoomId);
            }
        }

        public void UpdateInDatabase()
        {
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                MySqlClient.SetParameter("userid", mUserId);
                bool CreateNewRecord = (MySqlClient.ExecuteScalar("SELECT null FROM user_rated_rooms WHERE user_id = @userid LIMIT 1") == null);

                MySqlClient.SetParameter("userid", mUserId);
                MySqlClient.SetParameter("roomlist", string.Join("|", mInner));
                if (CreateNewRecord)
                {
                    MySqlClient.ExecuteNonQuery("INSERT INTO user_rated_rooms (user_id,rated_room_ids) VALUES (@userid,@roomlist)");
                }
                else
                {
                    MySqlClient.ExecuteNonQuery("UPDATE user_rated_rooms SET rated_room_ids = @roomlist WHERE user_id = @userid LIMIT 1");
                }
            }
        }
    }
}
