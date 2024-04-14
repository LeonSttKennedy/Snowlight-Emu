using Snowlight.Game.Sessions;
using Snowlight.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public enum MarketplaceCanSell
    {
        CanSell = 1,
        NoTradingPrivilege = 2,
        TradingDisabled = 3,
        WithoutTokens = 4
    }
    public static class CatalogMarketplaceCanSellComposer
    {
        public static ServerMessage Compose(Session Session)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.MARKETPLACE_CAN_SELL);

            MarketplaceCanSell ErrorCode = MarketplaceCanSell.CanSell;

            if (!Session.HasRight("trade"))
            {
                ErrorCode = MarketplaceCanSell.NoTradingPrivilege;
            }

            if (!Session.CharacterInfo.AllowTrade)
            {
                ErrorCode = MarketplaceCanSell.TradingDisabled;
            }

            if (ServerSettings.MarketplaceTokensBuyEnabled && Session.CharacterInfo.MarketplaceTokens == 0)
            {
                ErrorCode = MarketplaceCanSell.WithoutTokens;
            }

            int TokensCount = ServerSettings.MarketplaceTokensBuyEnabled ? Session.CharacterInfo.MarketplaceTokens : int.MaxValue;

            Message.AppendInt32((int)ErrorCode); // Error Code
            Message.AppendInt32(TokensCount); // Token Count
            return Message;
        }
    }
}
