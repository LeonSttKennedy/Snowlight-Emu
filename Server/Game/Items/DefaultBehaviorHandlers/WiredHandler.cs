using System;
using System.Collections.Generic;

using Snowlight.Game.Sessions;
using Snowlight.Game.Rooms;
using Snowlight.Specialized;
using Snowlight.Storage;
using Snowlight.Game.Misc;
using Snowlight.Communication.Outgoing;
using Snowlight.Communication;
using Snowlight.Game.Items.Wired;

namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class WiredHandler
    {
        public static List<Item> mSelectedItems;
        public static string mData;

        public static void Register()
        {
            mSelectedItems = new List<Item>();
            mData = string.Empty;

            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.WiredCondition, new ItemEventHandler(HandleWiredTrigger));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.WiredEffect, new ItemEventHandler(HandleWiredTrigger));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.WiredTrigger, new ItemEventHandler(HandleWiredTrigger));
        }

         private static bool HandleWiredTrigger(Session Session, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Placed:
                case ItemEventType.InstanceLoaded:
                    
                    Item.WiredData = Instance.WiredManager.LoadWired(Item.Id, Item.Definition.BehaviorData);
                    if (WiredTypesUtil.TriggerFromInt(Item.Definition.BehaviorData) == WiredTriggerTypes.periodically)
                    {
                        Item.RequestUpdate(Item.WiredData.Data2);
                    }

                    break;
                
                case ItemEventType.Removing:
                    
                    using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                    {
                        Instance.WiredManager.RemoveWired(Item.Id, MySqlClient);
                    }

                    Instance.WiredManager.DeRegisterWalkItem(Item.Id);

                    break;
                
                case ItemEventType.UpdateTick:
                    
                    if (Item.Definition.Behavior == ItemBehavior.WiredTrigger)
                    {
                        switch (WiredTypesUtil.TriggerFromInt(Item.Definition.BehaviorData))
                        {
                            case WiredTriggerTypes.periodically:

                                Instance.WiredManager.ExecuteActions(Item, null);
                                Item.RequestUpdate(Item.WiredData.Data2);

                                break;

                            case WiredTriggerTypes.at_given_time:

                                Instance.WiredManager.ExecuteActions(Item, null);

                                break;
                        }

                        return true;
                    }

                    Item.BroadcastStateUpdate(Instance);

                    break;

                case ItemEventType.Interact:

                    if (!Instance.CheckUserRights(Session))
                    {
                        return true;
                    }

                    switch (Item.Definition.Behavior)
                    {
                        case ItemBehavior.WiredTrigger:
                            Session.SendData(WiredFurniTriggerComposer.Compose(Item, Instance));
                            break;

                        case ItemBehavior.WiredEffect:
                            Session.SendData(WiredFurniActionComposer.Compose(Item, Instance));
                            break;

                        case ItemBehavior.WiredCondition:

                            ServerMessage Condition = new ServerMessage(OpcodesOut.WIRED_FURNI_CONDITION);
                            Condition.AppendBoolean(false); //check box toggling
                            Condition.AppendInt32(5); //furni limit
                            Condition.AppendInt32(mSelectedItems.Count); //furni count
                            if (mSelectedItems.Count > 0)
                            {
                                foreach (Item item_ in mSelectedItems)
                                {
                                    Condition.AppendUInt32(item_.Id);
                                }
                            }
                            Condition.AppendUInt32(Item.Definition.SpriteId); //sprite id, show the help thing
                            Condition.AppendUInt32(Item.Id); //item id
                            Condition.AppendStringWithBreak(string.Empty); //data
                            Condition.AppendInt32(0); //extra data count
                            Condition.AppendInt32(0); //delay, not work with this wired

                            Condition.AppendInt32(Item.Definition.BehaviorData); //type
                            Condition.AppendInt32(0); //conflicts count
                            Session.SendData(Condition);
                            break;
                    }
                    break;
            }

            return true;
        }
    }
}
