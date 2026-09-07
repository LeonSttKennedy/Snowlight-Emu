using Snowlight.Communication.Outgoing;
using Snowlight.Game.Achievements;
using Snowlight.Game.Characters;
using Snowlight.Game.Misc;
using Snowlight.Game.Quests;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Specialized;
using Snowlight.Storage;
using Snowlight.Util;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class FootballHandler
    {
        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.IcePuck, new ItemEventHandler(HandleFootballBall));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.Football, new ItemEventHandler(HandleFootballBall));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.Beachball, new ItemEventHandler(HandleFootballBall));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.FootballGate, new ItemEventHandler(HandleFootballGate));
        }

        private static bool HandleFootballBall(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Interact:

                    if (!Instance.GameManager.UserFootBallDistance.ContainsKey(Actor.ReferenceId))
                    {
                        Instance.GameManager.UserFootBallDistance.Add(Actor.ReferenceId, 0);
                    }

                    Instance.GameManager.UserFootBallDistance[Actor.ReferenceId] = Distance.Calculate(Actor.Position.GetVector2(), Item.RoomPosition.GetVector2());

                    break;

                case ItemEventType.UpdateTick:

                    int ItemFlags = int.Parse(Item.DisplayFlags);

                    if (ItemFlags > 10)
                    {
                        ItemFlags--;
                        Item.DisplayFlags = ItemFlags.ToString();
                        Item.RequestUpdate(1);
                    }

                    break;

                case ItemEventType.WalkOnItem:

                    List<RollerEvents> BallEvents = new List<RollerEvents>();

                    bool UserHasCachedDistance = Instance.GameManager.UserFootBallDistance.ContainsKey(Actor.ReferenceId) &&
                        Instance.GameManager.UserFootBallDistance[Actor.ReferenceId] > 0;

                    int PlayerDistance = UserHasCachedDistance ? Instance.GameManager.UserFootBallDistance[Actor.ReferenceId]
                        : Distance.Calculate(Actor.Position.GetVector2(), Item.RoomPosition.GetVector2());

                    int KickPower = RandomGenerator.GetNext(1, PlayerDistance + 1);

                    int dX = 0;
                    int dY = 0;

                    switch (Actor.BodyRotation)
                    {
                        case 4:

                            dY = 1;
                            break;

                        case 0:

                            dY = -1;
                            break;

                        case 6:

                            dX = -1;
                            break;

                        case 2:

                            dX = 1;
                            break;

                        case 3:

                            dX = 1;
                            dY = 1;
                            break;

                        case 1:

                            dX = 1;
                            dY = -1;
                            break;

                        case 7:

                            dX = -1;
                            dY = -1;
                            break;

                        case 5:

                            dX = -1;
                            dY = 1;
                            break;

                    }

                    Vector2 currentPos = Item.RoomPosition.GetVector2();

                    for (int i = 0; i < KickPower; i++)
                    {
                        Vector2 targetPos = new Vector2(currentPos.X + dX, currentPos.Y + dY);

                        if (Instance.IsValidStep(currentPos, targetPos, true))
                        {
                            currentPos = targetPos;

                            BallEvents.Add(new RollerEvents(Item.RoomPosition.Z, Instance.GetUserStepHeight(currentPos), 0, Item.Id));
                        }
                        else
                        {
                            Vector2 testX = new Vector2(currentPos.X + dX, currentPos.Y);
                            Vector2 testY = new Vector2(currentPos.X, currentPos.Y + dY);

                            bool hitX = !Instance.IsValidStep(currentPos, testX, true);
                            bool hitY = !Instance.IsValidStep(currentPos, testY, true);

                            if (hitX && hitY)
                            {
                                dX = -dX;
                                dY = -dY;
                            }
                            else if (hitX)
                            {
                                dX = -dX;
                            }
                            else if (hitY)
                            {
                                dY = -dY;
                            }
                            else
                            {
                                dX = -dX;
                                dY = -dY;
                            }

                            Vector2 bouncePos = new Vector2(currentPos.X + dX, currentPos.Y + dY);

                            if (Instance.IsValidStep(currentPos, bouncePos, true))
                            {
                                currentPos = bouncePos;

                                BallEvents.Add(new RollerEvents(Item.RoomPosition.Z, Instance.GetUserStepHeight(currentPos), 0, Item.Id));
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (currentPos != Item.RoomPosition.GetVector2())
                    {
                        double Z = Instance.GetUserStepHeight(currentPos);
                        Vector3 TargetVector3 = new Vector3(currentPos.X, currentPos.Y, Z);

                        Instance.BroadcastMessage(RollerEventComposer.Compose(Item.RoomPosition.GetVector2(), currentPos, 0, BallEvents));

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            Item.MoveToRoom(MySqlClient, Item.RoomId, TargetVector3, Item.RoomRotation);
                        }

                        int Minimum = 10 + (UserHasCachedDistance ? Instance.GameManager.UserFootBallDistance[Actor.ReferenceId] : 0);
                        int Maximum = 16 + (UserHasCachedDistance ? Instance.GameManager.UserFootBallDistance[Actor.ReferenceId] : 0);

                        Item.DisplayFlags = RandomGenerator.GetNext(Minimum, Maximum).ToString();
                        Item.BroadcastStateUpdate(Instance);

                        Instance.RegenerateRelativeHeightmap(true);

                        if (Instance.GameManager.UserFootBallDistance.ContainsKey(Actor.ReferenceId))
                        {
                            Instance.GameManager.UserFootBallDistance[Actor.ReferenceId] = 0;
                        }

                        List<Item> ItemsOnBallPosition = Instance.GetItemsOnPosition(currentPos).Where(I => I.Definition.Behavior.Equals(ItemBehavior.FootballGoal)).ToList();

                        if (ItemsOnBallPosition.Count > 0)
                        {
                            Item FootballGoal = ItemsOnBallPosition.FirstOrDefault();

                            switch (Item.Definition.Behavior)
                            {

                                case ItemBehavior.Football:

                                    HandleFootballGoal(Actor, FootballGoal, Instance, ItemEventType.ItemInvoking, RequestData);
                                    break;

                                case ItemBehavior.IcePuck:

                                    HandleIcePuckGoal(Actor, FootballGoal, Instance, ItemEventType.ItemInvoking, RequestData);
                                    break;
                            }
                        }
                    }

                    break;
            }

            return true;
        }

        private static bool HandleFootballGate(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Placed:
                case ItemEventType.InstanceLoaded:

                    Instance.GameManager.RegisterFootballGate(Item);

                    break;

                case ItemEventType.Removing:

                    Instance.GameManager.UnRegisterFootballGate(Item);

                    break;

                case ItemEventType.WalkOnItem:

                    GameManager FootballManager = Instance.GameManager;

                    TeamColors ItemGameColor = FootballManager.GateColor(GameType.Football, Item);

                    if (FootballManager.GetUserTeam(Actor) == TeamColors.None)
                    {
                        FootballManager.AddUserToTeam(GameType.Football, ItemGameColor, Actor);
                    }
                    else
                    {
                        TeamColors AlterGameColor = FootballManager.GetUserTeam(Actor) == ItemGameColor ?
                            TeamColors.None : ItemGameColor;

                        FootballManager.RemoveOrAlterUserTeam(GameType.Football, AlterGameColor, Actor);
                    }

                    if (!string.IsNullOrEmpty(Item.Flags))
                    {
                        string[] ItemExtraData = Item.Flags.Split(',');

                        if (ItemExtraData.Length > 1)
                        {
                            if (!Actor.IsBot)
                            {
                                Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                                int GenderInt = Session.CharacterInfo.Gender == CharacterGender.Male ? 0 : 1;
                                string GenderString = Session.CharacterInfo.Gender == CharacterGender.Male ? "m" : "f";

                                if (ItemExtraData[GenderInt].Contains("."))
                                {
                                    string NewFigure = string.Empty;

                                    if (!Instance.GameManager.UserClothesHolder.ContainsKey(Session.CharacterId))
                                    {
                                        Instance.GameManager.UserClothesHolder.Add(Session.CharacterId, Session.CharacterInfo.Figure);

                                        string[] ItemClothes = ItemExtraData[GenderInt].Split('.');
                                        List<string> UserClothes = Session.CharacterInfo.Figure.Split('.').ToList();

                                        for (int i = UserClothes.Count - 1; i >= 0; i--)
                                        {
                                            string UserClothePiece = UserClothes[i];

                                            if (string.IsNullOrEmpty(UserClothePiece) || UserClothePiece.Length < 1) // Ignores any empty string texts
                                            {
                                                continue;
                                            }

                                            if (UserClothePiece.StartsWith("hd") ||
                                                UserClothePiece.StartsWith("hr")) // Ignores any hair styles or head string texts
                                            {
                                                continue;
                                            }

                                            string ClothePieceCategory = UserClothePiece.Substring(0, 2);
                                            string MatchingClothePiece = ItemClothes.FirstOrDefault(x => x.StartsWith(ClothePieceCategory));

                                            if (MatchingClothePiece != null)
                                            {
                                                UserClothes[i] = MatchingClothePiece;
                                            }
                                            else
                                            {
                                                UserClothes.RemoveAt(i);
                                            }
                                        }

                                        NewFigure = string.Join(".", UserClothes);
                                    }
                                    else
                                    {
                                        NewFigure = Instance.GameManager.UserClothesHolder[Session.CharacterId];
                                        Instance.GameManager.UserClothesHolder.Remove(Session.CharacterId);
                                    }

                                    Session.CharacterInfo.UpdateFigure(GenderString, NewFigure);
                                    Session.SendInfoUpdate();
                                }
                            }
                        }
                    }

                    break;
            }

            return true;
        }

        private static bool HandleFootballGoal(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.ItemInvoking:

                    TeamColors GoalColor = Instance.GameManager.GameColorByInt(Item.Definition.BehaviorData);

                    List<Item> GeneralItemsOnRoom = Instance.GetFloorItems().ToList();

                    List<Item> GameCounters = GeneralItemsOnRoom.Where(I => I.Definition.Behavior.Equals(ItemBehavior.GameCounter)).ToList();
                    int GameCounterItemsOnRoomCount = GameCounters.Count();

                    if (GameCounterItemsOnRoomCount > 0)
                    {
                        Item GameCounter = GameCounters.FirstOrDefault();

                        if (GameCounter != null && GameCounter.TimmerRunning)
                        {
                            TeamColors ActorTeam = Instance.GameManager.GetUserTeam(Actor);
                            TeamColors OpponentTeam = Instance.GameManager.GetOpponentColor(GameType.Football, ActorTeam);

                            bool IsOpponentGoal = Instance.GameManager.GetUserTeam(Actor) != GoalColor;
                            TeamColors GoalScorerColor = IsOpponentGoal ? ActorTeam : OpponentTeam;

                            List<Item> GameScoreboardList = GeneralItemsOnRoom.Where(I => I.Definition.Behavior.Equals(ItemBehavior.FootballScore)).ToList();

                            foreach (Item Scoreboard in GameScoreboardList)
                            {
                                TeamColors ScoreboardColor = Instance.GameManager.GameColorByInt(Scoreboard.Definition.BehaviorData);

                                if (GoalScorerColor == ScoreboardColor)
                                {
                                    ItemEventDispatcher.InvokeItemEventHandler(Actor, Scoreboard, Instance, ItemEventType.ItemInvoking, 2);
                                }
                            }

                            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                            {
                                Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                                uint RoomOwnerId = Instance.Info.OwnerId;
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

                    break;
            }

            return true;
        }

        private static bool HandleIcePuckGoal(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.ItemInvoking:

                    TeamColors GoalColor = Instance.GameManager.GameColorByInt(Item.Definition.BehaviorData);

                    List<Item> GeneralItemsOnRoom = Instance.GetFloorItems().ToList();

                    List<Item> GameCounters = GeneralItemsOnRoom.Where(I => I.Definition.Behavior.Equals(ItemBehavior.GameCounter)).ToList();
                    int GameCounterItemsOnRoomCount = GameCounters.Count();

                    if (GameCounterItemsOnRoomCount > 0)
                    {
                        Item GameCounter = GameCounters.FirstOrDefault();

                        if (GameCounter != null && GameCounter.TimmerRunning)
                        {
                            TeamColors OpponentTeam = Instance.GameManager.GetOpponentColor(GameType.IceHockey, GoalColor);

                            List<Item> GameScoreboardList = GeneralItemsOnRoom.Where(I => I.Definition.Behavior.Equals(ItemBehavior.FootballScore)).ToList();

                            foreach (Item Scoreboard in GameScoreboardList)
                            {
                                TeamColors ScoreboardColor = Instance.GameManager.GameColorByInt(Scoreboard.Definition.BehaviorData);

                                if (OpponentTeam == ScoreboardColor)
                                {
                                    ItemEventDispatcher.InvokeItemEventHandler(Actor, Scoreboard, Instance, ItemEventType.ItemInvoking, 2);
                                }
                            }
                        }
                    }

                    break;
            }

            return true;
        }
    }
}
