using Snowlight.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Snowlight.Game.Items
{
    public enum ItemType
    {
        FloorItem = 0,
        WallItem = 1,
        AvatarEffect = 2,
        ClubSubscription = 3,
        Pet = 4
    }

    public enum ItemStackingBehavior
    {
        Normal = 0,
        Initiator = 1,
        Terminator = 2,
        Ignore = 3,
        InitiateAndTerminate = 4
    }

    public enum ItemWalkableMode
    {
        Never = 0,
        Limited = 1,
        Always = 2
    }

    public class ItemDefinition
    {
        private uint mId;
        private uint mSpriteId;
        private string mName;
        private string mPublicName;
        private ItemType mType;
        private ItemBehavior mBehavior;
        private int mBehaviorData;
        private ItemStackingBehavior mStackingBehavior;
        private ItemWalkableMode mWalkable;
        private int mRoomLimit;

        private int mSizeX;
        private int mSizeY;
        private float mHeight;
        private List<float> mAdjustableHeight;

        private bool mAllowRecycle;
        private bool mAllowTrade;
        private bool mAllowSell;
        private bool mAllowGift;
        private bool mAllowInventoryStack;

        private string mPetStatussesInteraction;
        private List<int> mPetTypeInteractors;

        public uint Id
        {
            get
            {
                return mId;
            }
        }

        public uint SpriteId
        {
            get
            {
                return mSpriteId;
            }
        }

        public string Name
        {
            get
            {
                return mName;
            }
        }

        public string PublicName
        {
            get
            {
                return mPublicName;
            }
        }

        public ItemType Type
        {
            get
            {
                return mType;
            }
        }

        public string TypeLetter
        {
            get
            {
                switch (mType)
                {
                    default:
                    case ItemType.FloorItem:

                        return "s";

                    case ItemType.WallItem:

                        return "i";

                    case ItemType.AvatarEffect:

                        return "e";

                    case ItemType.ClubSubscription:

                        return "h";

                    /*case ItemType.Pet:

                        return "p";*/
                }
            }
        }

        public ItemBehavior Behavior
        {
            get
            {
                return mBehavior;
            }
        }

        public int BehaviorData
        {
            get
            {
                return mBehaviorData;
            }
        }

        public ItemStackingBehavior StackingBehavior
        {
            get
            {
                return mStackingBehavior;
            }
        }

        public ItemWalkableMode Walkable
        {
            get
            {
                return mWalkable;
            }
        }

        public int RoomLimit
        {
            get
            {
                return mRoomLimit;
            }
        }

        public int SizeX
        {
            get
            {
                return mSizeX;
            }
        }

        public int SizeY
        {
            get
            {
                return mSizeY;
            }
        }

        public float Height
        {
            get
            {
                return mHeight;
            }
        }
        public List<float> AdjustableHeight
        {
            get
            {
                return mAdjustableHeight;
            }
        }
        public bool AllowRecycle
        {
            get
            {
                return mAllowRecycle;
            }
        }

        public bool AllowTrade
        {
            get
            {
                return mAllowTrade;
            }
        }

        public bool AllowSell
        {
            get
            {
                return mAllowSell;
            }
        }

        public bool AllowGift
        {
            get
            {
                return mAllowGift;
            }
        }

        public bool AllowInventoryStack
        {
            get
            {
                return mAllowInventoryStack;
            }
        }

        public bool Usable
        {
            get
            {
                return (mBehavior != ItemBehavior.Bed &&
                    mBehavior != ItemBehavior.Dice && mBehavior != ItemBehavior.HoloDice &&
                    mBehavior != ItemBehavior.LoveShuffler && mBehavior != ItemBehavior.HabboWheel &&
                    mBehavior != ItemBehavior.StaticItem && mBehavior != ItemBehavior.BallToy &&
                    mBehavior != ItemBehavior.PetNest && mBehavior != ItemBehavior.PetFood &&
                    mBehavior != ItemBehavior.MonkeyToy && mBehavior != ItemBehavior.SpiderToy &&
                    BehaviorData > 1) ||
                    (mBehavior == ItemBehavior.PetNest && BehaviorData == 2);
            }
        }

        public string PetStatusses
        {
            get
            {
                return mPetStatussesInteraction;
            }
        }

        public ReadOnlyCollection<int> PetTypeInteractors
        {
            get
            {
                List<int> Copy = new List<int>();
                Copy.AddRange(mPetTypeInteractors);
                return Copy.AsReadOnly();
            }
        }

        public ItemDefinition(uint Id, uint SpriteId, string Name, string PublicName, ItemType Type, ItemBehavior Behavior, int BehaviorData,
            ItemStackingBehavior StackingBehavior, ItemWalkableMode Walkable, int RoomLimit, int SizeX, int SizeY, float Height, string AdjustableHeight,
            bool AllowRecycle, bool AllowTrade, bool AllowSell, bool AllowGift, bool AllowInventoryStack)
        {
            mId = Id;
            mSpriteId = SpriteId;
            mName = Name;
            mPublicName = PublicName;
            mType = Type;
            mBehavior = Behavior;
            mBehaviorData = BehaviorData;
            mStackingBehavior = StackingBehavior;
            mWalkable = Walkable;
            mRoomLimit = RoomLimit;
            mSizeX = SizeX;
            mSizeY = SizeY;
            mHeight = Height;
            mAdjustableHeight = new List<float>();
            mAllowRecycle = AllowRecycle;
            mAllowTrade = AllowTrade;
            mAllowSell = AllowSell;
            mAllowGift = AllowGift;
            mAllowInventoryStack = AllowInventoryStack;

            mPetStatussesInteraction = string.Empty;
            mPetTypeInteractors = null;

            string[] splitedHeights = AdjustableHeight.Split('|');

            for(int i = 0; i < splitedHeights.Length; i++)
            {
                mAdjustableHeight.Add(float.Parse(splitedHeights[i], CultularUtils.NumberFormatInfo));
            }
        }

        public void SetPetStatussesInteraction(string Status)
        {
            mPetStatussesInteraction = Status;
        }

        public void SetPetTypeInteractors(List<int> PetTypeList)
        {
            mPetTypeInteractors = PetTypeList;
        }
    }
}
