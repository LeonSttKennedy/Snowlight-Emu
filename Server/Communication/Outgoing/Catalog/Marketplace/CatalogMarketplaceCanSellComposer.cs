using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public static class CatalogMarketplaceCanSellComposer
    {
        public static ServerMessage Compose(int TotalTickets)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.MARKETPLACE_CAN_SELL);
            if (TotalTickets > 0)
            {
                Message.AppendBoolean(true);
                Message.AppendInt32(TotalTickets);
            }
            else if (TotalTickets == 0)
            {
                Message.AppendInt32(4);
                Message.AppendBoolean(false);
            }

            return Message;
        }
    }
}
