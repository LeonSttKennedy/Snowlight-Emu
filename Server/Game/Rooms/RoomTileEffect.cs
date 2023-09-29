using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Snowlight.Game.Items;
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
        private RoomTileEffectType mType;
        private int mEffectId;
        private Vector2 mRootPosition;
        private int mRotation;
        private double mInteractionHeight;
        private uint mQuestData;
        private ItemBehavior mQuestBehavior;

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

        public ItemBehavior QuestBehavior
        {
            get
            {
                return mQuestBehavior;
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
            mQuestBehavior = ItemBehavior.None;
        }

        public RoomTileEffect(RoomTileEffectType Type, int Rotation, Vector2 RootPosition, double InteractionHeight,
            int EffectId = 0, uint QuestData = 0, ItemBehavior Behavior = ItemBehavior.None)
        {
            mType = Type;
            mRotation = Rotation;
            mRootPosition = RootPosition;
            mEffectId = EffectId;
            mInteractionHeight = InteractionHeight;
            mQuestData = QuestData;
            mQuestBehavior = Behavior;
        }
    }
}
