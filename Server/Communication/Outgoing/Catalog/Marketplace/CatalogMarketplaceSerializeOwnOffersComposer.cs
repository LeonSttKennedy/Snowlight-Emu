using Snowlight.Game.Catalog;
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
            int Profits = 0;

            using (SqlDatabaseClient dbClient = SqlDatabaseManager.GetClient())
            {
                DataTable Data = dbClient.ExecuteQueryTable("SELECT * FROM catalog_marketplace_offers WHERE user_id = '" + CharacterID + "'");
                string RawProfit = dbClient.ExecuteQueryRow("SELECT SUM(asking_price) FROM catalog_marketplace_offers WHERE state = '2' AND user_id = '" + CharacterID + "'")[0].ToString();

                if (RawProfit.Length > 0)
                {
                    Profits = int.Parse(RawProfit);
                }
                ServerMessage Message = new ServerMessage(OpcodesOut.MARKETPLACE_SERIALIZE_OWN_OFFERS);
                Message.AppendInt32(Profits);
                if (Data != null)
                {
                    Message.AppendInt32(Data.Rows.Count);
                    foreach (DataRow Row in Data.Rows)
                    {
                        // IhHI`n~^II[EFPN[OKPA

                        int MinutesLeft = (int)Math.Floor((((double)Row["timestamp"] + (ServerSettings.MarketplaceOfferTotalHours * 3600))) - UnixTimestamp.GetCurrent()) / 60;
                        int state = int.Parse(Row["state"].ToString());

                        if (MinutesLeft <= 0)
                        {
                            state = 3;
                            MinutesLeft = 0;
                        }

                        Message.AppendUInt32((uint)Row["offer_id"]);
                        Message.AppendInt32(state); // 1 = active, 2 = sold, 3 = expired
                        Message.AppendInt32(int.Parse(Row["item_type"].ToString())); // always 1 (??)
                        Message.AppendInt32((int)Row["sprite_id"]);
                        Message.AppendStringWithBreak("");
                        Message.AppendInt32((int)Row["total_price"]); // ??
                        Message.AppendInt32(MinutesLeft);
                        Message.AppendInt32((int)Row["sprite_id"]);
                    }
                }
                Message.AppendInt32(0);
                return Message;
            }
        }
    }
}
