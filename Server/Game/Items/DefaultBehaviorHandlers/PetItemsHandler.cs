﻿using System;
using System.Collections.Generic;

using Snowlight.Game.Sessions;
using Snowlight.Game.Rooms;
using Snowlight.Specialized;
using Snowlight.Storage;
using Snowlight.Game.Misc;
using Snowlight.Communication.Outgoing;
using System.Linq;
using System.Web.UI.WebControls;
using Microsoft.Win32;

namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class PetItemsHandler
    {
        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.PetNest, new ItemEventHandler(HandleNestSwitch));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.PetFood, new ItemEventHandler(HandleFoodSwitch));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.BallToy, new ItemEventHandler(HandleToySwitch));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.ChickenToy, new ItemEventHandler(HandleToySwitch));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.FrogToy, new ItemEventHandler(HandleToySwitch));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.MonkeyToy, new ItemEventHandler(HandleToySwitch));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.DragonToy, new ItemEventHandler(HandleToySwitch));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.SpiderToy, new ItemEventHandler(HandleSpiderToySwitch));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.PetWaterBowl, new ItemEventHandler(HandleWaterSwitch));
        }

        private static bool HandleNestSwitch(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
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

                    Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                    if (!Instance.CheckUserRights(Session) || Item.Definition.BehaviorData != 2)
                    {
                        return true;
                    }

                    int.TryParse(Item.Flags, out int CurrentState);

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

        private static bool HandleWaterSwitch(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Interact:

                    Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

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
        private static bool HandleToySwitch(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
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

                    List<RoomActor> Actors = Instance.GetActorsOnPosition(Item.RoomPosition.GetVector2()).Where(O => O.UserStatusses.ContainsKey(Item.Definition.PetStatusses)).ToList();

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
        private static bool HandleSpiderToySwitch(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
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

                    List<RoomActor> Actors = Instance.GetActorsOnPosition(Item.RoomPosition.GetVector2()).Where(O => O.UserStatusses.ContainsKey(Item.Definition.PetStatusses)).ToList();

                    int.TryParse(Item.DisplayFlags, out int CurrentState);

                    bool IsRewind = false;

                    int NewState = CurrentState;

                    if (CurrentState == (Item.Definition.BehaviorData - 1))
                    {
                        IsRewind = true;
                    }
                    else if (CurrentState == 0)
                    {
                        IsRewind = false;
                    }

                    if (Actors.Count == 0)
                    {
                        if (Item.TemporaryInteractionReferenceIds.ContainsKey(0))
                        {
                            Item.TemporaryInteractionReferenceIds.Remove(0);
                        }
                    }
                    else
                    {
                        if(IsRewind)
                        {
                            NewState--;
                        }
                        else
                        {
                            NewState++;
                        }
                    }

                    if (CurrentState != NewState)
                    {
                        Item.Flags = NewState.ToString();
                        Item.DisplayFlags = Item.Flags;
                        Item.BroadcastStateUpdate(Instance);
                    }

                    Item.RequestUpdate(2);
                    break;
            }

            return true;
        }
        private static bool HandleFoodSwitch(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
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
                            RoomActor InteractingActor = Instance.GetActor(Item.TemporaryInteractionReferenceIds[0]);
                            InteractingActor.ClearStatusses();
                            InteractingActor.UpdateNeeded = true;

                            Item.TemporaryInteractionReferenceIds.Remove(0);
                            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                            {
                                Item.RemovePermanently(MySqlClient);
                                Instance.TakeItem(Item.Id);
                                Instance.RegenerateRelativeHeightmap();
                            }
                        }
                    }

                    goto case ItemEventType.InstanceLoaded;

                case ItemEventType.InstanceLoaded:

                    Item.RequestUpdate(1);
                    break;
            }

            return true;
        }
    }
}
