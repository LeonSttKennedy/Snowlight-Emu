using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Items;
using Snowlight.Game.Sessions;
using Snowlight.Communication;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Catalog
{
    class MarketplaceManager
    {
        public Dictionary<uint, int> mMarketAverages;
        public Dictionary<uint, int> mMarketCounts;

        public Dictionary<uint, MarketplaceOffers> mMarketplaceOffers;

        public MarketplaceManager()
        {
            this.mMarketAverages = new Dictionary<uint, int>();
            this.mMarketCounts = new Dictionary<uint, int>();

            this.mMarketplaceOffers = new Dictionary<uint, MarketplaceOffers>();
        }

        public int CalculateComissionPrice(float SellingPrice)
        {
            double Double = SellingPrice / 100.0;
            return Convert.ToInt32(Math.Ceiling(Double * ServerSettings.MarketplaceTax));
        }
        public bool CanSellItem(Item UserItem)
        {
            return UserItem.CanTrade && UserItem.CanSell;
        }
        public double FormatTimestamp()
        {
            return UnixTimestamp.GetCurrent() - 172800;
        }
        public string FormatTimestampString()
        {
            return FormatTimestamp().ToString().Split(new char[] { ',' })[0];
        }

        public int AvgPriceForSprite(uint SpriteID)
        {
            int num = 0;
            int num2 = 0;
            if (mMarketAverages.ContainsKey(SpriteID) && mMarketCounts.ContainsKey(SpriteID))
            {
                if (mMarketCounts[SpriteID] > 0)
                {
                    return mMarketAverages[SpriteID] / mMarketCounts[SpriteID];
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

                mMarketAverages.Add(SpriteID, num);
                mMarketCounts.Add(SpriteID, num2);
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
        public int OfferCountForSprite(uint SpriteID)
        {
            Dictionary<uint, MarketplaceOffers> dictionary = new Dictionary<uint, MarketplaceOffers>();
            Dictionary<uint, int> dictionary2 = new Dictionary<uint, int>();
            foreach (MarketplaceOffers current in mMarketplaceOffers.Values)
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
        public void Purchase(Session Session, SqlDatabaseClient MySqlClient, uint OfferId)
        {
            DataRow Row = MySqlClient.ExecuteQueryRow("SELECT * FROM catalog_marketplace_offers WHERE offer_id = '" + OfferId + "' LIMIT 1");

            if (Row == null || (string)Row["state"] != "1" || (double)Row["timestamp"] <= FormatTimestamp())
            {
                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("catalog_marketplace_error")));
                return;
            }

            if ((uint)Row["user_id"] == Session.CharacterInfo.Id)
            {
                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("catalog_marketplace_boosting_error")));
                return;
            }

            if (Session.CharacterInfo.CreditsBalance < (int)Row["total_price"])
            {
                return;
            }

            ItemDefinition UserItem = ItemDefinitionManager.GetDefinition((uint)Row["item_id"]);
            if (UserItem == null)
            {
                return;
            }

            MySqlClient.ExecuteQueryTable("UPDATE catalog_marketplace_offers SET state = '2' WHERE offer_id = '" + OfferId + "' LIMIT 1");

            DataRow MarketData = MySqlClient.ExecuteQueryRow("SELECT * FROM catalog_marketplace_data WHERE daysago = 0 AND sprite_id = '" + (int)Row["sprite_id"] + "' LIMIT 1;");
            if (MarketData != null)
            {
                MySqlClient.ExecuteQueryTable("UPDATE catalog_marketplace_data SET daily_sold = daily_sold + 1, sold = sold + 1, avgprice = (avgprice + " + (int)Row["total_price"] + ") WHERE id = " + (int)MarketData["id"] + " LIMIT 1;");
            }
            else
            {
                MySqlClient.ExecuteQueryTable("INSERT INTO catalog_marketplace_data (sprite_id, sold, daily_sold, avgprice, daysago) VALUES ('" + UserItem.SpriteId + "', 1, 1, " + (int)Row["total_price"] + ", 0)");
            }

            if (mMarketAverages.ContainsKey(UserItem.SpriteId) && mMarketCounts.ContainsKey(UserItem.SpriteId))
            {
                int num3 = mMarketCounts[UserItem.SpriteId];
                int num4 = mMarketAverages[UserItem.SpriteId];
                num4 += (int)Row["total_price"];
                mMarketAverages.Remove(UserItem.SpriteId);
                mMarketAverages.Add(UserItem.SpriteId, num4);
                mMarketCounts.Remove(UserItem.SpriteId);
                mMarketCounts.Add(UserItem.SpriteId, num3 + 1);
            }
            else
            {
                if (!mMarketAverages.ContainsKey(UserItem.SpriteId))
                {
                    mMarketAverages.Add(UserItem.SpriteId, (int)Row["total_price"]);
                }
                if (!mMarketCounts.ContainsKey(UserItem.SpriteId))
                {
                    mMarketCounts.Add(UserItem.SpriteId, 1);
                }
            }

            Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, -(int)Row["total_price"]);
            Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));

            Dictionary<int, List<uint>> NewItems = new Dictionary<int, List<uint>>();
            List<Item> GeneratedGenericItems = new List<Item>();
            GeneratedGenericItems.Add(ItemFactory.CreateItem(MySqlClient, UserItem.Id,
                    Session.CharacterInfo.Id, (string)Row["extra_data"], (string)Row["extra_data"], 0));

            foreach (Item GeneratedItem in GeneratedGenericItems)
            {
                Session.InventoryCache.Add(GeneratedItem);

                int TabId = GeneratedItem.Definition.Type == ItemType.FloorItem ? 1 : 2;

                if (!NewItems.ContainsKey(TabId))
                {
                    NewItems.Add(TabId, new List<uint>());
                }

                NewItems[TabId].Add(GeneratedItem.Id);
            }

            foreach (KeyValuePair<int, List<uint>> NewItemData in NewItems)
            {
                foreach (uint NewItem in NewItemData.Value)
                {
                    Session.NewItemsCache.MarkNewItem(MySqlClient, NewItemData.Key, NewItem);
                }
            }

            if (NewItems.Count > 0)
            {
                Session.SendData(InventoryNewItemsComposer.Compose(new Dictionary<int, List<uint>>(NewItems)));
            }
            Session.SendData(InventoryRefreshComposer.Compose());
            SerializeOffers(Session, -1, -1, "", 1);
        }
        public void SellItem(Session Session, uint ItemID, int SellingPrice)
        {
            bool SellOK = false;

            Item UserItem = Session.InventoryCache.GetItem(ItemID);
            if (UserItem == null || SellingPrice > ServerSettings.MarketplaceMaxPrice || !CanSellItem(UserItem))
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

                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    MySqlClient.SetParameter("public_name", UserItem.Definition.Name);
                    MySqlClient.SetParameter("extra_data", UserItem.Flags);
                    string RawId = MySqlClient.ExecuteScalar("INSERT INTO catalog_marketplace_offers (item_id,user_id,asking_price,total_price,public_name,sprite_id,item_type,timestamp,extra_data) VALUES ('" + UserItem.DefinitionId + "','" + Session.CharacterId + "','" + SellingPrice + "','" + TotalPrice + "',@public_name,'" + UserItem.Definition.SpriteId + "','" + ItemType + "','" + UnixTimestamp.GetCurrent() + "',@extra_data); SELECT LAST_INSERT_ID();").ToString();

                    uint.TryParse(RawId, out uint Id);
                    if (Id > 0)
                    {
                        SellOK = true;
                        UserItem.RemovePermanently(MySqlClient);
                        Session.CharacterInfo.UpdateMarketplaceTokens(MySqlClient, -1);
                        Session.InventoryCache.RemoveItem(ItemID);
                        Session.SendData(InventoryRefreshComposer.Compose());

                        SerializeOffers(Session, -1, -1, "", 1);
                        Session.SendData(CatalogMarketplaceSerializeOwnOffersComposer.Compose(Session.CharacterId));
                    }
                }
            }

            Session.SendData(CatalogMarketplaceSellItemComposer.Compose(SellOK));
        }

        public void CancelOffer(Session Session, SqlDatabaseClient MySqlClient, uint OfferId)
        {
            DataRow Row = MySqlClient.ExecuteQueryRow("SELECT * FROM catalog_marketplace_offers WHERE offer_id = '" + OfferId + "' LIMIT 1");
            if (Row == null || (uint)Row["user_id"] != Session.CharacterId || (string)Row["state"] != "1")
            {
                return;
            }
            ItemDefinition UserItem = ItemDefinitionManager.GetDefinition((uint)Row["item_id"]);
            if (UserItem == null)
            {
                return;
            }

            Dictionary<int, List<uint>> NewItems = new Dictionary<int, List<uint>>();
            List<Item> GeneratedGenericItems = new List<Item>();
            GeneratedGenericItems.Add(ItemFactory.CreateItem(MySqlClient, UserItem.Id,
                    Session.CharacterInfo.Id, (string)Row["extra_data"], (string)Row["extra_data"], 0));

            foreach (Item GeneratedItem in GeneratedGenericItems)
            {
                Session.InventoryCache.Add(GeneratedItem);

                int TabId = GeneratedItem.Definition.Type == ItemType.FloorItem ? 1 : 2;

                if (!NewItems.ContainsKey(TabId))
                {
                    NewItems.Add(TabId, new List<uint>());
                }

                NewItems[TabId].Add(GeneratedItem.Id);
            }

            foreach (KeyValuePair<int, List<uint>> NewItemData in NewItems)
            {
                foreach (uint NewItem in NewItemData.Value)
                {
                    Session.NewItemsCache.MarkNewItem(MySqlClient, NewItemData.Key, NewItem);
                }
            }

            if (NewItems.Count > 0)
            {
                Session.SendData(InventoryNewItemsComposer.Compose(new Dictionary<int, List<uint>>(NewItems)));
            }
            Session.SendData(InventoryRefreshComposer.Compose());

            MySqlClient.ExecuteQueryTable("DELETE FROM catalog_marketplace_offers WHERE offer_id = '" + OfferId + "' LIMIT 1");
            Session.SendData(CatalogMarketplaceTakeBackComposer.Compose((uint)Row["offer_id"], true));
        }

        public void RedeemCredits(Session Session)
        {
            DataTable Results = null;

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                MySqlClient.SetParameter("id", Session.CharacterId);
                Results = MySqlClient.ExecuteQueryTable("SELECT asking_price FROM catalog_marketplace_offers WHERE user_id = @id AND state = '2'");
                if (Results != null)
                {
                    int Profit = 0;

                    foreach (DataRow Row in Results.Rows)
                    {
                        Profit += (int)Row["asking_price"];
                    }

                    if (Profit >= 1)
                    {
                        Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, Profit);
                        Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));

                        MySqlClient.SetParameter("id", Session.CharacterId);
                        MySqlClient.ExecuteQueryTable("DELETE FROM catalog_marketplace_offers WHERE user_id = @id AND state = '2'");
                    }
                }
            }
        }
        public void SerializeOffers(Session Session, int MinCost, int MaxCost, string SearchQuery, int FilterMode)
        {
            using (SqlDatabaseClient dbClient = SqlDatabaseManager.GetClient())
            {
                DataTable Data = dbClient.ExecuteQueryTable("SELECT * FROM catalog_marketplace_offers WHERE state = '1' AND timestamp >= " + FormatTimestampString());
                mMarketplaceOffers.Clear();

                if (Data != null)
                {
                    foreach (DataRow Row in Data.Rows)
                    {
                        MarketplaceOffers Offer = new MarketplaceOffers(Convert.ToInt32(Row["offer_id"]), Convert.ToUInt32(Row["item_id"]),
                                Convert.ToUInt32(Row["sprite_id"]), Convert.ToInt32(Row["total_price"]), int.Parse(Row["item_type"].ToString()),
                                Convert.ToInt32(Row["limited_number"]), Convert.ToInt32(Row["limited_stack"]));

                        mMarketplaceOffers.Add((uint)Row["offer_id"], Offer);
                    }
                }
            }

            IEnumerable<MarketplaceOffers> rawList = null;
            if (MinCost >= 0 && MaxCost >= 0)
            {
                rawList = mMarketplaceOffers.Values.Where(O => O.TotalPrice > MinCost && O.TotalPrice < MaxCost && O.GetItemDef().Name.Contains(SearchQuery));
            }
            else if (MinCost >= 0)
            {
                rawList = mMarketplaceOffers.Values.Where(O => O.TotalPrice > MinCost && O.GetItemDef().Name.Contains(SearchQuery));
            }
            else if (MaxCost >= 0)
            {
                rawList = mMarketplaceOffers.Values.Where(O => O.TotalPrice < MaxCost && O.GetItemDef().Name.Contains(SearchQuery));
            }
            else
            {
                rawList = mMarketplaceOffers.Values.Where(O => O.GetItemDef().Name.Contains(SearchQuery));
            }

            List<IGrouping<uint, MarketplaceOffers>> offers = null;
            switch (FilterMode)
            {
                case 1: //most expensive first
                    offers = rawList.OrderByDescending(o => o.TotalPrice).GroupBy(o => o.Sprite).ToList();
                    break;

                case 2: //most cheap first
                    offers = rawList.OrderBy(o => o.TotalPrice).GroupBy(o => o.Sprite).ToList();
                    break;

                case 3: //most traded today
                    offers = rawList.OrderByDescending(o => o.GetSoldsTodayBySpriteId()).GroupBy(o => o.Sprite).ToList();
                    break;

                case 4: //lest traded today
                    offers = rawList.OrderBy(o => o.GetSoldsTodayBySpriteId()).GroupBy(o => o.Sprite).ToList();
                    break;

                case 5: //most offers avaible
                    offers = rawList.GroupBy(o => o.ItemID).OrderByDescending(g => g.Count()).ToList();
                    break;

                case 6: //leasts offers avaible
                    offers = rawList.GroupBy(o => o.ItemID).OrderBy(g => g.Count()).ToList();
                    break;

                default:
                    offers = new List<IGrouping<uint, MarketplaceOffers>>();
                    break;
            }

            Session.SendData(CatalogMarketplaceSerializeOffersComposer.Compose(offers));
        }
    }
}
