using System;

namespace Snowlight.Communication.Outgoing
{
    public static class RoomUserRemovedComposer
    {
        public static ServerMessage Compose(uint Id)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.REMOVE_USER_FROM_ROOM);
            Message.AppendRawUInt32(Id);
            return Message;
        }
    }
}
