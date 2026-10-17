using System;

namespace Snowlight.Communication.Outgoing
{
    public static class RoomUserSpectatorComposer
    {
        public static ServerMessage Compose()
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.YOU_ARE_SPECTATOR);
            return Message;
        }
    }
}
