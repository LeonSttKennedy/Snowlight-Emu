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
            mBanzaiGameField = new uint[mRoomInstance.Model.Heightmap.SizeX, mRoomInstance.Model.Heightmap.SizeY];
            mBanzaiGameConnectedTiles = new uint[mRoomInstance.Model.Heightmap.SizeX, mRoomInstance.Model.Heightmap.SizeY];
            mBanzaiHelpBlinkCount = new Dictionary<uint, int>();
            mBanzaiHelpFlagsHolder = new Dictionary<uint, string>();
            #endregion

            #region Freeze
            mExitTileItems = new Dictionary<uint, Item>();
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

        public TeamColors Winner(GameType Game)
        {
            int WinnerPoints = 0;
            TeamColors Winner = TeamColors.None;

            ItemBehavior Scoreboard = ItemBehavior.None;
            switch (Game)
            {
                case GameType.BattleBanzai:

                    Scoreboard = ItemBehavior.BanzaiScore;
                    break;

                case GameType.Freeze:

                    Scoreboard = ItemBehavior.FreezeScore;
                    break;
            }

            List<Item> ScoreboardItems = mRoomInstance.GetFloorItems().Where(I => I.Definition.Behavior == Scoreboard).ToList();

            foreach (Item Item in ScoreboardItems)
            {
                int.TryParse(Item.DisplayFlags, out int TeamPoints);

                if (TeamPoints > WinnerPoints)
                {
                    WinnerPoints = TeamPoints;
                    Winner = (TeamColors)Item.Definition.BehaviorData;
                }
            }

            return Winner;
        }

        public int GameScore(GameType Game, bool RoomOwner = false)
        {
            int Points = 0;

            ItemBehavior Scoreboard = ItemBehavior.None;
            switch (Game)
            {
                case GameType.BattleBanzai:

                    Scoreboard = ItemBehavior.BanzaiScore;
                    break;

                case GameType.Freeze:

                    Scoreboard = ItemBehavior.FreezeScore;
                    break;
            }

            List<Item> ScoreboardItems = mRoomInstance.GetFloorItems().Where(I => I.Definition.Behavior == Scoreboard).ToList();

            if (!RoomOwner)
            {
                if (ScoreboardItems.Count > 0)
                {
                    Item Item = ScoreboardItems.Where(I => (TeamColors)I.Definition.BehaviorData == Winner(Game)).FirstOrDefault();

                    if (Item != null)
                    {
                        int.TryParse(Item.DisplayFlags, out Points);
                    }
                }
            }
            else
            {
                Points = ScoreboardItems.GroupBy(I => I.Definition.BehaviorData).Sum(x => int.Parse(x.First().DisplayFlags));
            }

            return Points;
        }

        public void StartGame(GameType Game)
        {
            List<RoomActor> PlayingActors = mRoomInstance.Actors.Where(A => A.PlayingGameType == Game).ToList();

            switch (Game)
            {
                case GameType.BattleBanzai:

                    GenerateGameField();
                    UpdateBanzaiTileState(BanzaiTileStatus.PowerOn);
                    UpdateBanzaiSphere(BanzaiSphere.PowerOn);

                    ChangeBanzaiScoreState(0);

                    break;

                case GameType.Freeze:

                    ResetFreezeTileState();
                    ResetFreezeBlockState();

                    ChangeFreezeScoreState(0);

                    foreach (RoomActor Actor in PlayingActors)
                    {
                        RegisterFreezeActor(Actor);

                        if (EnabledFreezeHeartPowerUp)
                        {
                            Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                            if (Session != null)
                            {
                                Session.SendData(GameLivesComposer.Compose(Actor.Id, mFreezeActorLives[Actor]));
                            }

                            ChangeFreezeScoreState(40, Actor.PlayerTeamColor);
                        }
                    }

                    break;
            }
        }

        public void StopGame(GameType Game)
        {
            List<RoomActor> PlayingActors = mRoomInstance.Actors.Where(A => A.PlayingGameType == Game).ToList();

            switch (Game)
            {
                case GameType.BattleBanzai:

                    foreach (RoomActor Actor in PlayingActors)
                    {
                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            CharacterInfo ActorInfo = CharacterInfoLoader.GetCharacterInfo(MySqlClient, Actor.ReferenceId);

                            Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                            AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_BattleBallPlayer", 1);

                            bool HasOponents = PlayingActors.Select(T => T.PlayerTeamColor).Distinct().Count() > 1;

                            if (HasOponents)
                            {
                                QuestManager.ProgressUserQuest(Session, QuestType.BANZAI_PLAY);
                            }

                            if (Actor.PlayerTeamColor == Winner(Game))
                            {
                                Actor.Wave();

                                if (HasOponents)
                                {
                                    AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_BattleBallWinner", 1);
                                    AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_GamePlayerExperience", GameScore(Game));

                                    QuestManager.ProgressUserQuest(Session, QuestType.BANZAI_WINNER);
                                }
                            }
                        }
                    }
                    
                    break;

                case GameType.Freeze:

                    ResetFreezeTileState();

                    foreach (RoomActor Actor in PlayingActors)
                    {
                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            CharacterInfo ActorInfo = CharacterInfoLoader.GetCharacterInfo(MySqlClient, Actor.ReferenceId);

                            if (ActorInfo.HasLinkedSession)
                            {
                                Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                                AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_FreezePlayer", 1);

                                QuestManager.ProgressUserQuest(Session, QuestType.FREEZE_PLAY);

                                if (EnabledFreezeHeartPowerUp)
                                {
                                    if (Actor.PlayerTeamColor == Winner(Game))
                                    {
                                        Actor.Wave();

                                        AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_FreezeWinner", 1);
                                        AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_GamePlayerExperience", GameScore(Game));
                                        
                                        QuestManager.ProgressUserQuest(Session, QuestType.FREEZE_WINNER);
                                    }
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

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                uint OwnerId = mRoomInstance.Info.OwnerId;
                int GamePointsScored = GameScore(Game, true);

                CharacterInfo OwnerInfo = CharacterInfoLoader.GetCharacterInfo(MySqlClient, OwnerId);

                if (OwnerInfo.HasLinkedSession)
                {
                    Session OwnerSession = SessionManager.GetSessionByCharacterId(OwnerId);

                    AchievementManager.ProgressUserAchievement(MySqlClient, OwnerSession, "ACH_GameAuthorExperience", GamePointsScored);
                }
                else
                {
                    AchievementManager.OfflineProgressUserAchievement(MySqlClient, OwnerId, "ACH_GameAuthorExperience", GamePointsScored);
                }
            }
        }

        public void ForceStopGame(GameType Game)
        {
            bool CanEndGame = false;

            switch (Game)
            {
                case GameType.BattleBanzai:

                    List<Item> BanzaiTileItems = mRoomInstance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.BanzaiTile && I.InteractHelper < 3).ToList();

                    CanEndGame = BanzaiTileItems == null || BanzaiTileItems.Count == 0;
                    break;

                case GameType.Freeze:

                    List<RoomActor> PlayingActors = mRoomInstance.Actors.Where(A => A.PlayingGameType == Game).ToList();

                    bool OnlyRemainingATeam = PlayingActors.Select(T => T.PlayerTeamColor).Distinct().Count() == 1;

                    CanEndGame = OnlyRemainingATeam;
                    break;
            }

            if (CanEndGame)
            {
                Item GameCounter = mRoomInstance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.GameCounter && (GameType)I.Definition.BehaviorData == Game).FirstOrDefault();

                if (GameCounter != null)
                {
                    GameCounter.TimmerRunning = false;
                }

                StopGame(Game);
            }
        }
    }
}
