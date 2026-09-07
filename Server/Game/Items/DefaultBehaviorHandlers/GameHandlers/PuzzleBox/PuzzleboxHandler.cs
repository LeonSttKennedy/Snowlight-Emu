using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Snowlight.Game.Sessions;
using Snowlight.Game.Rooms;
using Snowlight.Communication.Outgoing;
using Snowlight.Specialized;
using Snowlight.Game.Misc;
using Snowlight.Storage;
using Snowlight.Game.Pathfinding;

namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class PuzzleboxHandler
    {
        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.PuzzleBox, new ItemEventHandler(HandlePuzzlebox));
        }

        private static bool HandlePuzzlebox(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Interact:
                    {
                        if (Distance.Calculate(Actor.Position.GetVector2(), Item.RoomPosition.GetVector2()) < 2)
                        {
                            int NewRot = Rotation.Calculate(Actor.Position.GetVector2(),
                                Item.RoomPosition.GetVector2(), Actor.MoonWalkEnabled);

                            if (Actor.BodyRotation != NewRot)
                            {
                                Actor.BodyRotation = NewRot;
                                Actor.HeadRotation = NewRot;
                                Actor.UpdateNeeded = true;
                            }

                            if ((Actor.BodyRotation % 2) != 0)
                            {
                                Actor.BodyRotation--;
                                Actor.UpdateNeeded = true;
                                return true;
                            }

                            Vector2 NewPoint = new Vector2(0, 0);
                            if (Actor.BodyRotation == 4)
                            {
                                NewPoint = new Vector2(Item.RoomPosition.X, Item.RoomPosition.Y + 1);
                            }

                            if (Actor.BodyRotation == 0)
                            {
                                NewPoint = new Vector2(Item.RoomPosition.X, Item.RoomPosition.Y - 1);
                            }

                            if (Actor.BodyRotation == 6)
                            {
                                NewPoint = new Vector2(Item.RoomPosition.X - 1, Item.RoomPosition.Y);
                            }

                            if (Actor.BodyRotation == 2)
                            {
                                NewPoint = new Vector2(Item.RoomPosition.X + 1, Item.RoomPosition.Y);
                            }

                            if (Instance.IsValidStep(Item.RoomPosition.GetVector2(), NewPoint, true))
                            {
                                double Z = Instance.GetUserStepHeight(NewPoint);
                                Vector3 TargetVector3 = new Vector3(NewPoint.X, NewPoint.Y, Z);

                                Instance.BroadcastMessage(RollerEventComposer.Compose(Item.RoomPosition.GetVector2(), NewPoint, 0,
                                    new List<RollerEvents>() { new RollerEvents(Item.RoomPosition.Z,
                                    Instance.GetUserStepHeight(NewPoint), 0, Item.Id) } ));

                                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                                {
                                    Item.MoveToRoom(MySqlClient, Item.RoomId, TargetVector3, Item.RoomRotation);
                                }
                                Instance.RegenerateRelativeHeightmap(true);
                            }
                        }

                        return true;
                    }

                case ItemEventType.InstanceLoaded:
                case ItemEventType.Moved:
                case ItemEventType.Placed:

                    Item.RequestUpdate(1);
                    break;
            }

            return true;
        }
    }
}
