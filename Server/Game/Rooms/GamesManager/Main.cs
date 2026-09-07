using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.Collections.Generic;

using Snowlight.Game.Misc;
using Snowlight.Game.Items;
using Snowlight.Specialized;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Rooms
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
        Freeze = 5,
        Football = 6,
        IceHockey = 7
    }

    public enum TeamColors
    {
        None = 0,
        Red = 1,
        Green = 2,
        Blue = 3,
        Yellow = 4
    }

    public partial class GameManager
    {
        private RoomInstance mRoomInstance;

        private Dictionary<uint, DateTime> mRollerStrollerAchievement;

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

            mIceIceBabyAchievement = new Dictionary<uint, DateTime>();
            mIceTaggedUserList = new Dictionary<uint, RoomActor>();

            mBunnyRunTaggedUserList = new Dictionary<uint, RoomActor>();

            mFootballGateTeam = new Dictionary<Item, TeamColors>();
            mFootballActorTeam = new Dictionary<TeamColors, List<RoomActor>>();
            mUserClothesHolder = new Dictionary<uint, string>();
            mUserBallDistance = new Dictionary<uint, int>();

            mPlayingUsers = new Dictionary<uint, RoomActor>();

            mGameStarted = false;
        }

        public TeamColors GateColor(GameType GameGate, Item Item)
        {
            TeamColors Colors = TeamColors.None;

            switch(GameGate)
            {
                case GameType.BattleBanzai:
                case GameType.Freeze:

                    // Needs to be implemented

                    break;

                case GameType.Football:

                    Colors = mFootballGateTeam[Item];
                    break;
            }

            return Colors;
        }

        public void AddUserToTeam(GameType Type, TeamColors Color, RoomActor Actor)
        {
            switch (Type)
            {
                case GameType.Football:

                    if (!mFootballActorTeam.ContainsKey(Color))
                    {
                        mFootballActorTeam.Add(Color, new List<RoomActor>());
                    }

                    if (!mFootballActorTeam[Color].Contains(Actor))
                    {
                        mFootballActorTeam[Color].Add(Actor);
                    }

                    break;

            }
        }

        public void RemoveOrAlterUserTeam(GameType Type, TeamColors NewColor, RoomActor Actor)
        {
            bool Remove = NewColor == TeamColors.None;

            switch (Type)
            {
                case GameType.Football:

                    if (!Remove)
                    {
                        if (!mFootballActorTeam.ContainsKey(NewColor))
                        {
                            mFootballActorTeam.Add(NewColor, new List<RoomActor>());
                        }
                        else
                        {
                            mFootballActorTeam[NewColor].Add(Actor);
                        }
                    }

                    foreach (KeyValuePair<TeamColors, List<RoomActor>> KeyValuePair in mFootballActorTeam)
                    {
                        if (KeyValuePair.Value.Contains(Actor) &&
                            (KeyValuePair.Key != NewColor || Remove))
                        {
                            KeyValuePair.Value.Remove(Actor);
                            break;
                        }
                    }

                    break;
            }
        }

        public TeamColors GetUserTeam(RoomActor Actor)
        {
            TeamColors ActorTeam = TeamColors.None;

            foreach (KeyValuePair<TeamColors, List<RoomActor>> KeyValuePair in mFootballActorTeam)
            {
                if (KeyValuePair.Value.Contains(Actor))
                {
                    ActorTeam = KeyValuePair.Key;
                    break;
                }
            }
            
            return ActorTeam;
        }

        public TeamColors GetOpponentColor(GameType Game, TeamColors Color)
        {
            TeamColors OpponentTeam = TeamColors.None;

            switch (Game)
            {
                case GameType.Football:

                    foreach (KeyValuePair<Item, TeamColors> KeyValuePair in FootballGateTeam)
                    {
                        if (KeyValuePair.Value == Color)
                        {
                            continue;
                        }

                        OpponentTeam = KeyValuePair.Value;
                        break;
                    }

                    break;

                case GameType.IceHockey:

                    OpponentTeam = (TeamColors)((int)Color ^ 2);

                    break;
            }

            return OpponentTeam;
        }

        public TeamColors GameColorByInt(int Number)
        {
            return (TeamColors)Number;
        }
    }
}
