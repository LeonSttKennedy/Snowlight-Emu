using Snowlight.Communication;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Achievements;
using Snowlight.Game.Characters;
using Snowlight.Game.Items;
using Snowlight.Game.Quests;
using Snowlight.Game.Sessions;
using Snowlight.Specialized;
using Snowlight.Storage;
using Snowlight.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Game.Rooms
{
    public enum FreezePowerUp
    {
        None = 0,
        BlueArrow = 1,              // Hits a longer row of tiles.
        Snowballs = 2,              // Let a player throw two snowballs at a time. The effect can be doubled if a player grabs more of these power ups.
        GreenArrow = 3,             // Everyone that stands diagonal of where the player threw the snowball will be affected (in an X shape).
        OrangeSnowball = 4,         // Affects a larger area of tiles.
        Heart = 5,                  // Gives the player an extra life.
        Shield = 6                  // Gives the player invincibility for a short period of time. (10 seconds)
    }

    public enum FreezeFirePower
    {
        None = 0,
        LightBall = 1,
        MediumBall = 2,
        HighBall = 3,
        VeryHighBall = 4,
        NoEffect = 5,           // Literally, this effect not be programed by sulake
        OrangeBall = 6
    }

    public partial class GameManager
    {
        private Dictionary<RoomActor, int> mFreezeActorLives;
        private Dictionary<RoomActor, int> mFreezeActorShieldTime;
        private Dictionary<RoomActor, int> mFreezeActorFreezedTime;
        private Dictionary<RoomActor, int> mFreezeActorCooldownSnowballTime;
        private Dictionary<RoomActor, FreezePowerUp> mFreezeActorPowerUp;
        private Dictionary<RoomActor, int> mFreezeActorSnowballsPowerUpCount;

        public bool FreezeStarted
        {
            get
            {
                return mGameStarted.ContainsKey(GameType.Freeze) ? mGameStarted[GameType.Freeze] : false;
            }
        }

        public Dictionary<RoomActor, int> FreezeActorLives
        {
            get
            {
                return mFreezeActorLives;
            }

            set
            {
                mFreezeActorLives = value;
            }
        }

        public Dictionary<RoomActor, int> FreezeActorShieldTime
        {
            get
            {
                return mFreezeActorShieldTime;
            }

            set
            {
                mFreezeActorShieldTime = value;
            }
        }

        public Dictionary<RoomActor, int> FreezeActorFreezedTime
        {
            get
            {
                return mFreezeActorFreezedTime;
            }

            set
            {
                mFreezeActorFreezedTime = value;
            }
        }

        public Dictionary<RoomActor, int> FreezeActorCooldownSnowballTime
        {
            get
            {
                return mFreezeActorCooldownSnowballTime;
            }

            set
            {
                mFreezeActorCooldownSnowballTime = value;
            }
        }

        public Dictionary<RoomActor, FreezePowerUp> FreezeActorPowerUp
        {
            get
            {
                return mFreezeActorPowerUp;
            }

            set
            {
                mFreezeActorPowerUp = value;
            }
        }

        public Dictionary<RoomActor, int> FreezeActorSnowballsPowerUpCount
        {
            get
            {
                return mFreezeActorSnowballsPowerUpCount;
            }

            set
            {
                mFreezeActorSnowballsPowerUpCount = value;
            }
        }

        public void UnregisterFreezeActor(RoomActor Actor)
        {
            mFreezeActorFreezedTime.Remove(Actor);
            mFreezeActorLives.Remove(Actor);
            mFreezeActorPowerUp.Remove(Actor);
            mFreezeActorShieldTime.Remove(Actor);
            mFreezeActorCooldownSnowballTime.Remove(Actor);
            mFreezeActorSnowballsPowerUpCount.Remove(Actor);
        }

        public FreezeFirePower ActorFreezeFirePower(FreezePowerUp ActorPowerUp)
        {
            FreezeFirePower Result = FreezeFirePower.None;

            switch (ActorPowerUp)
            {
                case FreezePowerUp.OrangeSnowball:

                    Result = FreezeFirePower.OrangeBall;
                    break;

                case FreezePowerUp.BlueArrow:
                case FreezePowerUp.GreenArrow:
                case FreezePowerUp.Snowballs:

                    Result = FreezeFirePower.VeryHighBall;
                    break;

                default:

                    int Random = RandomGenerator.GetNext(0, 7);

                    switch (Random)
                    {
                        default:

                            Result = FreezeFirePower.LightBall;
                            break;

                        case 2:
                        case 4:

                            Result = FreezeFirePower.MediumBall;
                            break;

                        case 3:
                        case 5:

                            Result = FreezeFirePower.HighBall;
                            break;

                    }

                    break;
            }

            return Result;
        }

        public FreezePowerUp GenerateRandomPowerUp()
        {
            FreezePowerUp Result = FreezePowerUp.None;

            int Random = RandomGenerator.GetNext(0, 12);

            switch (Random)
            {
                default:

                    Result = FreezePowerUp.None;
                    break;

                case 1:

                    Result = FreezePowerUp.BlueArrow;
                    break;

                case 2:

                    Result = FreezePowerUp.Snowballs;
                    break;

                case 3:

                    Result = FreezePowerUp.GreenArrow;
                    break;

                case 4:

                    Result = FreezePowerUp.OrangeSnowball;
                    break;

                case 5:

                    Result = FreezePowerUp.Heart;
                    break;

                case 6:

                    Result = FreezePowerUp.Shield;
                    break;
            }

            return Result;
        }

        public void PickUpPowerUp(RoomActor Actor, FreezePowerUp PowerUp)
        {
            Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

            QuestType QuestType = QuestType.NONE;

            switch (PowerUp)
            {
                case FreezePowerUp.BlueArrow:
                case FreezePowerUp.Snowballs:
                case FreezePowerUp.GreenArrow:
                case FreezePowerUp.OrangeSnowball:

                    if (!mFreezeActorPowerUp.ContainsKey(Actor))
                    {
                        mFreezeActorPowerUp.Add(Actor, PowerUp);
                        break;
                    }

                    if (mFreezeActorSnowballsPowerUpCount.ContainsKey(Actor) &&
                        mFreezeActorSnowballsPowerUpCount[Actor] > 0 &&
                        mFreezeActorPowerUp[Actor] == FreezePowerUp.Snowballs &&
                        PowerUp != FreezePowerUp.Snowballs)
                    {
                        mFreezeActorSnowballsPowerUpCount.Remove(Actor);
                    }

                    mFreezeActorPowerUp[Actor] = PowerUp;

                    switch (PowerUp)
                    {
                        case FreezePowerUp.BlueArrow:

                            QuestType = QuestType.FREEZE_BIGGER_BOMB;
                            break;

                        case FreezePowerUp.Snowballs:

                            if (!mFreezeActorSnowballsPowerUpCount.ContainsKey(Actor))
                            {
                                mFreezeActorSnowballsPowerUpCount.Add(Actor, 0);
                            }

                            mFreezeActorSnowballsPowerUpCount[Actor] += 1;

                            QuestType = QuestType.FREEZE_MORE_BOMBS;
                            break;

                        case FreezePowerUp.GreenArrow:

                            QuestType = QuestType.FREEZE_XBOMB;
                            break;

                        case FreezePowerUp.OrangeSnowball:

                            QuestType = QuestType.FREEZE_MEGA_BOMB;
                            break;
                    }

                    break;

                case FreezePowerUp.Heart:

                    if (!mFreezeActorLives.ContainsKey(Actor))
                    {
                        mFreezeActorLives.Add(Actor, 0);
                    }

                    if (mFreezeActorLives[Actor] < 3)
                    {
                        mFreezeActorLives[Actor]++;

                        ChangeFreezeScoreState(10, Actor.PlayerTeamColor);
                    }

                    if (Session != null)
                    {
                        Session.SendData(GameLivesComposer.Compose(Actor.Id, mFreezeActorLives[Actor]));
                    }

                    QuestType = QuestType.FREEZE_EXTRA_LIFE;
                    break;

                case FreezePowerUp.Shield:

                    if (!mFreezeActorShieldTime.ContainsKey(Actor))
                    {
                        mFreezeActorShieldTime.Add(Actor, 0);
                    }

                    mFreezeActorShieldTime[Actor] = 10;

                    int ApplyEffect = 48 + (int)Actor.PlayerTeamColor;

                    Actor.ApplyEffect(ApplyEffect, true);

                    QuestType = QuestType.FREEZE_SHIELD;
                    break;
            }

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                CharacterInfo ActorInfo = CharacterInfoLoader.GetCharacterInfo(MySqlClient, Actor.ReferenceId);

                if (ActorInfo.HasLinkedSession)
                {
                    AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_FreezePowerUp", 1);
                }
            }

            if (QuestType != QuestType.NONE)
            {
                QuestManager.ProgressUserQuest(Session, QuestType);
            }

            if (Session != null)
            {
                QuestManager.ProgressUserQuest(Session, QuestType.FREEZE_POWERUPPER);
            }
        }

        public List<Item> GetLineItems(Vector2 Tile, FreezeFirePower FirePower)
        {
            Vector2[] Directions =
            {
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0),
                new Vector2(0, -1)
            };

            return GetAffectedItems(Tile, FirePower, Directions);
        }

        public List<Item> GetDiagonalLineItems(Vector2 Tile, FreezeFirePower FirePower)
        {
            Vector2[] Directions =
            {
                new Vector2(1, 1),
                new Vector2(1, -1),
                new Vector2(-1, 1),
                new Vector2(-1, -1)
            };

            return GetAffectedItems(Tile, FirePower, Directions);
        }

        public List<Item> GetAffectedItems(Vector2 Tile, FreezeFirePower FirePower, Vector2[] PossibleDirections)
        {
            List<Item> AffectedItems = new List<Item>();

            int Length = (int)FirePower;

            foreach (Vector2 Direction in PossibleDirections)
            {
                for (int i = 1; i <= Length; i++)
                {
                    Vector2 AffectedTile = new Vector2(Tile.X + Direction.X * i, Tile.Y + Direction.Y * i);

                    if (!mRoomInstance.IsValidPosition(AffectedTile))
                    {
                        continue;
                    }

                    List<Item> ItemsOnPosition = mRoomInstance.GetItemsOnPosition(AffectedTile).OrderBy(I => I.RoomPosition.Z).Reverse().ToList();

                    foreach (Item AffectedItem in ItemsOnPosition)
                    {
                        if (AffectedItem == null)
                        {
                            continue;
                        }

                        switch (AffectedItem.Definition.Behavior)
                        {
                            case ItemBehavior.FreezeBlock:

                                bool ContainsFreezeTile = ItemsOnPosition.Where(I => I.Definition.Behavior == ItemBehavior.FreezeTile).Count() > 0;

                                if (ContainsFreezeTile)
                                {
                                    AffectedItems.Add(AffectedItem);
                                }

                                break;

                            case ItemBehavior.FreezeTile:

                                AffectedItems.Add(AffectedItem);
                                break;
                        }
                    }
                }
            }

            return AffectedItems;
        }

        public void ChangeFreezeScoreState(int Request, TeamColors TeamColor = TeamColors.None)
        {
            List<Item> RoomScoreboardItems = mRoomInstance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.FreezeScore).ToList();

            switch (Request)
            {
                default:

                    if (RoomScoreboardItems.Count > 0)
                    {
                        Item TeamItem = RoomScoreboardItems.Where(I => (TeamColors)I.Definition.BehaviorData == TeamColor).FirstOrDefault();

                        if (TeamItem != null)
                        {
                            ItemEventDispatcher.InvokeItemEventHandler(null, TeamItem, mRoomInstance, ItemEventType.ItemInvoking, Request);
                        }
                    }

                    break;

                case 0:

                    foreach (TeamColors Colors in Enum.GetValues(typeof(TeamColors)))
                    {
                        Item TeamItem = RoomScoreboardItems.Where(I => (TeamColors)I.Definition.BehaviorData == Colors).FirstOrDefault();

                        if (TeamItem != null)
                        {
                            ItemEventDispatcher.InvokeItemEventHandler(null, TeamItem, mRoomInstance, ItemEventType.ItemInvoking);
                        }
                    }

                    break;
            }
        }

        public void HandleActorToFreeze(RoomActor Actor)
        {
            bool HasShield = mFreezeActorShieldTime.ContainsKey(Actor) ? mFreezeActorShieldTime[Actor] > 0 : false;

            if (Actor.IsBot || HasShield || Actor.PlayerTeamColor == TeamColors.None)
            {
                return;
            }

            if (!mFreezeActorFreezedTime.ContainsKey(Actor))
            {
                mFreezeActorFreezedTime.Add(Actor, 0);
            }

            mFreezeActorFreezedTime[Actor] = 10;

            if (mFreezeActorLives.ContainsKey(Actor))
            {
                mFreezeActorLives[Actor]--;
            }

            if (mFreezeActorLives[Actor] <= 0)
            {
                Item ExitTile = mRoomInstance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.FreezeExitTile).FirstOrDefault();

                if (ExitTile != null)
                {
                    Actor.BlockWalking();

                    Actor.PositionToSet = ExitTile.RoomPosition.GetVector2();
                    Actor.MoveTo(Actor.PositionToSet);
                    Actor.HeadRotation = ExitTile.RoomRotation;
                    Actor.BodyRotation = ExitTile.RoomRotation;
                    Actor.UpdateNeeded = true;

                    Actor.UnblockWalking();
                }

                ChangeFreezeScoreState(-20, Actor.PlayerTeamColor);

                int ClearEffect = 0;
                Actor.ApplyEffect(ClearEffect, true);
                Actor.PlayingGameType = GameType.None;
                Actor.PlayerTeamColor = TeamColors.None;

                UnregisterFreezeActor(Actor);

                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    CharacterInfo ActorInfo = CharacterInfoLoader.GetCharacterInfo(MySqlClient, Actor.ReferenceId);

                    if (ActorInfo.HasLinkedSession)
                    {
                        Session AchievementSession = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                        AchievementManager.ProgressUserAchievement(MySqlClient, AchievementSession, "ACH_FreezePlayer", 1);
                    }
                }

                ForceStopGame(GameType.Freeze);
                return;
            }

            Actor.BlockWalking();
            Actor.ApplyEffect(12);

            ChangeFreezeScoreState(-10, Actor.PlayerTeamColor);

            Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);
            if (Session != null)
            {
                Session.SendData(GameLivesComposer.Compose(Actor.Id, mFreezeActorLives[Actor]));
            }
        }
    }
}
