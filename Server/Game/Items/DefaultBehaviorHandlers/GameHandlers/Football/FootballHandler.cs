using System;
using System.Linq;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Misc;
using Snowlight.Game.Rooms;
using Snowlight.Specialized;
using Snowlight.Game.Sessions;
using Snowlight.Game.Characters;
using Snowlight.Game.Achievements;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Quests;


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
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.FootballScore, new ItemEventHandler(HandleFootballScoreboard));
        }

        private static bool HandleFootballBall(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            GameManager FootballManager = Instance.GameManager;

            switch (Event)
            {
                case ItemEventType.UpdateTick:

                    int.TryParse(Item.DisplayFlags, out int ItemFlags);

                    if (ItemFlags > 10)
                    {
                        ItemFlags--;
                        Item.DisplayFlags = ItemFlags.ToString();
                        Item.RequestUpdate(1);
                    }

                    break;

                case ItemEventType.WalkOnItem:

                    List<RollerEvents> BallEvents = new List<RollerEvents>();

                    int PlayerDistance = Distance.Calculate(Actor.FromPosition, Item.RoomPosition.GetVector2());
                    
                    int Random = RandomGenerator.GetNext(0, 2);
                    int KickPower = RandomGenerator.GetNext(1, PlayerDistance + (Random == 0 ? 1 : 0));

                    int TempX = 0;
                    int TempY = 0;

                    switch (Actor.BodyRotation)
                    {
                        case 4:

                            TempY = 1;
                            break;

                        case 0:

                            TempY = -1;
                            break;

                        case 6:

                            TempX = -1;
                            break;

                        case 2:

                            TempX = 1;
                            break;

                        case 3:

                            TempX = 1;
                            TempY = 1;
                            break;

                        case 1:

                            TempX = 1;
                            TempY = -1;
                            break;

                        case 7:

                            TempX = -1;
                            TempY = -1;
                            break;

                        case 5:

                            TempX = -1;
                            TempY = 1;
                            break;

                    }

                    Vector2 UpdatedPos = Item.RoomPosition.GetVector2();

                    for (int i = 0; i < KickPower; i++)
                    {
                        Vector2 BouncePosition = new Vector2(UpdatedPos.X + TempX, UpdatedPos.Y + TempY);

                        if (Instance.CanMoveItem(BouncePosition))
                        {
                            UpdatedPos = BouncePosition;

                            BallEvents.Add(new RollerEvents(Item.RoomPosition.Z, Instance.GetUserStepHeight(UpdatedPos), 0, Item.Id));
                        }
                        else
                        {
                            Vector2 TestX = new Vector2(UpdatedPos.X + TempX, UpdatedPos.Y);
                            Vector2 TestY = new Vector2(UpdatedPos.X, UpdatedPos.Y + TempY);

                            bool CanMoveX = !Instance.CanMoveItem(TestX);
                            bool CanMoveY = !Instance.CanMoveItem(TestY);

                            if (CanMoveX && CanMoveY)
                            {
                                TempX = -TempX;
                                TempY = -TempY;
                            }
                            else if (CanMoveX)
                            {
                                TempX = -TempX;
                            }
                            else if (CanMoveY)
                            {
                                TempY = -TempY;
                            }
                            else
                            {
                                TempX = -TempX;
                                TempY = -TempY;
                            }

                            BouncePosition = new Vector2(UpdatedPos.X + TempX, UpdatedPos.Y + TempY);

                            if (Instance.CanMoveItem(BouncePosition))
                            {
                                UpdatedPos = BouncePosition;

                                BallEvents.Add(new RollerEvents(Item.RoomPosition.Z, Instance.GetUserStepHeight(UpdatedPos), 0, Item.Id));
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (UpdatedPos != Item.RoomPosition.GetVector2())
                    {
                        double Z = Instance.GetUserStepHeight(UpdatedPos);
                        Vector3 TargetVector3 = new Vector3(UpdatedPos.X, UpdatedPos.Y, Z);

                        Instance.BroadcastMessage(RollerEventComposer.Compose(Item.RoomPosition.GetVector2(), UpdatedPos, 0, BallEvents));

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            Item.MoveToRoom(MySqlClient, Item.RoomId, TargetVector3, Item.RoomRotation);
                        }

                        int Minimum = 10;
                        int Maximum = 16;

                        int RandomMaximum = RandomGenerator.GetNext(Minimum + 1, Maximum);

                        Item.Flags = RandomGenerator.GetNext(Minimum, RandomMaximum).ToString();
                        Item.DisplayFlags = Item.Flags;
                        Item.BroadcastStateUpdate(Instance);

                        Instance.RegenerateRelativeHeightmap(true);

                        FootballManager.HandleFootballGoal(Actor, UpdatedPos, Item.Definition.Behavior);
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

                    Actor.PlayingGameType = Actor.PlayingGameType == GameType.None ? GameType.Football : GameType.None;
                    Actor.PlayerTeamColor = Actor.PlayerTeamColor == TeamColors.None ? ItemGameColor : TeamColors.None;

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

        private static bool HandleFootballScoreboard(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Placed:

                    Item.Flags = "0";
                    Item.DisplayFlags = Item.Flags;

                    RoomManager.MarkWriteback(Item, true);
                    Item.BroadcastStateUpdate(Instance);
                    break;

                case ItemEventType.Interact:
                case ItemEventType.ItemInvoking:

                    if (Event == ItemEventType.Interact)
                    {
                        Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                        if (!Instance.CheckUserRights(Session))
                        {
                            break;
                        }

                        if (!Distance.TilesTouching(Actor.Position.GetVector2(), Item.RoomPosition.GetVector2()))
                        {
                            Actor.MoveToItemAndInteract(Item, RequestData);
                            break;
                        }
                    }

                    int.TryParse(Item.DisplayFlags, out int Val);

                    if (RequestData == 1)
                    {
                        Val--;

                        if (Val < 0)
                        {
                            Val = 99;
                        }
                    }
                    else if (RequestData == 2)
                    {
                        Val++;

                        if (Val > 99)
                        {
                            Val = 0;
                        }
                    }

                    Item.Flags = Val.ToString();
                    Item.DisplayFlags = Item.Flags;

                    RoomManager.MarkWriteback(Item, true);
                    Item.BroadcastStateUpdate(Instance);

                    break;
            }

            return true;
        }
    }
}
