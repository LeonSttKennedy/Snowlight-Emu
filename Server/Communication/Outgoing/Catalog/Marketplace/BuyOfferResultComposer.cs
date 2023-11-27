using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public enum MarketplaceError
    {
        Sucess = 1,
        AllSoldOut = 2,
        UpdateItem = 3,
        NoCredits = 4
    }

    public static class CatalogMarketplaceBuyOfferResultComposer
    {
        public static ServerMessage Compose(MarketplaceError Result, uint NewId = 0, int Price = 0, uint OldId = 0)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.MARKETPLACE_BUY_OFFER_RESULT);
            Message.AppendInt32((int)Result); //result, 1 = success, 2 = all sold out, 3 = update item and show confirmation, 4 = no credits
            Message.AppendUInt32(NewId); //new id
            Message.AppendInt32(Price); //price
            Message.AppendUInt32(OldId); //old id
            return Message;
        }
    }
}
