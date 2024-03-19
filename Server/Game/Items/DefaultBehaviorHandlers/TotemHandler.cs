using System;
using System.Collections.Generic;

using Snowlight.Game.Sessions;
using Snowlight.Game.Rooms;
using Snowlight.Specialized;
using Snowlight.Storage;
using Snowlight.Game.Misc;
using Snowlight.Communication.Outgoing;
using System.Linq;
using Snowlight.Game.AvatarEffects;

namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class TotemHandler
    {
        private static double mLastUserInteraction = 0;

        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.TotemLeg, new ItemEventHandler(HandleTotemLeg));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.TotemHead, new ItemEventHandler(HandleTotemHead));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.TotemPlanet, new ItemEventHandler(HandleTotemPlanet));
        }

        private static bool HandleTotemLeg(Session Session, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Interact:

                    if (!Instance.CheckUserRights(Session))
                    {
                        return true;
                    }

                    Item TotemHead = null;
                    List<Item> ItensOnTileWhenInteract = Instance.GetItemsOnPosition(Item.RoomPosition.GetVector2()).Where(O => O.Definition.Behavior.Equals(ItemBehavior.TotemHead)).ToList();

                    if (ItensOnTileWhenInteract.Count > 0)
                    {
                        Item TempItem = ItensOnTileWhenInteract.FirstOrDefault();

                        if (TempItem != null && TempItem.RoomPosition.Z > Item.RoomPosition.Z)
                        {
                            TotemHead = TempItem;
                        }
                    }

                    int.TryParse(Item.Flags, out int CurrentState);

                    int NewState = CurrentState + 1;

                    if (CurrentState < 0 || CurrentState >= (Item.Definition.BehaviorData - 1))
                    {
                        NewState = 0;
                    }

                    if (CurrentState != NewState && TotemHead == null)
                    {
                        Item.Flags = NewState.ToString();
                        Item.DisplayFlags = Item.Flags;

                        RoomManager.MarkWriteback(Item, true);

                        Item.BroadcastStateUpdate(Instance);
                        Instance.RegenerateRelativeHeightmap(true);
                    }

                    break;
            }

            return true;
        }

        private static bool HandleTotemHead(Session Session, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Interact:

                    if (!Instance.CheckUserRights(Session))
                    {
                        return true;
                    }

                    Item TotemLeg = null;
                    List<Item> ItensOnTileWhenInteract = Instance.GetItemsOnPosition(Item.RoomPosition.GetVector2()).Where(O => O.Definition.Behavior.Equals(ItemBehavior.TotemLeg)).ToList();

                    if (ItensOnTileWhenInteract.Count > 0)
                    {
                        Item TempItem = ItensOnTileWhenInteract.FirstOrDefault();

                        if (TempItem != null && TempItem.RoomPosition.Z < Item.RoomPosition.Z)
                        {
                            TotemLeg = TempItem;
                        }
                    }

                    int.TryParse(Item.Flags, out int CurrentState);

                    int NewState = CurrentState + 1;

                    if (CurrentState < 0 || CurrentState >= 2)
                    {
                        NewState = 0;
                    }

                    if (CurrentState != NewState && TotemLeg == null)
                    {
                        Item.Flags = NewState.ToString();
                        Item.DisplayFlags = Item.Flags;

                        RoomManager.MarkWriteback(Item, true);

                        Item.BroadcastStateUpdate(Instance);
                        Instance.RegenerateRelativeHeightmap(true);
                    }

                    break;

                case ItemEventType.Placed:
                case ItemEventType.Moved:
                case ItemEventType.UpdateTick:

                    Item TotemLegItem = null;
                    List<Item> ItensOnTileWhenMove = Instance.GetItemsOnPosition(Item.RoomPosition.GetVector2()).Where(O => O.Definition.Behavior.Equals(ItemBehavior.TotemLeg)).ToList();
                    
                    if(ItensOnTileWhenMove.Count > 0)
                    {
                        Item TempItem = ItensOnTileWhenMove.FirstOrDefault();

                        if (TempItem != null && TempItem.RoomPosition.Z < Item.RoomPosition.Z)
                        {
                            TotemLegItem = TempItem;
                        }
                    }

                    int.TryParse(Item.Flags, out int CurrentItemState);

                    int NewItemState = TotemLegItem == null ? (int)TotemUtil.GetHeadTypeFromInt(CurrentItemState) - 1 :
                        (4 * (int)TotemUtil.GetHeadTypeFromInt(CurrentItemState)) + (int)TotemUtil.GetLegColorFromInt(int.Parse(TotemLegItem.Flags)) - 1;

                    if (CurrentItemState != NewItemState)
                    {
                        Item.Flags = NewItemState.ToString();
                        Item.DisplayFlags = Item.Flags;

                        RoomManager.MarkWriteback(Item, true);

                        Item.BroadcastStateUpdate(Instance);
                        Instance.RegenerateRelativeHeightmap(true);
                    }

                    break;
            }

            return true;
        }

        private static bool HandleTotemPlanet(Session Session, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Placed:

                    mLastUserInteraction = 0;

                    break;

                case ItemEventType.Interact:

                    if (!Instance.CheckUserRights(Session))
                    {
                        return true;
                    }

                    Item TotemLeg = null;
                    Item TotemHead = null;

                    List<Item> TotemLegItensOnTile = Instance.GetItemsOnPosition(Item.RoomPosition.GetVector2()).Where(O => O.Definition.Behavior.Equals(ItemBehavior.TotemLeg)).ToList();

                    if (TotemLegItensOnTile.Count > 0)
                    {
                        Item TempItem = TotemLegItensOnTile.FirstOrDefault();

                        if (TempItem != null && TempItem.RoomPosition.Z < Item.RoomPosition.Z)
                        {
                            TotemLeg = TempItem;
                        }
                    }

                    List<Item> TotemHeadItensOnTile = Instance.GetItemsOnPosition(Item.RoomPosition.GetVector2()).Where(O => O.Definition.Behavior.Equals(ItemBehavior.TotemHead)).ToList();

                    if (TotemHeadItensOnTile.Count > 0)
                    {
                        Item TempItem = TotemHeadItensOnTile.FirstOrDefault();

                        if (TempItem != null && TotemLeg != null && TempItem.RoomPosition.Z > TotemLeg.RoomPosition.Z)
                        {
                            TotemHead = TempItem;
                        }
                    }

                    if (TotemHead == null || TotemLeg == null || TotemHead == null && TotemLeg == null)
                    {
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
                            Instance.RegenerateRelativeHeightmap(true);
                        }
                    }
                    else
                    {
                        DateTime LastInteraction = UnixTimestamp.GetDateTimeFromUnixTimestamp(mLastUserInteraction);
                        bool CanGiveEffect = DateTime.Compare(LastInteraction.AddDays(1), DateTime.Now) <= 0;

                        TotemType Leg = TotemUtil.GetLegTypeFromInt(int.Parse(TotemLeg.Flags));
                        TotemType Head = TotemUtil.GetHeadTypeFromInt(int.Parse(TotemHead.Flags));
                        TotemColor Color = TotemUtil.GetHeadColorFromInt(int.Parse(TotemHead.Flags));
                        TotemPlanet Planet = TotemUtil.GetPlanetFromInt(int.Parse(Item.Flags));

                        int GiveEffectId = TotemUtil.GetEffectFromCombination(Planet, Head, Leg, Color);

                        if (GiveEffectId > 0 && CanGiveEffect)
                        {
                            AvatarEffect Effect = null;

                            if (Session.AvatarEffectCache.HasEffect(GiveEffectId))
                            {
                                Effect = Session.AvatarEffectCache.GetEffect(GiveEffectId);

                                if (Effect != null)
                                {
                                    Effect.AddToQuantity();
                                }
                            }
                            else
                            {
                                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                                {
                                    Effect = AvatarEffectFactory.CreateEffect(MySqlClient, Session.CharacterId, GiveEffectId, 86400);
                                    Session.AvatarEffectCache.Add(Effect);
                                }
                            }

                            if (Effect != null)
                            {
                                Session.SendData(UserEffectAddedComposer.Compose(Effect));
                            }

                            mLastUserInteraction = UnixTimestamp.GetCurrent();
                        }
                    }

                    break;
            }

            return true;
        }
    }
}
