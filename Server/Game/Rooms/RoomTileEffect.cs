using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Snowlight.Specialized;

namespace Snowlight.Game.Rooms
{
    public enum RoomTileEffectType
    {
        None = 0,
        Sit = 1,
        Lay = 2,
        Effect = 3
    }

    public class RoomTileEffect
    {
        // Tile Effects
        private RoomTileEffectType mType;
        private int mEffectId;
        private Vector2 mRootPosition;
        private int mRotation;
        private double mInteractionHeight;
        private uint mQuestData;

        // Trigger Actions
        private Vector3 mRoomPos;
        private RoomTriggerList mAction;
        private Vector3 mToRoomPos;
        private uint mToRoomId;

        // Tile Effects
        public RoomTileEffectType Type
        {
            get
            {
                return mType;
            }
        }

        public int EffectId
        {
            get
            {
                return mEffectId;
            }
        }

        public Vector2 RootPosition
        {
            get
            {
                return mRootPosition;
            }
        }

        public int Rotation
        {
            get
            {
                return mRotation;
            }
        }

        public double InteractionHeight
        {
            get
            {
                return mInteractionHeight;
            }
        }

        public uint QuestData
        {
            get
            {
                return mQuestData;
            }
        }

        // Trigger Actions
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

        public RoomTileEffect()
        {
            mType = RoomTileEffectType.None;
            mEffectId = -1;
            mRootPosition = new Vector2(0, 0);
            mRotation = 0;
            mInteractionHeight = -1;
            mQuestData = 0;
        }

        public RoomTileEffect(RoomTileEffectType Type, int Rotation, Vector2 RootPosition, double InteractionHeight,
            int EffectId = 0, uint QuestData = 0)
        {
            mType = Type;
            mRotation = Rotation;
            mRootPosition = RootPosition;
            mEffectId = EffectId;
            mInteractionHeight = InteractionHeight;
            mQuestData = QuestData;
        }

        public RoomTileEffect(Vector3 RoomPos, RoomTriggerList Action, Vector3 ToRoomPos, uint ToRoomId)
        {
            // TODO: SEND THE ACTIONS TO A PERFORM UPDATE!!
            mRoomPos = RoomPos;
            mAction = Action;
            mToRoomPos = ToRoomPos;
            mToRoomId = ToRoomId;
        }
    }
}
