﻿using System;
using Snowlight.Game.Items;
using Snowlight.Game.Items.Wired;
using Snowlight.Game.Rooms;
using System.Collections.Generic;

namespace Snowlight.Communication.Outgoing
{
    public static class WiredFurniTriggerComposer
    {
        public static ServerMessage Compose(Item Item, RoomInstance Instance)
        {   
            // com.sulake.habbo.communication.messages.incoming.userdefinedroomevents.WiredFurniTriggerEvent;
            ServerMessage Message = new ServerMessage(OpcodesOut.WIRED_FURNI_TRIGGER);
            Message.AppendInt32(0);
            Message.AppendInt32(5);

            if (Item.WiredData.Data1.Contains("|"))
            {
                string[] Selected = Item.WiredData.Data1.Split('|');
                Message.AppendInt32(Selected.Length - 1);
                foreach (string selected in Selected)
                {
                    if (selected == "")
                    {
                        continue;
                    }

                    int.TryParse(selected, out int result);
                    Message.AppendInt32(result);
                }
            }
            else
            {
                Message.AppendInt32(0);
            }

            Message.AppendUInt32(Item.Definition.SpriteId);
            Message.AppendInt32((int)Item.Id);
            Message.AppendStringWithBreak(Item.WiredData.Data1);
            Message.AppendInt32(1);
            Message.AppendInt32(Item.WiredData.Data2);
            Message.AppendInt32(0);
            Message.AppendInt32(Item.Definition.BehaviorData);
            List<Item> Items = Instance.WiredManager.TriggerRequiresActor(Item.Definition.BehaviorData, Item.RoomPosition.GetVector2());
            Message.AppendInt32(Items.Count); // Contains Event that needs a User, but there is a trigger, that isn't triggered by a User
            foreach (Item Blocked in Items)
            {
                Message.AppendUInt32(Blocked.Definition.SpriteId);
            }
            return Message;
        }
    }
}