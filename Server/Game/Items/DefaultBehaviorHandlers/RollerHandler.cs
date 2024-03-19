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
        private static int mRollerTick = 4;
        private static bool mCanRollerOut = false;
        private static List<Item> mRollerOutItem = new List<Item>();

        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.Roller, new ItemEventHandler(HandleRoller));
        }

        private static bool HandleRoller(Session Session, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.UpdateTick:

                    mRollerOutItem.Clear();

                    if (mCanRollerOut)
                    {
                        List<RoomActor> ActorsToMove = Instance.GetActorsOnPosition(Item.RoomPosition.GetVector2()).ToList();
                        
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

                        List<Item> ItemsToMove = Instance.GetItemsOnPosition(Item.RoomPosition.GetVector2()).ToList();
                        List<Item> ItemsOnNextTile = Instance.GetItemsOnPosition(Item.SquareInFront).ToList();

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
                                    if (Items.ActiveHeight > NextZ)
                                    {
                                        NextZ = Items.ActiveHeight;

                                        NextRoller = true;
                                    }
                                }
                            }

                            if (NextRoller)
                            {
                                foreach (Item Items in ItemsOnNextTile)
                                {
                                    if (Items.ActiveHeight > NextZ)
                                    {
                                        NextZ = Items.ActiveHeight;

                                        NextRollerIsClear = false;
                                    }
                                }
                            }

                            foreach (Item MoveItem in ItemsToMove)
                            {
                                if (MoveItem.Definition.Behavior == ItemBehavior.Roller ||
                                    MoveItem == Item || MoveItem == null)
                                {
                                    continue;
                                }

                                if (!mRollerOutItem.Contains(MoveItem) && Instance.IsValidPosition(Item.SquareInFront)
                                    && Item.RoomPosition.Z < MoveItem.RoomPosition.Z && Instance.GetActorsOnPosition(Item.SquareInFront).Count == 0)
                                {
                                    
                                    if(!NextPositionIsRoller)
                                    {
                                        NextZ = MoveItem.RoomPosition.Z - Item.ActiveHeight;
                                    }
                                    else
                                    {
                                        NextZ = MoveItem.RoomPosition.Z;
                                    }

                                    Vector3 TargetVector3 = new Vector3(Item.SquareInFront.X, Item.SquareInFront.Y, NextZ);

                                    Instance.BroadcastMessage(RollerEventComposer.Compose(MoveItem.RoomPosition, TargetVector3, 0, 0, MoveItem.Id));

                                    using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                                    {
                                        MoveItem.MoveToRoom(MySqlClient, MoveItem.RoomId, TargetVector3, MoveItem.RoomRotation);
                                    }

                                    Instance.RegenerateRelativeHeightmap(true);

                                    mRollerOutItem.Add(MoveItem);
                                }
                            }
                        }

                        mRollerTick = 4;
                        mCanRollerOut = false;
                    }
                    else
                    {
                        mRollerTick--;

                        if(mRollerTick == 0)
                        {
                            mCanRollerOut = true;
                        }
                    }

                    goto case ItemEventType.InstanceLoaded;

                case ItemEventType.InstanceLoaded:
                case ItemEventType.Placed:

                    Item.RequestUpdate(2);
                    break;
            }

            return true;
        }
    }
}
