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
                        }

                        Vector2 NewPoint = new Vector2(Item.RoomPosition.X + TempX, Item.RoomPosition.Y + TempY);

                        if (Instance.CanMoveItem(NewPoint))
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

                            Instance.RegenerateRelativeHeightmap(true);
                        }
                    }

                    break;
            }

            return true;
        }
    }
}
