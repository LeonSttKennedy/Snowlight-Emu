using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Misc;
using Snowlight.Game.Rooms;
using Snowlight.Specialized;
using Snowlight.Communication.Outgoing;


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

                        if (Instance.IsValidStep(currentPos, targetPos, false))
                        {
                            currentPos = targetPos;
                            
                            BallEvents.Add(new RollerEvents(Item.RoomPosition.Z, Instance.GetUserStepHeight(currentPos), 0, Item.Id));
                        }
                        else
                        {
                            Vector2 testX = new Vector2(currentPos.X + dX, currentPos.Y);
                            Vector2 testY = new Vector2(currentPos.X, currentPos.Y + dY);

                            bool hitX = !Instance.IsValidStep(currentPos, testX, false);
                            bool hitY = !Instance.IsValidStep(currentPos, testY, false);

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

                            if (Instance.IsValidStep(currentPos, bouncePos, false))
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
                    }

                    break;
            }

            return true;
        }
    }
}
