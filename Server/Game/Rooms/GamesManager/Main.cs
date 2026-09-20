using Microsoft.VisualBasic;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Achievements;
using Snowlight.Game.Characters;
using Snowlight.Game.Items;
using Snowlight.Game.Misc;
using Snowlight.Game.Quests;
using Snowlight.Game.Sessions;
using Snowlight.Specialized;
using Snowlight.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

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

        private Dictionary<GameType, bool> mGameStarted;

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

        public Dictionary<GameType, bool> GameStarted
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

            #region Roller Skating
            mRollerStrollerAchievement = new Dictionary<uint, DateTime>();
            #endregion

            #region Ice Skating
            mIceIceBabyAchievement = new Dictionary<uint, DateTime>();
            mIceTaggedUserList = new Dictionary<uint, RoomActor>();
            #endregion

            #region Bunny Run
            mBunnyRunTaggedUserList = new Dictionary<uint, RoomActor>();
            #endregion

            #region Battle Banzai
            #endregion

            #region Freeze
            mFreezeActorLives = new Dictionary<RoomActor, int>();
            mFreezeActorShieldTime = new Dictionary<RoomActor, int>();
            mFreezeActorFreezedTime = new Dictionary<RoomActor, int>();
            mFreezeActorCooldownSnowballTime = new Dictionary<RoomActor, int>();
            mFreezeActorPowerUp = new Dictionary<RoomActor, FreezePowerUp>();
            mFreezeActorSnowballsPowerUpCount = new Dictionary<RoomActor, int>();
            #endregion

            #region Football
            mFootballGateTeam = new Dictionary<Item, TeamColors>();
            mUserClothesHolder = new Dictionary<uint, string>();
            mUserBallDistance = new Dictionary<uint, int>();
            #endregion

            mPlayingUsers = new Dictionary<uint, RoomActor>();

            mGameStarted = new Dictionary<GameType, bool>();
        }

        public TeamColors GateColor(GameType GameGate, Item Item)
        {
            TeamColors Colors = TeamColors.None;

            switch(GameGate)
            {
                case GameType.BattleBanzai:
                case GameType.Freeze:

                    Colors = (TeamColors)Item.Definition.BehaviorData;

                    break;

                case GameType.Football:

                    Colors = mFootballGateTeam[Item];
                    break;
            }

            return Colors;
        }

        public int GameTeamPlayerCount(GameType Game, TeamColors Color)
        {
            int Count = 0;

            List<RoomActor> Actors = mRoomInstance.Actors.Where(A => A.PlayingGameType == Game && A.PlayerTeamColor == Color).ToList();

            if (Actors != null)
            {
                Count = Actors.Count;
            }

            return Count;
        }

        public void RemoveTeamColor(GameType Game, TeamColors Color)
        {
            List<RoomActor> Actors = mRoomInstance.Actors.Where(A => A.PlayingGameType == Game && A.PlayerTeamColor == Color).ToList();

            int ClearEffect = 0;

            foreach (RoomActor Actor in Actors)
            {
                Actor.PlayingGameType = GameType.None;
                Actor.PlayerTeamColor = TeamColors.None;

                Actor.ApplyEffect(ClearEffect, true);
            }
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

        public void HandleCicleGameActors(GameType Game, Item Counter)
        {
            List<RoomActor> PlayingActors = mRoomInstance.Actors.Where(A => A.PlayingGameType == Game).ToList();

            switch (Game)
            {
                case GameType.Freeze:

                    foreach(TeamColors Colors in Enum.GetValues(typeof(TeamColors)))
                    {
                        if (Colors == TeamColors.None)
                        {
                            continue;
                        }

                        foreach (RoomActor Actor in PlayingActors.Where(C => C.PlayerTeamColor == Colors))
                        {
                            if (mFreezeActorFreezedTime.ContainsKey(Actor))
                            {
                                if (mFreezeActorFreezedTime[Actor] > 0)
                                {
                                    if (Counter.TimmerRunning)
                                    {
                                        mFreezeActorFreezedTime[Actor]--;
                                    }
                                    else
                                    {
                                        mFreezeActorFreezedTime[Actor] = 0;
                                    }
                                }

                                if (mFreezeActorFreezedTime[Actor] == 0)
                                {
                                    int ApplyEffect = 39 + (int)Colors;
                                    Actor.ApplyEffect(ApplyEffect, true);

                                    Actor.UnblockWalking();

                                    mFreezeActorFreezedTime.Remove(Actor);
                                }
                            }

                            if (mFreezeActorShieldTime.ContainsKey(Actor))
                            {
                                if (mFreezeActorShieldTime[Actor] > 0)
                                {
                                    if (Counter.TimmerRunning)
                                    {
                                        mFreezeActorShieldTime[Actor]--;
                                    }
                                    else
                                    {
                                        mFreezeActorShieldTime[Actor] = 0;
                                    }
                                }

                                if (mFreezeActorShieldTime[Actor] == 0)
                                {
                                    int ApplyEffect = 39 + (int)Colors;
                                    Actor.ApplyEffect(ApplyEffect, true);

                                    mFreezeActorShieldTime.Remove(Actor);
                                }
                            }

                            if (mFreezeActorCooldownSnowballTime.ContainsKey(Actor))
                            {
                                if (mFreezeActorCooldownSnowballTime[Actor] > 0)
                                {
                                    if (Counter.TimmerRunning)
                                    {
                                        mFreezeActorCooldownSnowballTime[Actor]--;
                                    }
                                    else
                                    {
                                        mFreezeActorCooldownSnowballTime[Actor] = 0;
                                    }
                                }
                            }
                        }
                    }

                    break;
            }
        }

        public void StartGame(GameType Game)
        {
            List<RoomActor> PlayingActors = mRoomInstance.Actors.Where(A => A.PlayingGameType == Game).ToList();

            switch (Game)
            {
                case GameType.Freeze:

                    List<Item> FreezeItems = new List<Item>();

                    List<Item> FreezeTileItems = mRoomInstance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.FreezeTile).ToList();
                    List<Item> FreezeBlockItems = mRoomInstance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.FreezeBlock).ToList();

                    FreezeItems.AddRange(FreezeTileItems);
                    FreezeItems.AddRange(FreezeBlockItems);

                    foreach (Item Item in FreezeItems)
                    {
                        ItemEventDispatcher.InvokeItemEventHandler(null, Item, mRoomInstance, ItemEventType.InstanceLoaded);
                    }

                    ChangeFreezeScoreState(0);

                    foreach (RoomActor Actor in PlayingActors)
                    {
                        if (!mFreezeActorLives.ContainsKey(Actor))
                        {
                            mFreezeActorLives.Add(Actor, 0);
                        }

                        mFreezeActorLives[Actor] = 3;

                        Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                        if (Session != null)
                        {
                            Session.SendData(GameLivesComposer.Compose(Actor.Id, mFreezeActorLives[Actor]));

                            QuestManager.ProgressUserQuest(Session, QuestType.FREEZE_PLAY);
                        }
                        

                        if (!mFreezeActorCooldownSnowballTime.ContainsKey(Actor))
                        {
                            mFreezeActorCooldownSnowballTime.Add(Actor, 0);
                        }

                        ChangeFreezeScoreState(40, Actor.PlayerTeamColor);
                    }

                    break;
            }
        }

        public void StopGame(GameType Game)
        {
            List<RoomActor> PlayingActors = mRoomInstance.Actors.Where(A => A.PlayingGameType == Game).ToList();

            switch (Game)
            {
                case GameType.Freeze:

                    List<Item> FreezeTileItems = mRoomInstance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.FreezeTile).ToList();

                    foreach (Item Item in FreezeTileItems)
                    {
                        ItemEventDispatcher.InvokeItemEventHandler(null, Item, mRoomInstance, ItemEventType.InstanceLoaded);
                    }

                    List<Item> ScoreboardItems = mRoomInstance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.FreezeScore).ToList();

                    int WinnerPoints = 0;
                    TeamColors Winner = TeamColors.None;

                    foreach (Item Item in ScoreboardItems)
                    {
                        int.TryParse(Item.DisplayFlags, out int TeamPoints);

                        if (TeamPoints > WinnerPoints)
                        {
                            WinnerPoints = TeamPoints;
                            Winner = (TeamColors)Item.Definition.BehaviorData;
                        }
                    }

                    foreach (RoomActor Actor in PlayingActors)
                    {
                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            CharacterInfo ActorInfo = CharacterInfoLoader.GetCharacterInfo(MySqlClient, Actor.ReferenceId);

                            if (ActorInfo.HasLinkedSession)
                            {
                                Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                                AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_FreezePlayer", 1);

                                if (Actor.PlayerTeamColor == Winner)
                                {
                                    Actor.Wave();

                                    AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_FreezeWinner", 1);

                                    QuestManager.ProgressUserQuest(Session, QuestType.FREEZE_WINNER);
                                }
                            }
                        }
                        

                        if (mFreezeActorFreezedTime.ContainsKey(Actor) ||
                            mFreezeActorShieldTime.ContainsKey(Actor))
                        {
                            int ApplyEffect = 39 + (int)Actor.PlayerTeamColor;
                            Actor.ApplyEffect(ApplyEffect, true);

                            if (mFreezeActorFreezedTime.ContainsKey(Actor) &&
                                mFreezeActorFreezedTime[Actor] > 0)
                            {
                                Actor.UnblockWalking();
                            }
                        }

                        UnregisterFreezeActor(Actor);
                    }

                    break;
            }
        }

        public void ForceStopGame(GameType Game)
        {
            List<RoomActor> PlayingActors = mRoomInstance.Actors.Where(A => A.PlayingGameType != Game).ToList();

            switch (Game)
            {
                case GameType.Freeze:

                    bool OnlyRemainingATeam = PlayingActors.Select(T => T.PlayerTeamColor).Distinct().Count() == 1;

                    if (OnlyRemainingATeam)
                    {
                        Item GameCounter = mRoomInstance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.GameCounter && (GameType)I.Definition.BehaviorData == Game).FirstOrDefault();
                        
                        if(GameCounter != null)
                        {
                            GameCounter.TimmerRunning = false;
                        }
                        
                        StopGame(Game);
                    }

                    break;
            }
        }
    }
}
