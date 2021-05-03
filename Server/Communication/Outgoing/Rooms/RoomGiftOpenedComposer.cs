using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication
{
    public static class RoomGiftOpenedComposer
    {
        public static ServerMessage Compose(string ItemType, uint SpriteId, string ItemName)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.ROOM_GIFT_OPENED);
            Message.AppendStringWithBreak(ItemType);
            Message.AppendUInt32(SpriteId);
            Message.AppendStringWithBreak(ItemName);
            return Message;
        }
    }
}
