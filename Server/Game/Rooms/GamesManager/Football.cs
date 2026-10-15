using Snowlight.Game.Achievements;
using Snowlight.Game.Characters;
using Snowlight.Game.Items;
using Snowlight.Game.Quests;
using Snowlight.Game.Sessions;
using Snowlight.Specialized;
using Snowlight.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Game.Rooms
{
    public partial class GameManager
    {
        private Dictionary<Item, TeamColors> mFootballGateTeam;
        private Dictionary<uint, string> mUserClothesHolder;

        public Dictionary<Item, TeamColors> FootballGateTeam
        {
            get
            {
                return mFootballGateTeam;
            }

            set
            {
                mFootballGateTeam = value;
            }
        }

        public Dictionary<uint, string> UserClothesHolder
        {

            get
            {
                return mUserClothesHolder;
            }

            set
            {
                mUserClothesHolder = value;
            }
        }

        public bool FootballStarted
        {
            get
            {
                return mGameStarted.ContainsKey(GameType.Football) ? mGameStarted[GameType.Football] : false;
            }
        }

        public void RegisterFootballGate(Item Item)
        {
            if (!mFootballGateTeam.ContainsKey(Item))
            {
                List<string> ListTeams = Enum.GetNames(typeof(TeamColors)).ToList();

                foreach (string Colors in ListTeams)
                {
                    if (Colors == "None")
                    {
                        continue;
                    }

                    Enum.TryParse(Colors, true, out TeamColors TeamColor);

                    if (!mFootballGateTeam.ContainsValue(TeamColor))
                    {
                        mFootballGateTeam.Add(Item, TeamColor);

                        List<string> ItemExtraData = Item.Flags.Split(',').ToList();

                        if (ItemExtraData.Count < 3)
                        {
                            ItemExtraData.Add(((int)TeamColor).ToString());
                        }
                        else
                        {
                            ItemExtraData[2] = ((int)TeamColor).ToString();
                        }

                        Item.Flags = string.Join(",", ItemExtraData);
                        Item.DisplayFlags = Item.Flags;
                        RoomManager.MarkWriteback(Item, true);

                        break;
                    }
                }
            }
        }

        public void UnRegisterFootballGate(Item Item)
        {
            if (mFootballGateTeam.ContainsKey(Item))
            {
                mFootballGateTeam.Remove(Item);
            }
        }

        public void HandleFootballGoal(RoomActor Actor, Vector2 RoomPosition, ItemBehavior BallType)
        {
            List<Item> ItemsOnBallPosition = mRoomInstance.GetItemsOnPosition(RoomPosition).Where(I => I.Definition.Behavior == ItemBehavior.FootballGoal).ToList();

            if (ItemsOnBallPosition.Count > 0)
            {
                Item Goal = ItemsOnBallPosition.FirstOrDefault();

                switch (BallType)
                {
                    case ItemBehavior.Football:

                        HandleFootballGoal(Actor, Goal);
                        break;

                    case ItemBehavior.IcePuck:

                        HandleIcePuckGoal(Actor, Goal);
                        break;

                    case ItemBehavior.Beachball:

                        HandleBeachBallGoal(Actor, Goal);
                        break;
                }
            }
        }

        private void HandleFootballGoal(RoomActor Actor, Item Item)
        {
            if (FootballStarted)
            {
                List<Item> GeneralItemsOnRoom = mRoomInstance.GetFloorItems().ToList();

                TeamColors GoalColor = GameColorByInt(Item.Definition.BehaviorData);
                TeamColors OpponentTeam = GetOpponentColor(GameType.Football, Actor.PlayerTeamColor);

                bool IsOpponentGoal = Actor.PlayerTeamColor != GoalColor;
                TeamColors GoalScorerColor = IsOpponentGoal ? Actor.PlayerTeamColor : OpponentTeam;

                List<Item> GameScoreboardList = GeneralItemsOnRoom.Where(I => I.Definition.Behavior.Equals(ItemBehavior.FootballScore)).ToList();

                foreach (Item Scoreboard in GameScoreboardList)
                {
                    TeamColors ScoreboardColor = GameColorByInt(Scoreboard.Definition.BehaviorData);

                    if (GoalScorerColor == ScoreboardColor)
                    {
                        ItemEventDispatcher.InvokeItemEventHandler(Actor, Scoreboard, mRoomInstance, ItemEventType.ItemInvoking, 2);
                    }
                }

                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                    uint RoomOwnerId = mRoomInstance.Info.OwnerId;
                    CharacterInfo OwnerInfo = CharacterInfoLoader.GetCharacterInfo(MySqlClient, RoomOwnerId);

                    if (Session.CharacterId != OwnerInfo.Id)
                    {
                        if (OwnerInfo.HasLinkedSession)
                        {
                            Session OwnerSession = SessionManager.GetSessionByCharacterId(RoomOwnerId);

                            AchievementManager.ProgressUserAchievement(MySqlClient, OwnerSession, "ACH_FootballGoalScoredInRoom", 1);
                        }
                        else
                        {
                            AchievementManager.OfflineProgressUserAchievement(MySqlClient, RoomOwnerId, "ACH_FootballGoalScoredInRoom", 1);
                        }
                    }

                    AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_FootballGoalScored", 1);
                }
            }
        }

        private void HandleIcePuckGoal(RoomActor Actor, Item Item)
        {
            if (FootballStarted)
            {
                List<Item> GeneralItemsOnRoom = mRoomInstance.GetFloorItems().ToList();

                TeamColors GoalColor = mRoomInstance.GameManager.GameColorByInt(Item.Definition.BehaviorData);
                TeamColors OpponentTeam = mRoomInstance.GameManager.GetOpponentColor(GameType.IceHockey, GoalColor);

                List<Item> GameScoreboardList = GeneralItemsOnRoom.Where(I => I.Definition.Behavior.Equals(ItemBehavior.FootballScore)).ToList();

                foreach (Item Scoreboard in GameScoreboardList)
                {
                    TeamColors ScoreboardColor = mRoomInstance.GameManager.GameColorByInt(Scoreboard.Definition.BehaviorData);

                    if (OpponentTeam == ScoreboardColor)
                    {
                        ItemEventDispatcher.InvokeItemEventHandler(Actor, Scoreboard, mRoomInstance, ItemEventType.ItemInvoking, 2);
                    }
                }
            }
        }

        private void HandleBeachBallGoal(RoomActor Actor, Item Item)
        {
            Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

            if (Session != null)
            {
                QuestManager.ProgressUserQuest(Session, QuestType.BEACHBALL_GOAL);
            }
        }
    }
}
