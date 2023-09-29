using Snowlight.Game.Catalog;
using Snowlight.Game.Items;
using Snowlight.Storage;
using Snowlight.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public static class CatalogMarketplaceSerializeOwnOffersComposer
    {
        public static ServerMessage Compose(uint CharacterID)
        {
            // IhHI`n~^II[EFPN[OKPA

            IEnumerable<MarketplaceOffers> UserOfferList = CatalogManager.MarketplaceOffers.Values.Where(O => O.UserId == CharacterID);
            int Profits = UserOfferList.Where(O => O.State == 2).Sum(O => O.AskingPrice);

            ServerMessage Message = new ServerMessage(OpcodesOut.MARKETPLACE_SERIALIZE_OWN_OFFERS);
            Message.AppendInt32(Profits);

            if (UserOfferList.Count() > 0)
            {
                Message.AppendInt32(UserOfferList.Count());

                foreach (MarketplaceOffers UserOffers in UserOfferList)
                {
                    double TimeLeft = UserOffers.Timestamp + (ServerSettings.MarketplaceOfferTotalHours * 60 * 60);
                    DateTime Future = UnixTimestamp.GetDateTimeFromUnixTimestamp(TimeLeft);
                    TimeSpan TSOffer = Future - DateTime.Now;
                    int Compare = DateTime.Compare(DateTime.Now, Future);

                    Message.AppendUInt32(UserOffers.Id);
                    Message.AppendInt32(UserOffers.State); // 1 = active, 2 = sold, 3 = expired
                    Message.AppendInt32(UserOffers.ItemType);
                    Message.AppendUInt32(UserOffers.Sprite);
                    Message.AppendStringWithBreak(UserOffers.ItemType == 2 ? UserOffers.ExtraData : string.Empty);
                    Message.AppendInt32(UserOffers.TotalPrice); // ??
                    Message.AppendInt32(Compare > -1 ? 0 : (int)TSOffer.TotalMinutes);
                    Message.AppendUInt32(UserOffers.Sprite);
                }
            }

            return Message;
        }
    }
}
