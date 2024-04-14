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

        private static bool HandlePuzzlebox(Session Session, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Interact:
                    {
                        RoomActor InteractingActor = Instance.GetActorByReferenceId(Session.CharacterId);

                        if (InteractingActor == null)
                        {
                            return true;
                        }

                        if (Distance.Calculate(InteractingActor.Position.GetVector2(), Item.RoomPosition.GetVector2()) < 2)
                        {
                            int NewRot = Rotation.Calculate(InteractingActor.Position.GetVector2(),
                                Item.RoomPosition.GetVector2(), InteractingActor.MoonWalkEnabled);

                            if (InteractingActor.BodyRotation != NewRot)
                            {
                                InteractingActor.BodyRotation = NewRot;
                                InteractingActor.HeadRotation = NewRot;
                                InteractingActor.UpdateNeeded = true;
                            }

                            if ((InteractingActor.BodyRotation % 2) != 0)
                            {
                                InteractingActor.BodyRotation--;
                                InteractingActor.UpdateNeeded = true;
                                return true;
                            }

                            Vector2 NewPoint = new Vector2(0, 0);
                            if (InteractingActor.BodyRotation == 4)
                            {
                                NewPoint = new Vector2(Item.RoomPosition.X, Item.RoomPosition.Y + 1);
                            }

                            if (InteractingActor.BodyRotation == 0)
                            {
                                NewPoint = new Vector2(Item.RoomPosition.X, Item.RoomPosition.Y - 1);
                            }

                            if (InteractingActor.BodyRotation == 6)
                            {
                                NewPoint = new Vector2(Item.RoomPosition.X - 1, Item.RoomPosition.Y);
                            }

                            if (InteractingActor.BodyRotation == 2)
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
