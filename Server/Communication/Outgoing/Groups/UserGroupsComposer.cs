using Snowlight.Game.Characters;
using Snowlight.Game.Groups;
using System;
using System.Collections.Generic;

namespace Snowlight.Communication.Outgoing
{
    public static class UserGroupsComposer
    {
        public static ServerMessage Compose(CharacterInfo UserInfo)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.ROOM_USER_GROUPS_LIST);
            Message.AppendInt32(GroupManager.GetUserGroups(UserInfo.Id).Count);

            foreach (int GroupId in GroupManager.GetUserGroups(UserInfo.Id))
            {
                Group AllGroups = GroupManager.GetGroup(GroupId);

                Message.AppendInt32(AllGroups.Id);
                Message.AppendStringWithBreak(AllGroups.Name);
                Message.AppendStringWithBreak(AllGroups.Badge);
                Message.AppendBoolean(UserInfo.FavoriteGroupId == GroupId);
            }
            return Message;
        }
    }
}
