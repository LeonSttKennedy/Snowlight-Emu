﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Snowlight.Specialized;
using Snowlight.Game.Rights;

namespace Snowlight.Communication.Outgoing
{
    public static class UserBadgeInventoryComposer
    {
        public static ServerMessage Compose(List<InventoryBadge> Badges, SortedDictionary<int, InventoryBadge> EquippedBadges)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.USER_BADGE_INVENTORY);
            Message.AppendInt32(Badges.Count);

            foreach (InventoryBadge Badge in Badges)
            {
                Message.AppendUInt32(Badge.Id);
                Message.AppendStringWithBreak(Badge.Definition.Code);
            }

            Message.AppendInt32(EquippedBadges.Count);

            foreach (KeyValuePair<int, InventoryBadge> EquippedBadge in EquippedBadges)
            {
                Message.AppendInt32(EquippedBadge.Key);
                Message.AppendStringWithBreak(EquippedBadge.Value.Definition.Code);
            }

            return Message;
        }
    }
}
