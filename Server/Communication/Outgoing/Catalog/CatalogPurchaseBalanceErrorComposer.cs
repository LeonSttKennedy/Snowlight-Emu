using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public static class CatalogPurchaseBalanceErrorComposer
    {
        public static ServerMessage Composer(bool Credits, bool Pixels)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.CATALOG_PURCHASE_BALANCE_ERROR);
            Message.AppendBoolean(Credits); // Credits error
            Message.AppendBoolean(Pixels);  // Pixel error
            return Message;
        }
    }
}
