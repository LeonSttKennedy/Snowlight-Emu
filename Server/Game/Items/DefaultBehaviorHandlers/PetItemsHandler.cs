using System;
using System.Collections.Generic;

using Snowlight.Game.Sessions;
using Snowlight.Game.Rooms;
using Snowlight.Specialized;
using Snowlight.Storage;
using Snowlight.Game.Misc;
using Snowlight.Communication.Outgoing;
using System.Linq;
using System.Web.UI.WebControls;

namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class PetItemsHandler
    {
        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.PetNest, new ItemEventHandler(HandleNestSwitch));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.PetFood, new ItemEventHandler(HandleFoodSwitch));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.PetBall, new ItemEventHandler(HandleToySwitch));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.ChickenTrampoline, new ItemEventHandler(HandleToySwitch));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.FrogPond, new ItemEventHandler(HandleToySwitch));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.MonkeyPond, new ItemEventHandler(HandleToySwitch));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.DragonTree, new ItemEventHandler(HandleToySwitch));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.PetWaterBowl, new ItemEventHandler(HandleWaterSwitch));
        }

        private static bool HandleNestSwitch(Session Session, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Placed:

                    if (Item.TemporaryInteractionReferenceIds.ContainsKey(0))
                    {
                        Item.TemporaryInteractionReferenceIds.Remove(0);
                    }

                    if (Item.DisplayFlags != "0")
                    {
                        Item.Flags = "0";
                        Item.DisplayFlags = "0";

                        Item.BroadcastStateUpdate(Instance);
                    }

                    Item.RequestUpdate(1);

                    Instance.RegenerateRelativeHeightmap(true);
                    break;

                case ItemEventType.Moved:

                    if (Item.TemporaryInteractionReferenceIds.ContainsKey(0))
                    {
                        Item.TemporaryInteractionReferenceIds.Remove(0);
                    }

                    Item.RequestUpdate(1);

                    Instance.RegenerateRelativeHeightmap(true);

                    break;

                case ItemEventType.Interact:

                    if (!Instance.CheckUserRights(Session) || Item.Definition.BehaviorData != 2)
                    {
                        return true;
                    }

                    int CurrentState = 0;
                    int.TryParse(Item.Flags, out CurrentState);

                    int NewState = CurrentState + 1;

                    if (CurrentState < 0 || CurrentState >= (Item.Definition.BehaviorData - 1))
                    {
                        NewState = 0;
                    }

                    if (CurrentState != NewState)
                    {
                        Item.Flags = NewState.ToString();
                        Item.DisplayFlags = Item.Flags;

                        RoomManager.MarkWriteback(Item, true);

                        Item.BroadcastStateUpdate(Instance);
                    }

                    break;

                case ItemEventType.UpdateTick:

                    List<RoomActor> Actors = Instance.GetActorsOnPosition(Item.RoomPosition.GetVector2()).ToList();

                    string Flags = "0";

                    if (Actors.Count == 0)
                    {
                        Flags = "0";

                        if (Item.TemporaryInteractionReferenceIds.ContainsKey(0))
                        {
                            Item.TemporaryInteractionReferenceIds.Remove(0);
                        }
                    }
                    
                    if(Item.TemporaryInteractionReferenceIds.ContainsKey(0))
                    {
                        Flags = "1";
                    }

                    if (Item.DisplayFlags != Flags &&
                        Item.Definition.BehaviorData != 2)
                    {
                        Item.Flags = Flags;
                        Item.DisplayFlags = Flags;
                        Item.BroadcastStateUpdate(Instance);
                    }

                    Item.RequestUpdate(1);
                    break;
            }

            return true;
        }

        private static bool HandleWaterSwitch(Session Session, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Interact:

                    if (!Instance.CheckUserRights(Session))
                    {
                        return true;
                    }

                    int.TryParse(Item.Flags, out int CurrentState);

                    int NewState = CurrentState + 1;

                    if (CurrentState < 0)
                    {
                        NewState = 0;
                    }

                    if(CurrentState >= (Item.Definition.BehaviorData - 1))
                    {
                        return true;
                    }

                    if (CurrentState != NewState)
                    {
                        Item.Flags = NewState.ToString();
                        Item.DisplayFlags = Item.Flags;

                        RoomManager.MarkWriteback(Item, true);

                        Item.BroadcastStateUpdate(Instance);
                    }

                    break;

                case ItemEventType.UpdateTick:

                    int.TryParse(Item.Flags, out int CurrentWaterState);

                    CurrentWaterState--;
                    int NewWaterState = 0;

                    if (CurrentWaterState < 0)
                    {
                        CurrentWaterState = 0;
                    }

                    if (CurrentWaterState != NewWaterState)
                    {
                        Item.Flags = CurrentWaterState.ToString();
                        Item.DisplayFlags = Item.Flags;

                        RoomManager.MarkWriteback(Item, true);

                        Item.BroadcastStateUpdate(Instance);
                    }

                    break;
            }

            return true;
        }
        private static bool HandleToySwitch(Session Session, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            string status = Item.Definition.Behavior.Equals(ItemBehavior.FrogPond) ? "dip" :
                (Item.Definition.Behavior.Equals(ItemBehavior.MonkeyPond) ? "swm" : "pla");

            switch (Event)
            {
                case ItemEventType.InstanceLoaded:
                case ItemEventType.Moved:
                case ItemEventType.Placed:

                    if (Item.DisplayFlags != "0")
                    {
                        Item.Flags = "0";
                        Item.DisplayFlags = "0";
                        Item.BroadcastStateUpdate(Instance);
                    }

                    Item.RequestUpdate(1);
                    break;

                case ItemEventType.UpdateTick:

                    List<RoomActor> Actors = Instance.GetActorsOnPosition(Item.RoomPosition.GetVector2()).Where(O => O.UserStatusses.ContainsKey(status)).ToList();

                    string Flags;

                    if(Actors.Count == 0)
                    {
                        Flags = "0";

                        if(Item.TemporaryInteractionReferenceIds.ContainsKey(0))
                        {
                            Item.TemporaryInteractionReferenceIds.Remove(0);
                        }
                    }
                    else
                    {
                        Flags = "1";
                    }

                    if (Item.DisplayFlags != Flags)
                    {
                        Item.Flags = Flags;
                        Item.DisplayFlags = Flags;
                        Item.BroadcastStateUpdate(Instance);
                    }

                    Item.RequestUpdate(1);
                    break;
            }

            return true;
        }
        private static bool HandleFoodSwitch(Session Session, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.UpdateTick:
                    
                    List<RoomActor> Actors = Instance.GetActorsOnPosition(Item.RoomPosition.GetVector2()).Where(O => O.UserStatusses.ContainsKey("shk") || O.UserStatusses.ContainsKey("eat")).ToList();

                    int.TryParse(Item.Flags, out int CurrentFoodState);

                    CurrentFoodState++;
                    int NewFoodState = 0;

                    if (Actors.Count == 0)
                    {
                        if (Item.TemporaryInteractionReferenceIds.ContainsKey(0))
                        {
                            Item.TemporaryInteractionReferenceIds.Remove(0);
                        }
                    }

                    if (CurrentFoodState > 0 && Item.TemporaryInteractionReferenceIds.Count > 0)
                    {
                        if (CurrentFoodState != NewFoodState)
                        {
                            Item.Flags = CurrentFoodState.ToString();
                            Item.DisplayFlags = Item.Flags;

                            RoomManager.MarkWriteback(Item, true);

                            Item.BroadcastStateUpdate(Instance);
                        }

                        if(CurrentFoodState >= Item.Definition.BehaviorData)
                        {
                            RoomActor Actor = Instance.GetActor(Item.TemporaryInteractionReferenceIds[0]);
                            Actor.ClearStatusses();
                            Actor.UpdateNeeded = true;

                            Item.TemporaryInteractionReferenceIds.Remove(0);
                            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                            {
                                Item.RemovePermanently(MySqlClient);
                                Instance.TakeItem(Item.Id);
                                Instance.RegenerateRelativeHeightmap();
                            }
                        }
                    }

                    break;
            }

            return true;
        }
    }
}
