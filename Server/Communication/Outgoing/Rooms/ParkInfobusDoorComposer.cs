using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public static class ParkInfobusDoorComposer
    {
        public static ServerMessage Compose(int DoorStatus)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.PARK_INFOBUS_DOOR);
            Message.AppendInt32(DoorStatus); // 0 = CLOSED 1 = OPEN
            return Message;
        }
    }
}
