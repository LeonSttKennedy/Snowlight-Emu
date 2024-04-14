using System;

namespace Snowlight.Communication.Outgoing
{
    public enum MarketplaceSellOk
    {
        SellOK = 1,
        TechinicalError = 2,
        MarketplaceDisabled = 3,
        ItemWasAdded = 4
    }

    public static class CatalogMarketplaceSellItemComposer
    {

        public static ServerMessage Compose(MarketplaceSellOk Id)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.MARKETPLACE_SELL_OK);
            Message.AppendInt32((int)Id);
            return Message;
        }
    }
}
