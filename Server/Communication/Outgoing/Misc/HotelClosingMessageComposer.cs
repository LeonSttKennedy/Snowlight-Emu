using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public static class HotelClosingMessageComposer
    {
        public static ServerMessage Compose(int Minutes)
        {
            // Thanks to Skylight emu developers
            ServerMessage Message = new ServerMessage(OpcodesOut.HOTEL_CLOSING_MESSAGE);
            Message.AppendInt32(Minutes);
            return Message;
        }
    }
}

