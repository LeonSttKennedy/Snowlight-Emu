using Snowlight.Game.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public static class CatalogMarketplaceSerializeOffersComposer
    {
        private static MarketplaceManager Marketplace;

        public static ServerMessage Compose(List<IGrouping<uint, MarketplaceOffers>> dictionary)
        {
            Marketplace = new MarketplaceManager();

            ServerMessage Message = new ServerMessage(OpcodesOut.MARKETPLACE_SERIALIZE_OFFERS);
            Message.AppendInt32(dictionary.Count > 500 ? 500 : dictionary.Count);
            foreach (IGrouping<uint, MarketplaceOffers> offerGroup in dictionary.Take(500))
            {
                foreach (MarketplaceOffers offer in offerGroup.Take(1))
                {
                    Message.AppendInt32(offer.OfferID);
                    Message.AppendInt32(1);
                    Message.AppendInt32(offer.ItemType);
                    Message.AppendUInt32(offer.Sprite);
                    Message.AppendInt32(256);
                    Message.AppendStringWithBreak("");
                    Message.AppendInt32(offer.TotalPrice);
                    Message.AppendUInt32(offer.Sprite);
                    Message.AppendInt32(Marketplace.AvgPriceForSprite(offer.Sprite));
                    Message.AppendInt32(offerGroup.Count(g => g.Sprite == offer.Sprite));
                }
            }
            Message.AppendInt32(dictionary.Count);
            return Message;
        }
    }
}
