using Snowlight.Game.Groups;
using Snowlight.Game.Rooms;
using System;

namespace Snowlight.Communication.Outgoing
{
    public static class GroupInfoComposer
    {
        public static ServerMessage Compose(Group Group, uint UserId, bool IsFavorite)
        {
            RoomInfo Info = RoomInfoLoader.GetRoomInfo(Group.RoomId);

            ServerMessage Message = new ServerMessage(OpcodesOut.ROOM_GROUP_INFO);
            Message.AppendInt32(Group.Id);                              // Group ID 
            Message.AppendStringWithBreak(Group.Name);                  // Group Name  
            Message.AppendStringWithBreak(Group.Desc);                  // Group Desc 
            Message.AppendStringWithBreak(Group.Badge);                 // Badge Hash (habbo-imagining/badge)
            Message.AppendUInt32(Group.RoomId);                         // Room Id 
            Message.AppendStringWithBreak(Info.Name);                   // Room Name 
            Message.AppendInt32(Group.ResquestStatus(UserId));          // Request Status
            Message.AppendInt32(Group.MembershipList.Count);            // Members
            Message.AppendBoolean(IsFavorite);                          // Is Favorite
            return Message;
        }
    }
}
