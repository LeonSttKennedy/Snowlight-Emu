using System;
using Snowlight.Game.Items;
using Snowlight.Game.Rooms;
using System.Collections.Generic;
using Snowlight.Game.Items.Wired;

namespace Snowlight.Communication.Outgoing
{
    public static class WiredFurniActionComposer
    {
        public static ServerMessage Compose(Item Item, RoomInstance Instance)
        {
            // com.sulake.habbo.communication.messages.incoming.userdefinedroomevents.WiredFurniActionEvent;
            ServerMessage Message = new ServerMessage(OpcodesOut.WIRED_FURNI_ACTION);

            Message.AppendInt32(0);
            Message.AppendInt32(5);  // Furni limit

            if (Item.WiredData.Data1.Contains("|"))
            {
                string[] Selected = Item.WiredData.Data1.Split('|');

                Message.AppendInt32(Selected.Length - 1); // Selected Furni Count
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
                Message.AppendUInt32(0);
            }

            Message.AppendUInt32(Item.Definition.SpriteId);
            Message.AppendUInt32(Item.Id);

            Message.AppendStringWithBreak(Item.WiredData.Data1);

            WiredActionTypes Type = WiredTypesUtil.ActionFromInt(Item.Definition.BehaviorData);

            if (WiredActionTypes.match_to_sshot == Type || WiredActionTypes.move_rotate == Type)
            {
                Message.AppendUInt32(3);  // Data Count
            }

            Message.AppendInt32(Item.WiredData.Data2);
            Message.AppendInt32(Item.WiredData.Data3);

            if (WiredActionTypes.match_to_sshot == Type || WiredActionTypes.move_rotate == Type)
            {
                Message.AppendInt32(Item.WiredData.Data4);
                Message.AppendUInt32(0);
            }
            int behavior = Item.Definition.BehaviorData;
            Message.AppendInt32(behavior);

            Message.AppendInt32(Item.WiredData.Time); // TIME

            List<Item> Items = Instance.WiredManager.ActionRequiresActor(Item.Definition.BehaviorData, Item.RoomPosition.GetVector2());

            Message.AppendInt32(Items.Count);
            foreach (Item Blocked in Items)
            {
                Message.AppendUInt32(Blocked.Definition.SpriteId);
            }

            return Message;
        }
    }
}