using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Snowlight.Game.Rooms.Games
{
    public enum TagStatus
    {
        None = 0,
        Playing = 1,
        Tagged = 2
    }

    public enum GameType
    {
        None = 0,
        RollerSkating = 1,
        IceSkating = 2,
        BunnyRun = 3,
        BattleBanzai = 4,
        Freeze = 5
    }

    public enum TeamColors
    {
        None = 0,
        Red = 1,
        Green = 2,
        Blue = 3,
        Yellow = 4
    }

    public class GameManager
    {
        private RoomInstance mRoomInstance;

        private Dictionary<uint, DateTime> mRollerStrollerAchievement;

        private Dictionary<uint, int> mSkateboardUserList;

        private Dictionary<uint, DateTime> mIceIceBabyAchievement;
        private Dictionary<uint, RoomActor> mIceTaggedUserList;

        private Dictionary<uint, RoomActor> mBunnyRunTaggedUserList;

        private Dictionary<uint, RoomActor> mPlayingUsers;

        private bool mGameStarted;

        #region Roller Rink
        /// <summary>
        /// This used for obtain Roller Stroller achievement<br />
        /// <br />
        /// Storages uint UserId and DateTime
        /// </summary>
        public Dictionary<uint, DateTime> RollerStrollerAchievement
        { 
            get
            {
                return mRollerStrollerAchievement;
            }

            set
            {
                mRollerStrollerAchievement = value;
            }
        }
        #endregion

        #region Skateboard
        /// <summary>
        /// This used for obtain Skateboard achievements <br />
        /// <br />
        /// Storages uint User Id and int User body/head rotation
        /// </summary>
        public Dictionary<uint, int> SkateboardUserList
        {
            get
            {
                return mSkateboardUserList;
            }

            set
            {
                mSkateboardUserList = value;
            }
        }
        #endregion

        #region Ice Tag
        /// <summary>
        /// This used for obtain Roller Stroller achievement<br />
        /// <br />
        /// Storages uint UserId and DateTime
        /// </summary>
        public Dictionary<uint, DateTime> IceIceBabyAchievement
        {
            get
            {
                return mIceIceBabyAchievement;
            }

            set
            {
                mIceIceBabyAchievement = value;
            }
        }

        /// <summary>
        /// This used for holding actual tagged items <br />
        /// <br />
        /// Storages uint Item Id and RoomActor
        /// </summary>
        public Dictionary<uint, RoomActor> IceTaggedUserList
        {
            get
            {
                return mIceTaggedUserList;
            }

            set
            {
                mIceTaggedUserList = value;
            }
        }
        #endregion

        #region Bunny Run
        /// <summary>
        /// This used for holding actual tagged items <br />
        /// <br />
        /// Storages uint Item Id and RoomActor
        /// </summary>
        public Dictionary<uint, RoomActor> BunnyRunTaggedUserList
        {
            get
            {
                return mBunnyRunTaggedUserList;
            }

            set
            {
                mBunnyRunTaggedUserList = value;
            }
        }
        #endregion

        /// <summary>
        /// This used for holding Users are playing<br />
        /// <br />
        /// Storages ReferenceId
        /// </summary>
        public Dictionary<uint, RoomActor> PlayingUsers
        {
            get
            {
                return mPlayingUsers;
            }

            set
            {
                mPlayingUsers = value;
            }
        }

        public bool GameStarted
        {
            get 
            { 
                return mGameStarted;
            }

            set 
            {
                mGameStarted = value; 
            }
        }

        public GameManager(RoomInstance Instance) 
        {
            mRoomInstance = Instance;

            mRollerStrollerAchievement = new Dictionary<uint, DateTime>();

            mSkateboardUserList = new Dictionary<uint, int>();

            mIceIceBabyAchievement = new Dictionary<uint, DateTime>();
            mIceTaggedUserList = new Dictionary<uint, RoomActor>();

            mBunnyRunTaggedUserList = new Dictionary<uint, RoomActor>();

            mPlayingUsers = new Dictionary<uint, RoomActor>();

            mGameStarted = false;
        }
    }
}
