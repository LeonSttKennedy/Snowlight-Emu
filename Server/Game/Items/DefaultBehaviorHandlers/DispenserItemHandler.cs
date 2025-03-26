using System;
using Snowlight.Game.Sessions;
using Snowlight.Game.Rooms;
using Snowlight.Game.Pathfinding;
using Snowlight.Game.Misc;

namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class DispenserItemHandler
    {
        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.Dispenser, new ItemEventHandler(HandleDispenser));
        }

        private static bool HandleDispenser(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Moved:
                    {
                        RoomActor InteractingActor = (Item.TemporaryInteractionReferenceIds.ContainsKey(0) ?
                            Instance.GetActor(Item.TemporaryInteractionReferenceIds[0]) : null);

                        if (InteractingActor != null)
                        {
                            InteractingActor.UnblockWalking();
                        }

                        goto case ItemEventType.InstanceLoaded;
                    }
                case ItemEventType.InstanceLoaded:
                    {
                        if (Item.DisplayFlags != "0")
                        {
                            Item.DisplayFlags = "0";
                            Item.BroadcastStateUpdate(Instance);
                        }

                        return true;
                    }
                case ItemEventType.Interact:
                    {
                        if (Actor.Position.X != Item.SquareInFront.X || Actor.Position.Y !=
                            Item.SquareInFront.Y)
                        {
                            Actor.MoveToItemAndInteract(Item, RequestData);
                            return true;
                        }

                        if (Item.TemporaryInteractionReferenceIds.Count == 0)
                        {
                            Item.TemporaryInteractionReferenceIds.Add(0, Actor.Id);

                            Actor.BlockWalking();
                            Actor.PositionToSet = Item.SquareInFront;

                            int NewRot = Rotation.Calculate(Actor.Position.GetVector2(),
                                Item.RoomPosition.GetVector2(), Actor.MoonWalkEnabled);

                            if (Actor.BodyRotation != NewRot)
                            {
                                Actor.BodyRotation = NewRot;
                                Actor.HeadRotation = NewRot;
                                Actor.UpdateNeeded = true;
                            }

                            Item.DisplayFlags = "1";
                            Item.BroadcastStateUpdate(Instance);       

                            Item.RequestUpdate(2);
                        }

                        return true;
                    }

                case ItemEventType.UpdateTick:
                    {
                        if (Item.DisplayFlags != "0")
                        {
                            Item.DisplayFlags = "0";
                            Item.BroadcastStateUpdate(Instance);
                        }

                        if (Item.TemporaryInteractionReferenceIds.Count < 1)
                        {
                            return true;
                        }

                        RoomActor InteractingActor = Instance.GetActor(Item.TemporaryInteractionReferenceIds[0]);
                        Item.TemporaryInteractionReferenceIds.Clear();

                        if (InteractingActor == null)
                        {
                            return true;
                        }

                        InteractingActor.CarryItem(DrinkSetManager.GetRandomDrinkForSet(Item.Definition.BehaviorData));

                        int NewRot = Rotation.Calculate(InteractingActor.Position.GetVector2(),
                            Item.RoomPosition.GetVector2(), InteractingActor.MoonWalkEnabled);

                        if (InteractingActor.BodyRotation != NewRot)
                        {
                            InteractingActor.BodyRotation = NewRot;
                            InteractingActor.HeadRotation = NewRot;
                            InteractingActor.UpdateNeeded = true;
                        }

                        InteractingActor.UnblockWalking();
                        return true;
                    }
            }

            return true;
        }
    }
}
