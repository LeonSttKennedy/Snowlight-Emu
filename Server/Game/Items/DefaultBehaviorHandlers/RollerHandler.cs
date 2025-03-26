using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Storage;
using Snowlight.Game.Misc;
using Snowlight.Game.Rooms;
using Snowlight.Specialized;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class RollerHandler
    {
        private static int mRollerTick = 4;
        private static Dictionary<uint, int> mRollerTicks = new Dictionary<uint, int>();
        private static Dictionary<uint, List<RollerEvents>> mRollerEvents = new Dictionary<uint, List<RollerEvents>>();
        private static List<uint> mRollerItemsIds = new List<uint>();

        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.Roller, new ItemEventHandler(HandleRoller));
        }

        private static bool HandleRoller(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.UpdateTick:

                    if (mRollerTicks[Instance.Info.Id] == 0)
                    {
                        mRollerTicks[Instance.Info.Id] = mRollerTick;

                        mRollerItemsIds.Clear();

                        mRollerEvents.Clear();
                        RollerEvents RollerEvent = null;

                        List<Item> RoomRollers = Instance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.Roller).ToList();

                        foreach (Item Roller in RoomRollers)
                        {
                            if(!mRollerEvents.ContainsKey(Roller.Id))
                            {
                                mRollerEvents[Roller.Id] = new List<RollerEvents>();
                            }

                            List<RoomActor> ActorsToMove = Instance.GetActorsOnPosition(Roller.RoomPosition.GetVector2()).ToList();

                            if (ActorsToMove.Count > 0)
                            {
                                foreach (RoomActor MoveActors in ActorsToMove)
                                {
                                    if (MoveActors.IsMoving)
                                    {
                                        continue;
                                    }

                                    if (Instance.IsValidStep(MoveActors.Position.GetVector2(), Roller.SquareInFront, false))
                                    {
                                        MoveActors.PositionToSet = Roller.SquareInFront;

                                        RollerEvent = new RollerEvents(MoveActors.Position.Z, Instance.GetUserStepHeight(MoveActors.PositionToSet),
                                            MoveActors.Id, 0, MovementType.Slide);

                                        if (RollerEvent != null)
                                        {
                                            mRollerEvents[Roller.Id].Add(RollerEvent);

                                            RollerEvent = null;
                                        }
                                    }
                                }
                            }

                            List<Item> ItemsToMove = Instance.GetItemsOnPosition(Roller.RoomPosition.GetVector2()).OrderBy(O => O.RoomPosition.Z).ToList();
                            List<Item> ItemsOnNextTile = Instance.GetItemsOnPosition(Roller.SquareInFront).ToList();

                            if (ItemsToMove.Count > 0)
                            {
                                double NextZ = 0.0;
                                bool NextRollerIsClear = true;

                                bool NextPositionIsRoller = ItemsOnNextTile.Where(O => O.Definition.Behavior.Equals(ItemBehavior.Roller)).ToList().Count > 0;
                                bool NextRoller = false;

                                foreach (Item Items in ItemsOnNextTile)
                                {
                                    if (Items.Definition.Behavior.Equals(ItemBehavior.Roller))
                                    {
                                        NextRoller = true;
                                    }
                                }

                                if (NextRoller)
                                {
                                    foreach (Item Items in ItemsOnNextTile.Where(O=> O.RoomPosition.Z > Roller.RoomPosition.Z))
                                    {
                                        if (Instance.GetUserStepHeight(Items.RoomPosition.GetVector2()) > Roller.ActiveHeight)
                                        {
                                            NextRollerIsClear = false;
                                        }
                                    }
                                }

                                foreach (Item MoveItem in ItemsToMove)
                                {
                                    if (MoveItem == null || MoveItem == Roller || 
                                        MoveItem.Definition.Behavior == ItemBehavior.Roller)
                                    {
                                        continue;
                                    }

                                    if (Roller.RoomPosition.Z < MoveItem.RoomPosition.Z 
                                        && NextRollerIsClear
                                        && !mRollerItemsIds.Contains(MoveItem.Id)
                                        && Instance.IsValidPosition(Roller.SquareInFront)
                                        && Instance.GetActorsOnPosition(Roller.SquareInFront).Count == 0)
                                    {
                                        mRollerItemsIds.Add(MoveItem.Id);

                                        NextZ = MoveItem.RoomPosition.Z;

                                        if (!NextPositionIsRoller)
                                        {
                                            NextZ -= Roller.ActiveHeight;
                                        }

                                        Vector3 TargetVector3 = new Vector3(Roller.SquareInFront.X, Roller.SquareInFront.Y, NextZ);

                                        RollerEvent = new RollerEvents(MoveItem.RoomPosition.Z, NextZ, 0, MoveItem.Id);
                                        if (mRollerEvents != null)
                                        {
                                            mRollerEvents[Roller.Id].Add(RollerEvent);

                                            RollerEvent = null;
                                        }

                                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                                        {
                                            MoveItem.MoveToRoom(MySqlClient, MoveItem.RoomId, TargetVector3, MoveItem.RoomRotation);
                                        }
                                    }
                                }
                            }

                            if (mRollerEvents[Roller.Id].Count > 0)
                            {
                                Instance.RegenerateRelativeHeightmap(true);
                                Instance.BroadcastMessage(RollerEventComposer.Compose(Roller.RoomPosition.GetVector2(), Roller.SquareInFront, Roller.Id, mRollerEvents[Roller.Id]));
                            }
                        }
                    }
                    else
                    {
                        mRollerTicks[Instance.Info.Id]--;
                    }

                    Item.RequestUpdate(4);
                    break;

                case ItemEventType.InstanceLoaded:
                case ItemEventType.Placed:

                    if (!mRollerTicks.ContainsKey(Instance.Info.Id))
                    {
                        mRollerTicks.Add(Instance.Info.Id, mRollerTick);
                    }

                    Item.RequestUpdate(0);
                    break;

                case ItemEventType.Removing:

                    List<Item> RemaingRollers = Instance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.Roller).ToList();

                    if(RemaingRollers.Count == 0)
                    {
                        if (mRollerTicks.ContainsKey(Instance.Info.Id))
                        {
                            mRollerTicks.Remove(Instance.Info.Id);
                        }
                    }

                    break;
            }

            return true;
        }
    }
}
