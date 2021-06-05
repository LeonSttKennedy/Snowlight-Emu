using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Snowlight.Specialized;

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

        public RoomTriggers(uint Id, Vector3 RoomPosition, RoomTriggerList Action, uint ToRoomId, Vector3 ToRoomPosition, int ToRoomRotation)
        {
            mId = Id;
            mRoomPos = RoomPosition;
            mAction = Action;
            mToRoomId = ToRoomId;
            mToRoomPos = ToRoomPosition;
            mToRoomRotation = ToRoomRotation;
        }
    }
}
