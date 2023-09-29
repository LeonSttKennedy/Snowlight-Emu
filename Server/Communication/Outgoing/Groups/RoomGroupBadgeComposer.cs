using Snowlight.Game.Groups;
using System;
using System.Collections.Generic;

namespace Snowlight.Communication.Outgoing
{
    public static class RoomGroupBadgeComposer
    {
        public static ServerMessage Compose(uint UserId, Group Group)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.ROOM_GROUP_BADGES);
            Message.AppendInt32(GroupManager.GetUserGroups(UserId).Count);              // User Group Count
            Message.AppendInt32(Group != null ? Group.Id : 0);                          // group id
            Message.AppendStringWithBreak(Group != null ? Group.Badge : string.Empty);  // badge code
            return Message;
        }
    }
}
