using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Snowlight.Storage;
using Snowlight.Specialized;
using System.Data;

namespace Snowlight.Game.Rooms
{
    public enum RoomTriggerList
    {
        DEFAULT = 0,
        ROLLER = 1,
        TELEPORT = 2
    }
    public class RoomTriggers
    {
        private uint mId;
        private Vector3 mRoomPos;
        private RoomTriggerList mAction;
        private uint mToRoomId;
        private Vector3 mToRoomPos;
        private int mToRoomRotation;

        public uint Id
        {
            get
            {
                return mId;
            }
        }
        public Vector3 RoomPosition
        {
            get
            {
                return mRoomPos;
            }
        }
        public RoomTriggerList Action
        {
            get 
            {
                return mAction;
            }
        }
        public uint ToRoomId
        {
            get
            {
                return mToRoomId;
            }
        }
        public Vector3 ToRoomPosition
        {
            get
            {
                return mToRoomPos;
            }
        }
        public int ToRoomRotation
        {
            get
            {
                return mToRoomRotation;
            }
        }

        public RoomTriggers(uint Id, Vector3 RoomPosition, RoomTriggerList Action,
            Vector3 ToRoomPosition, uint ToRoomId, int ToRoomRotation)
        {
            mId = Id;
            mRoomPos = RoomPosition;
            mAction = Action;
            mToRoomId = ToRoomId;
            mToRoomPos = ToRoomPosition;
            mToRoomRotation = ToRoomRotation;
        }

        public static RoomTriggers GetTrigger(uint TriggerId)
        {
            RoomTriggers Return = null;

            using(SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                MySqlClient.SetParameter("triggerid", TriggerId);
                DataRow Row = MySqlClient.ExecuteQueryRow("SELECT * FROM room_triggers WHERE id = @triggerid");

                RoomTriggerList Trigger = RoomTriggerList.DEFAULT;
                switch ((string)Row["action"])
                {
                    case "roller":
                        Trigger = RoomTriggerList.ROLLER;
                        break;

                    case "teleport":
                        Trigger = RoomTriggerList.TELEPORT;
                        break;
                }

                Return = new RoomTriggers((uint)Row["id"], Vector3.FromString((string)Row["room_pos"]),
                        Trigger, Vector3.FromString((string)Row["to_room_pos"]), (uint)Row["to_room_id"],
                        (int)Row["to_room_dir"]);
            }

            return Return;
        }
    }
}
