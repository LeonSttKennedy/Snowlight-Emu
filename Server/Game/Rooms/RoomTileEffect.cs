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
        private uint mId;
        private Vector3 mRoomPos;
        private RoomTriggerList mAction;
        private Vector3 mToRoomPos;
        private uint mToRoomId;
        private int mToRoomRotation;

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
        public uint TriggerId
        {
            get
            {
                return mId;
            }
        }
        public Vector3 TriggerRoomPosition
        {
            get
            {
                return mRoomPos;
            }
        }
        public RoomTriggerList TriggerAction
        {
            get
            {
                return mAction;
            }
        }
        public uint TriggerToRoomId
        {
            get
            {
                return mToRoomId;
            }
        }
        public Vector3 TriggerToRoomPosition
        {
            get
            {
                return mToRoomPos;
            }
        }
        public int TriggerToRoomRotation
        {
            get
            {
                return mToRoomRotation;
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

        public RoomTileEffect(uint TriggerId, Vector3 TriggerRoomPos, RoomTriggerList TriggerAction, 
                        Vector3 TriggerToRoomPos, uint TriggerToRoomId, int TriggerRoomRotation)
        {
            mId = TriggerId;
            mRoomPos = TriggerRoomPos;
            mAction = TriggerAction;
            mToRoomPos = TriggerToRoomPos;
            mToRoomId = TriggerToRoomId;
            mToRoomRotation = TriggerRoomRotation;
        }
    }
}
