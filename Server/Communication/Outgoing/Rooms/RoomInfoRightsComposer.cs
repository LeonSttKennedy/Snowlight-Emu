﻿using System;

namespace Snowlight.Communication.Outgoing
{
    public static class RoomInfoRightsComposer
    {
        public static ServerMessage Compose(bool IsFlat, uint RoomId, bool HasOwnerRights, string PubInternalName, int PubInternalId)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.ROOM_INFO_RIGHTS);
            Message.AppendBoolean(IsFlat);

            if (IsFlat)
            {
                Message.AppendUInt32(RoomId);
                Message.AppendBoolean(HasOwnerRights);
            }
            else
            {
                Message.AppendStringWithBreak(PubInternalName);
                Message.AppendInt32(PubInternalId);
            }

            return Message;
        }
    }
}
