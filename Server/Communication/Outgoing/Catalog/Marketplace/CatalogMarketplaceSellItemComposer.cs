using System;

namespace Snowlight.Communication.Outgoing
{
    public static class CatalogMarketplaceSellItemComposer
    {
        public static ServerMessage Compose(bool SellOk)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.MARKETPLACE_SELL_OK);
            Message.AppendBoolean(SellOk);
            return Message;
        }
    }
}
