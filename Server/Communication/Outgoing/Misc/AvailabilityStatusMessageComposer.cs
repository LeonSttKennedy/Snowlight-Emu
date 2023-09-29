using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public static class AvailabilityStatusMessageComposer
    {
        public static ServerMessage Compose()
        {
            // Thanks to Skylight emu developers
            ServerMessage Message = new ServerMessage(OpcodesOut.AVAILABILITY_STATUS);
            Message.AppendInt32(1); // Is Open, Unused
            Message.AppendInt32(0); // Trading Disabled
            return Message;
        }
    }
}
