using Snowlight.Specialized;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Game.Rooms
{
    public enum RoomTiggerList
    {
        DEFAULT = 0,
        ROLLER = 1,
        TELEPORT = 2
    }
    public class RoomTriggers
    {
        private uint mId;
        private uint mRoomId;
        private Vector3 mRoomPos;
        private RoomTiggerList mAction;
        private uint mToRoomId;
        private Vector3 mToRoomPos;

        public uint Id
        {
            get
            {
                return mId;
            }
        }
        public uint RoomId
        {
            get
            {
                return mRoomId;
            }
        }
        public Vector3 RoomPosition
        {
            get
            {
                return mRoomPos;
            }
        }
        public RoomTiggerList Action
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

        public RoomTriggers(uint Id, uint RoomId, Vector3 RoomPosition, RoomTiggerList Action, uint ToRoomId, Vector3 ToRoomPosition)
        {
            mId = Id;
            mRoomId = RoomId;
            mRoomPos = RoomPosition;
            mAction = Action;
            mToRoomId = ToRoomId;
            mToRoomPos = ToRoomPosition;
        }
    }
}
