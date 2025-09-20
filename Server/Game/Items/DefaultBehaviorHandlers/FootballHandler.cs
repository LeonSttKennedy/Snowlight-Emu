using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

using Snowlight.Storage;
using Snowlight.Specialized;

using Snowlight.Game.Bots;
using Snowlight.Game.Misc;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Game.Characters;
using Snowlight.Game.Pathfinding;
using Snowlight.Game.Achievements;

using Snowlight.Communication.Outgoing;
using Snowlight.Game.Rooms.Games;
using Snowlight.Util;
using System.Diagnostics.Eventing.Reader;


namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class FootballHandler
    {
        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.Football, new ItemEventHandler(HandleFootballBall));
        }

        private static bool HandleFootballBall(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Placed:
                case ItemEventType.InstanceLoaded:

                    break;

                case ItemEventType.Removing:

                    break;

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

                    bool UserHasCachedDistance = Instance.GameManager.UserFootBallDistance.ContainsKey(Actor.ReferenceId) &&
                        Instance.GameManager.UserFootBallDistance[Actor.ReferenceId] > 0;

                    int PlayerDistance = UserHasCachedDistance ? Instance.GameManager.UserFootBallDistance[Actor.ReferenceId]
                        : Distance.Calculate(Actor.Position.GetVector2(), Item.RoomPosition.GetVector2());

                    int KickPower = RandomGenerator.GetNext(1, PlayerDistance);

                    Vector2 NewPoint = new Vector2(0, 0);

                    switch (Actor.BodyRotation)
                    {
                        case 4:

                            NewPoint = new Vector2(Item.RoomPosition.X, Item.RoomPosition.Y + KickPower);
                            break;

                        case 0:

                            NewPoint = new Vector2(Item.RoomPosition.X, Item.RoomPosition.Y - KickPower);
                            break;

                        case 6:

                            NewPoint = new Vector2(Item.RoomPosition.X - KickPower, Item.RoomPosition.Y);
                            break;

                        case 2:

                            NewPoint = new Vector2(Item.RoomPosition.X + KickPower, Item.RoomPosition.Y);
                            break;

                    }

                    if (Instance.IsValidStep(Item.RoomPosition.GetVector2(), NewPoint, true))
                    {
                        double Z = Instance.GetUserStepHeight(NewPoint);
                        Vector3 TargetVector3 = new Vector3(NewPoint.X, NewPoint.Y, Z);

                        Instance.BroadcastMessage(RollerEventComposer.Compose(Item.RoomPosition.GetVector2(), NewPoint, 0,
                            new List<RollerEvents>() { new RollerEvents(Item.RoomPosition.Z,
                                    Instance.GetUserStepHeight(NewPoint), 0, Item.Id) }));

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            Item.MoveToRoom(MySqlClient, Item.RoomId, TargetVector3, Item.RoomRotation);
                        }

                        int Minimum = 10 + (UserHasCachedDistance ? Instance.GameManager.UserFootBallDistance[Actor.ReferenceId] : 0);
                        int Maximum = 16;

                        Item.DisplayFlags = RandomGenerator.GetNext(Minimum, Maximum).ToString();
                        Item.BroadcastStateUpdate(Instance);

                        Instance.RegenerateRelativeHeightmap(true);

                        if (Instance.GameManager.UserFootBallDistance.ContainsKey(Actor.ReferenceId))
                        {
                            Instance.GameManager.UserFootBallDistance[Actor.ReferenceId] = 0;
                        }
                    }

                    break;
            }

            return true;
        }
    }
}
