using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Snowlight.Util;

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
            Message.AppendInt32(ServerSettings.MarketplaceTax);
            Message.AppendInt32(1);
            Message.AppendInt32(5);
            Message.AppendInt32(1);
            Message.AppendInt32(ServerSettings.MarketplaceMaxPrice);
            Message.AppendInt32(48);
            Message.AppendInt32(timeSpan.Days);
            return Message;
        }
    }
}
