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
        public static ServerMessage Compose(int OfferCount, uint Sprite, int ItemType)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.MARKETPLACE_ITEM_STATS);
            Message.AppendInt32(1);
            Message.AppendInt32(OfferCount);
            Dictionary<int, DataRow> dictionary = new Dictionary<int, DataRow>();
            DataTable Table = null;
            using (SqlDatabaseClient adapter = SqlDatabaseManager.GetClient())
            {
                Table = adapter.ExecuteQueryTable("SELECT * FROM catalog_marketplace_data WHERE daysago > -30 AND sprite_id = " + Sprite + " LIMIT 30;");
            }
            if (Table != null)
            {
                foreach (DataRow dataRow in Table.Rows)
                {
                    dictionary.Add(Convert.ToInt32(dataRow["daysago"]), dataRow);
                }
            }
            Message.AppendInt32(30);
            Message.AppendInt32(29);
            for (int i = -29; i < 0; i++)
            {
                Message.AppendInt32(i);
                if (dictionary.ContainsKey(i + 1))
                {
                    Message.AppendInt32(Convert.ToInt32(dictionary[i + 1]["avgprice"]) / Convert.ToInt32(dictionary[i + 1]["sold"]));
                    Message.AppendInt32(Convert.ToInt32(dictionary[i + 1]["sold"]));
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