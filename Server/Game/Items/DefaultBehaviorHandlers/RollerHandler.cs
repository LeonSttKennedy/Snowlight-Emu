using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Snowlight.Game.Sessions;
using Snowlight.Game.Rooms;
using Snowlight.Communication.Outgoing;
using Snowlight.Specialized;
using Snowlight.Storage;

namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class RollerHandler
    {
        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.Roller, new ItemEventHandler(HandleRoller));
        }

        private static bool HandleRoller(Session Session, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.UpdateTick:

                    List<RoomActor> ActorsToMove = Instance.GetActorsOnPosition(Item.RoomPosition.GetVector2()).ToList();
                    List<Item> ItemsToMove = Instance.GetItemsOnPosition(Item.RoomPosition.GetVector2()).ToList();

                    if (ActorsToMove.Count > 0)
                    {
                        foreach (RoomActor Actor in ActorsToMove)
                        {
                            if (Actor.IsMoving)
                            {
                                continue;
                            }

                            if (Instance.IsValidStep(Actor.Position.GetVector2(), Item.SquareInFront, false))
                            {
                                Actor.PositionToSet = Item.SquareInFront;
                                Instance.BroadcastMessage(RollerEventComposer.Compose(Actor.Position, new Vector3(
                                    Actor.PositionToSet.X, Actor.PositionToSet.Y,
                                    Instance.GetUserStepHeight(Actor.PositionToSet)), Item.Id, Actor.Id, 0));
                            }
                        }
                    }

                    if(ItemsToMove.Count > 0)
                    {
                        foreach (Item MoveItem in ItemsToMove)
                        {
                            if (MoveItem.Definition.Behavior == ItemBehavior.Roller ||
                                MoveItem == Item)
                            {
                                continue;
                            }

                            if (Instance.IsValidPosition(Item.SquareInFront))
                            {
                                double Z = Instance.GetUserStepHeight(Item.SquareInFront);
                                Vector3 TargetVector3 = new Vector3(Item.SquareInFront.X, Item.SquareInFront.Y, Z);

                                Instance.BroadcastMessage(RollerEventComposer.Compose(MoveItem.RoomPosition, TargetVector3, 0, 0, MoveItem.Id));
                                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                                {
                                    MoveItem.MoveToRoom(MySqlClient, MoveItem.RoomId, TargetVector3, MoveItem.RoomRotation);
                                }
                                Instance.RegenerateRelativeHeightmap(true);
                            }
                        }
                    }

                    goto case ItemEventType.InstanceLoaded;

                case ItemEventType.InstanceLoaded:
                case ItemEventType.Placed:

                    Item.RequestUpdate(4);
                    break;
            }

            return true;
        }
    }
}
