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
            int MarketplaceTokens = Session.HasRight("club_vip") ? ServerSettings.MarketplacePremiumTokens 
                : ServerSettings.MarketplaceNormalTokens;
            
            ServerMessage Message = new ServerMessage(OpcodesOut.MARKETPLACE_CONFIG);
            Message.AppendBoolean(ServerSettings.MarketplaceEnabled);                   //allow marketplace
            Message.AppendInt32(ServerSettings.MarketplaceTax);                         //compremission
            Message.AppendInt32(ServerSettings.MarketplaceTokensPrice);                 //tokens price
            Message.AppendInt32(MarketplaceTokens);                                     //buy tokens at once
            Message.AppendInt32(ServerSettings.MarketplaceMinPrice);                    //min price
            Message.AppendInt32(ServerSettings.MarketplaceMaxPrice);                    //max price
            Message.AppendInt32(ServerSettings.MarketplaceOfferTotalHours);             //offer active (hours)
            Message.AppendInt32(ServerSettings.MarketplaceAvarageDays);                 //avarage price (days)
            return Message;
        }
    }
}
