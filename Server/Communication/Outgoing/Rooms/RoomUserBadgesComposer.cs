using System;
using System.Collections.Generic;

using Snowlight.Specialized;
using Snowlight.Game.Rights;

namespace Snowlight.Communication.Outgoing
{
    public static class RoomUserBadgesComposer
    {
        public static ServerMessage Compose(uint CharacterId, SortedDictionary<int, InventoryBadge> Badges)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.ROOM_USER_BADGES);
            Message.AppendUInt32(CharacterId);
            Message.AppendInt32(Badges.Count);

            foreach (InventoryBadge Badge in Badges.Values)
            {
                Message.AppendUInt32(Badge.Id);
                Message.AppendStringWithBreak(Badge.Definition.Code);
            }

            return Message;
        }
    }
}
