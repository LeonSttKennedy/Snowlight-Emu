using Snowlight.Game.Sessions;
using Snowlight.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public static class CatalogMarketplaceCanSellComposer
    {
        public static ServerMessage Compose(Session Session)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.MARKETPLACE_CAN_SELL);

            int ErrorCode = 1;

            if (!Session.CharacterInfo.AllowTrade)
            {
                ErrorCode = 2;
            }

            if (!Session.HasRight("trade"))
            {
                ErrorCode = 3;
            }

            if (ServerSettings.MarketplaceTokensBuyEnabled && Session.CharacterInfo.MarketplaceTokens == 0)
            {
                ErrorCode = 4;
            }

            int TokensCount = ServerSettings.MarketplaceTokensBuyEnabled ? Session.CharacterInfo.MarketplaceTokens : int.MaxValue;

            Message.AppendInt32(ErrorCode); // Error Code
            Message.AppendInt32(TokensCount); // Token Count
            return Message;
        }
    }
}
