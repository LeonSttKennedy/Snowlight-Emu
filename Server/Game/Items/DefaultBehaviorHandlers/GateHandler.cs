using System;
using System.Collections.Generic;

using Snowlight.Game.Sessions;
using Snowlight.Game.Rooms;
using Snowlight.Specialized;
using Snowlight.Storage;
using Snowlight.Game.Misc;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class GateHandler
    {
        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.Gate, new ItemEventHandler(HandleFixedGateSwitch));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.OneWayGate, new ItemEventHandler(HandleOneWayGate));
        }

        private static bool HandleFixedGateSwitch(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Interact:

                    Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                    if (!Instance.CheckUserRights(Session))
                    {
                        return true;
                    }

                    List<Vector2> GateTiles = Instance.CalculateAffectedTiles(Item, Item.RoomPosition.GetVector2(), Item.RoomRotation);

                    foreach (Vector2 Tile in GateTiles)
                    {
                        if (Instance.GetActorsOnPosition(Tile).Count > 0)
                        {
                            return true;
                        }
                    }

                    int.TryParse(Item.Flags, out int CurrentState);

                    Item.Flags = (CurrentState == 0 ? 1 : 0).ToString();
                    Item.DisplayFlags = Item.Flags;

                    RoomManager.MarkWriteback(Item, true);

                    Item.BroadcastStateUpdate(Instance);
                    Instance.RegenerateRelativeHeightmap(true);
                    break;
            }

            return true;
        }

        private static bool HandleOneWayGate(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.InstanceLoaded:
                case ItemEventType.Moved:
                case ItemEventType.Placed:

                    if (Item.DisplayFlags != "0")
                    {
                        Item.DisplayFlags = "0";
                        Instance.BroadcastMessage(OneWayGateStatusComposer.Compose(Item.Id, Item.DisplayFlags.Equals("1")));
                    }

                    foreach (uint ActorId in Item.TemporaryInteractionReferenceIds.Values)
                    {
                        RoomActor ActorToUnlock = Instance.GetActor(ActorId);

                        if (ActorToUnlock != null)
                        {
                            ActorToUnlock.UnblockWalking();
                        }
                    }

                    Item.TemporaryInteractionReferenceIds.Clear();
                    break;

                case ItemEventType.Interact:

                    if (Actor.Position.X != Item.SquareInFront.X || Actor.Position.Y != Item.SquareInFront.Y)
                    {
                        Actor.MoveToItemAndInteract(Item, RequestData);
                        break;
                    }

                    if (Item.TemporaryInteractionReferenceIds.Count == 0 && Instance.IsValidStep(Item.RoomPosition.GetVector2(),
                        Item.SquareBehind, true) && Item.DisplayFlags == "0")
                    {
                        //Actor.BlockWalking();
                        Actor.MoveTo(Item.RoomPosition.GetVector2(), true, true, true);
                        Item.TemporaryInteractionReferenceIds.Add(1, Actor.Id);
                        Item.RequestUpdate(1);
                    }

                    break;

                case ItemEventType.UpdateTick:
                    
                    RoomActor UpdateActor = null;

                    if (Item.TemporaryInteractionReferenceIds.ContainsKey(1))
                    {
                        UpdateActor = Instance.GetActor(Item.TemporaryInteractionReferenceIds[1]);
                    }

                    if (UpdateActor == null || !Instance.IsValidStep(Item.RoomPosition.GetVector2(), Item.SquareBehind, true)
                        || ((UpdateActor.Position.X != Item.RoomPosition.X || UpdateActor.Position.Y !=
                        Item.RoomPosition.Y) && (UpdateActor.Position.X != Item.SquareInFront.X ||
                        UpdateActor.Position.Y != Item.SquareInFront.Y)))
                    {
                        if (Item.DisplayFlags != "0")
                        {
                            Item.DisplayFlags = "0";
                            Instance.BroadcastMessage(OneWayGateStatusComposer.Compose(Item.Id, Item.DisplayFlags.Equals("1")));
                        }

                        if (Item.TemporaryInteractionReferenceIds.Count > 0)
                        {
                            Item.TemporaryInteractionReferenceIds.Clear();
                        }

                        if (UpdateActor != null)
                        {
                            UpdateActor.UnblockWalking();
                        }

                        break;
                    }

                    Item.DisplayFlags = "1";
                    Instance.BroadcastMessage(OneWayGateStatusComposer.Compose(Item.Id, Item.DisplayFlags.Equals("1")));

                    UpdateActor.OverrideClipping = !UpdateActor.OverrideClipping;
                    UpdateActor.MoveTo(Item.SquareBehind, true, true, true);
                    
                    Item.RequestUpdate(2);
                    UpdateActor.OverrideClipping = !UpdateActor.OverrideClipping;
                    break;
            }

            return true;
        }
    }
}