using System;

namespace Snowlight.Communication.Outgoing
{
    public static class SoundSettingsComposer
    {
        public static ServerMessage Compose(int Volume)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.CLIENT_CONFIG);
            Message.AppendInt32(Volume);
            return Message;
        }
    }
}
