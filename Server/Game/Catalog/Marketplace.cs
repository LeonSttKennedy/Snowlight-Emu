using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Snowlight.Communication;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Sessions;
using Snowlight.Storage;
using Snowlight.Game.Items;

namespace Snowlight.Game.Catalog
{
    class Marketplace
    {
        public List<uint> MarketItemKeys;
        public List<MarketplaceItems> MarketplaceItems;
        public Dictionary<int, int> MarketAverages;
        public Dictionary<int, int> MarketCounts;

        public Marketplace()
        {
            this.MarketItemKeys = new List<uint>();
            this.MarketplaceItems = new List<MarketplaceItems>();
            this.MarketAverages = new Dictionary<int, int>();
            this.MarketCounts = new Dictionary<int, int>();
        }

        public bool CanSellItem(Item UserItem)
        {
            return UserItem.CanTrade && UserItem.CanSell;
        }

        public int CalculateComissionPrice(float SellingPrice)
        {
            return Convert.ToInt32(Math.Ceiling(SellingPrice / 100 * 1));
        }
        public Double FormatTimestamp()
        {
            return UnixTimestamp.GetCurrent() - 172800;
        }
        internal string FormatTimestampString()
        {
            return this.FormatTimestamp().ToString().Split(new char[] { ',' })[0];
        }
        public int OfferCountForSprite(int SpriteID)
        {
            Dictionary<int, MarketplaceItems> dictionary = new Dictionary<int, MarketplaceItems>();
            Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
            foreach (MarketplaceItems current in this.MarketplaceItems)
            {
                if (dictionary.ContainsKey(current.Sprite))
                {
                    if (dictionary[current.Sprite].TotalPrice > current.TotalPrice)
                    {
                        dictionary.Remove(current.Sprite);
                        dictionary.Add(current.Sprite, current);
                    }
                    int num = dictionary2[current.Sprite];
                    dictionary2.Remove(current.Sprite);
                    dictionary2.Add(current.Sprite, num + 1);
                }
                else
                {
                    dictionary.Add(current.Sprite, current);
                    dictionary2.Add(current.Sprite, 1);
                }
            }
            if (dictionary2.ContainsKey(SpriteID))
            {
                return dictionary2[SpriteID]; 
            }
            return 0;
        }
        public int AvgPriceForSprite(int SpriteID)
        {
            int num = 0;
            int num2 = 0;
            if (this.MarketAverages.ContainsKey(SpriteID) && this.MarketCounts.ContainsKey(SpriteID))
            {
                if (this.MarketCounts[SpriteID] > 0)
                {
                    return this.MarketAverages[SpriteID] / this.MarketCounts[SpriteID];
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                try
                {
                    using (SqlDatabaseClient adapter = SqlDatabaseManager.GetClient())
                    {
                        DataRow Row = adapter.ExecuteQueryRow("SELECT * FROM catalog_marketplace_data WHERE sprite = '" + SpriteID + "' AND daysago = 0 LIMIT 1;");
                        num = (int)Row["avgprice"];
                        num2 = (int)Row["sold"];
                        
                    }
                }
                catch { }

                this.MarketAverages.Add(SpriteID, num);
                this.MarketCounts.Add(SpriteID, num2);
                if (num2 > 0)
                {
                    return (int)Math.Ceiling((double)((float)(num / num2)));
                }
                else
                {
                    return 0;
                }
            }
        }
        public void SellItem(Session Session, uint ItemID, int SellingPrice)
        {
            bool SellOK = true;
            Item UserItem = Session.InventoryCache.GetItem(ItemID);
            if (UserItem == null || SellingPrice > 10000 || !CanSellItem(UserItem))
            {
                SellOK = false;
            }
            else 
            {
                int Comission = CalculateComissionPrice(SellingPrice);
                int TotalPrice = Comission + SellingPrice;
                int ItemType = 1;

                if (UserItem.Definition.TypeLetter == "i")
                {
                    ItemType++;
                }

                using (SqlDatabaseClient dbClient = SqlDatabaseManager.GetClient())
                {
                    dbClient.SetParameter("public_name", UserItem.Definition.Name);
                    dbClient.SetParameter("extra_data", UserItem.Flags);
                    dbClient.ExecuteQueryTable("INSERT INTO catalog_marketplace_offers (item_id,user_id,asking_price,total_price,public_name,sprite_id,item_type,timestamp,extra_data) VALUES ('" + UserItem.DefinitionId + "','" + Session.CharacterId + "','" + SellingPrice + "','" + TotalPrice + "',@public_name,'" + UserItem.Definition.SpriteId + "','" + ItemType + "','" + UnixTimestamp.GetCurrent() + "',@extra_data)");

                    UserItem.RemovePermanently(dbClient);
                    Session.CharacterInfo.UpdateMarketplaceTokens(dbClient, -1);
                    Session.InventoryCache.RemoveItem(ItemID);
                    Session.SendData(InventoryRefreshComposer.Compose());
                    Session.SendData(CatalogMarketplaceSellItemComposer.Compose(SellOK));
                }
            }
        }

        public ServerMessage SerializeOffersNew(int MinCost, int MaxCost, string SearchQuery, int FilterMode)
        {
            // IgI`UJUIIY~JX]gXoAJISA

            StringBuilder WhereClause = new StringBuilder();

            WhereClause.Append("WHERE state = '1' AND timestamp >= " + FormatTimestampString());

            if (MinCost >= 0)
            {
                WhereClause.Append(" AND total_price >= " + MinCost);
            }

            if (MaxCost >= 0)
            {
                WhereClause.Append(" AND total_price <= " + MaxCost);
            }

            if (SearchQuery.Length >= 1)
            {
                WhereClause.Append(" AND public_name LIKE '%" + SearchQuery + "%'");
            }
            switch (FilterMode)
            {
                case 1:
                    WhereClause.Append(" ORDER BY asking_price DESC");
                    break;

                case 2:
                    WhereClause.Append(" ORDER BY asking_price ASC");
                    break;
            }

            using (SqlDatabaseClient dbClient = SqlDatabaseManager.GetClient())
            {
                DataTable Data = dbClient.ExecuteQueryTable("SELECT * FROM catalog_marketplace_offers " + WhereClause.ToString());
                this.MarketplaceItems.Clear();
                this.MarketItemKeys.Clear();
                if (Data != null)
                {
                    foreach (DataRow Row in Data.Rows)
                    {
                        if (!this.MarketItemKeys.Contains((uint)Row["offer_id"]))
                        {
                            MarketplaceItems item = new MarketplaceItems(Convert.ToInt32(Row["offer_id"]), Convert.ToInt32(Row["sprite_id"]), Convert.ToInt32(Row["total_price"]), int.Parse(Row["item_type"].ToString()), Convert.ToInt32(Row["limited_number"]), Convert.ToInt32(Row["limited_stack"]));
                            this.MarketItemKeys.Add((uint)Row["offer_id"]);
                            this.MarketplaceItems.Add(item);
                        }
                    }
                }
            }

            Dictionary<int, MarketplaceItems> MarketItems = new Dictionary<int, MarketplaceItems>();
            Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
            foreach (MarketplaceItems Item in this.MarketplaceItems)
            {
                if (MarketItems.ContainsKey(Item.Sprite))
                {
                    if (MarketItems[Item.Sprite].TotalPrice > Item.TotalPrice)
                    {
                        MarketItems.Remove(Item.Sprite);
                        MarketItems.Add(Item.Sprite, Item);
                    }
                    int num = dictionary2[Item.Sprite];
                    dictionary2.Remove(Item.Sprite);
                    dictionary2.Add(Item.Sprite, num + 1);
                }
                else
                {
                    MarketItems.Add(Item.Sprite, Item);
                    dictionary2.Add(Item.Sprite, 1);
                }
            }

            return this.SerializeOffers(MinCost, MaxCost, MarketItems, dictionary2);
        }
        public ServerMessage SerializeOffers(int MinCost, int MaxCost, Dictionary<int, MarketplaceItems> dictionary, Dictionary<int, int> dictionary2)
        {
            ServerMessage Message = new ServerMessage(615);
            Message.AppendInt32(dictionary.Count);
            if (dictionary.Count > 0)
            {
                foreach (KeyValuePair<int, MarketplaceItems> pair in dictionary)
                {
                    Message.AppendInt32(pair.Value.OfferID);
                    Message.AppendInt32(1);
                    Message.AppendInt32(pair.Value.ItemType);
                    Message.AppendInt32(pair.Value.Sprite);
                    Message.AppendInt32(256);
                    Message.AppendStringWithBreak("");
                    Message.AppendInt32(pair.Value.TotalPrice);
                    Message.AppendInt32(pair.Value.Sprite);
                    Message.AppendInt32(this.AvgPriceForSprite(pair.Value.Sprite));
                    Message.AppendInt32(dictionary2[pair.Value.Sprite]);
                }
            }
            Message.AppendInt32(dictionary.Count);
            return Message;
        }
    }
}
