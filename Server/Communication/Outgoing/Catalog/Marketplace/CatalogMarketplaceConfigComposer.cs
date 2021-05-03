using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public static class CatalogMarketplaceConfigComposer
    {
        public static ServerMessage Compose() 
        {
            DateTime now = DateTime.Now;
            TimeSpan timeSpan = now - Program.ServerStarted;
            ServerMessage Message = new ServerMessage(OpcodesOut.MARKETPLACE_CONFIG);
            Message.AppendBoolean(true);
            Message.AppendInt32(1);
            Message.AppendInt32(1);
            Message.AppendInt32(5);
            Message.AppendInt32(1);
            Message.AppendInt32(10000);
            Message.AppendInt32(48);
            Message.AppendInt32(timeSpan.Days);
            return Message;
        }
    }
}
