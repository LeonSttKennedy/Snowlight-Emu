using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Snowlight.Util;
using Snowlight.Game.Sessions;

namespace Snowlight.Communication.Outgoing
{
    public static class CatalogMarketplaceConfigComposer
    {
        public static ServerMessage Compose(Session Session) 
        {
            /*DateTime now = DateTime.Now;
            TimeSpan timeSpan = now - Program.ServerStarted;*/
            ServerMessage Message = new ServerMessage(OpcodesOut.MARKETPLACE_CONFIG);
            Message.AppendBoolean(ServerSettings.MarketplaceEnabled);   //allow marketplace
            Message.AppendInt32(ServerSettings.MarketplaceTax);         //compremission
            Message.AppendInt32(ServerSettings.MarketplaceTokensPrice); //tokens price
            Message.AppendInt32(Session.HasHcOrVip() ? ServerSettings.MarketplacePremiumTokens : ServerSettings.MarketplaceNormalTokens); //buy tokens at once
            Message.AppendInt32(ServerSettings.MarketplaceMinPrice);    //min price
            Message.AppendInt32(ServerSettings.MarketplaceMaxPrice);    //max price
            Message.AppendInt32(ServerSettings.MarketplaceOfferTotalHours); //offer active (hours)
            Message.AppendInt32(ServerSettings.MarketplaceAvarageDays); //avarage price (days)
            return Message;
        }
    }
}
