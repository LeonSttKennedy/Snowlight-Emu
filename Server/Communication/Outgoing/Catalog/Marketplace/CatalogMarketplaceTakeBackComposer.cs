using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public static class CatalogMarketplaceTakeBackComposer
    {
        public static ServerMessage Compose(uint OfferId)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.MARKETPLACE_TAKE_BACK);
            Message.AppendUInt32(OfferId);
            Message.AppendBoolean(true);
            return Message;
        }
    }
}
