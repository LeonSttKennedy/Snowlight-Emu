using System;

namespace Snowlight.Communication.Outgoing
{
    public static class InfobusClosedComposer
    {
        public static ServerMessage Compose(string InfobusMessage)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.INFOBUS_CLOSED);
            Message.AppendStringWithBreak(InfobusMessage);
            return Message;
        }
    }
}
