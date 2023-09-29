using Snowlight.Game.Catalog;
using Snowlight.Game.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public static class CatalogMarketplaceSerializeOffersComposer
    {
        public static ServerMessage Compose(List<IGrouping<uint, MarketplaceOffers>> dictionary)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.MARKETPLACE_SERIALIZE_OFFERS);
            Message.AppendInt32(dictionary.Count > 500 ? 500 : dictionary.Count);
            foreach (IGrouping<uint, MarketplaceOffers> offerGroup in dictionary.Take(500))
            {
                foreach (MarketplaceOffers offer in offerGroup.OrderBy(O => O.TotalPrice).Take(1))
                {
                    string extradata = offer.ItemType == 2 ? offer.ExtraData : string.Empty;
                    int AvaragePrice = MarketplaceManager.AveragePriceForItem(offer.ItemType, offer.Sprite, offer.ExtraData);

                    Message.AppendUInt32(offer.Id);
                    Message.AppendInt32(offer.State);
                    Message.AppendInt32(offer.ItemType);
                    Message.AppendUInt32(offer.Sprite);
                    Message.AppendStringWithBreak(extradata);
                    Message.AppendInt32(offer.TotalPrice);
                    Message.AppendUInt32(offer.Sprite);
                    Message.AppendInt32(AvaragePrice);
                    Message.AppendInt32(offer.CountOffers());
                }
            }
            Message.AppendInt32(dictionary.Count);
            return Message;
        }
    }
}
