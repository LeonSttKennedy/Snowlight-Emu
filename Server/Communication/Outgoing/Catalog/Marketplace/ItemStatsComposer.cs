using Snowlight.Game.Catalog;
using Snowlight.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public static class CatalogMarketplaceItemStatsComposer
    {
        public static ServerMessage Compose(int Average, int OfferCount, uint Sprite, int ItemType)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.MARKETPLACE_ITEM_STATS);
            Message.AppendInt32(Average);
            Message.AppendInt32(OfferCount);
            Message.AppendInt32(30);
            Message.AppendInt32(29);

            List<IGrouping<DateTime, MarketplaceAvarage>> Avarages = CatalogManager.MarketplaceAvarages.ContainsKey(Sprite) ?
                CatalogManager.MarketplaceAvarages[Sprite].OrderByDescending(s => s.SoldTimeStamp).GroupBy(s => s.SoldTimeStamp.Date).ToList() :
                new List<IGrouping<DateTime, MarketplaceAvarage>>();

            for (int i = -29; i < 0; i++)
            {
                Message.AppendInt32(i);
                DateTime date = DateTime.Now.AddDays(i).Date;

                if (Avarages.Any(o => o.Key == date))
                {
                    IEnumerable<MarketplaceAvarage> items = Avarages.Where(o => o.Key == date).SelectMany(g => g);
                    int price = items.Sum(s => s.TotalPrice) / items.Count();

                    Message.AppendInt32(price); //price
                    Message.AppendInt32(items.Count()); //trade
                }
                else
                {
                    Message.AppendInt32(0);
                    Message.AppendInt32(0);
                }
            }

            Message.AppendInt32(ItemType);
            Message.AppendUInt32(Sprite);
            return Message;
        }
    }
}