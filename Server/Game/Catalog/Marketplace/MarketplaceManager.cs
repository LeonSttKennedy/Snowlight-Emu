using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Items;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Misc;
using System.Web.Caching;

namespace Snowlight.Game.Catalog
{
    public static class MarketplaceManager
    {
        public static int CalculateComissionPrice(float SellingPrice)
        {
            double Double = SellingPrice / 100.0;
            return Convert.ToInt32(Math.Ceiling(Double * ServerSettings.MarketplaceTax));
        }

        public static double FormatTimestamp()
        {
            return UnixTimestamp.GetCurrent() - (ServerSettings.MarketplaceOfferTotalHours * 60 * 60);
        }

        public static int AveragePriceForItem(int ItemType, uint SpriteId, string ExtraData = "")
        {
            int AveragePrice = 0;
            int ItemSoldCount = 0;

            switch (ItemType)
            {
                default:
                case 1:
                    {
                        if (CatalogManager.MarketplaceAvarages.ContainsKey(SpriteId))
                        {
                            foreach (MarketplaceAvarage Item in CatalogManager.MarketplaceAvarages[SpriteId])
                            {
                                TimeSpan SoldTime = DateTime.Now - Item.SoldTimeStamp;
                                DateTime SoldTimeStamp = DateTime.Now.AddDays(-ServerSettings.MarketplaceAvarageDays);

                                ItemSoldCount = CatalogManager.MarketplaceAvarages[SpriteId].Count(o => o.SoldTimeStamp >= SoldTimeStamp);
                                if (SoldTime.TotalDays <= ServerSettings.MarketplaceAvarageDays)
                                {
                                    AveragePrice += Item.TotalPrice;
                                }
                            }
                        }

                        return AveragePrice > 0 && ItemSoldCount > 0 ? AveragePrice / ItemSoldCount : 0;
                    }

                case 2:
                    {
                        if (CatalogManager.MarketplaceAvarages.ContainsKey(SpriteId))
                        {
                            foreach (MarketplaceAvarage Item in CatalogManager.MarketplaceAvarages[SpriteId])
                            {
                                TimeSpan SoldTime = DateTime.Now - Item.SoldTimeStamp;
                                DateTime SoldTimeStamp = DateTime.Now.AddDays(-ServerSettings.MarketplaceAvarageDays);

                                ItemSoldCount = CatalogManager.MarketplaceAvarages[SpriteId].Where(I => I.ExtraData == ExtraData).Count(o => o.SoldTimeStamp >= SoldTimeStamp);
                                if (Item.ExtraData == ExtraData && SoldTime.TotalDays <= ServerSettings.MarketplaceAvarageDays)
                                {
                                    AveragePrice += Item.TotalPrice;
                                }
                            }
                        }

                        return AveragePrice > 0 && ItemSoldCount > 0 ? AveragePrice / ItemSoldCount : 0;
                    }
            }
        }

        public static int CountForSprite(uint SpriteID)
        {
            return CatalogManager.MarketplaceOffers.Values.Where(O => O.State == 1 && O.Sprite == SpriteID).Count();
        }

        public static void Purchase(Session Session, SqlDatabaseClient MySqlClient, uint OfferId)
        {
            MarketplaceOffers Offer = TryGetOffer(OfferId);
            MarketplaceFiltersCache UserCache = Session.MarketplaceFiltersCache;

            if (Offer == null)
            {
                SerializeOffers(Session, UserCache.MinPrice, UserCache.MaxPrice, UserCache.SearchQuery, UserCache.FilterMode);

                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("catalog_marketplace_offer_not_found")));
                return;
            }

            if(Offer.State != 1 || Offer.Timestamp <= FormatTimestamp())
            {
                if (CatalogManager.MarketplaceOffers.Values.Where(O => O.State == 1 && O.DefinitionId == Offer.DefinitionId).Count() > 0)
                {
                    Offer = CatalogManager.MarketplaceOffers.Values.Where(O => O.State == 1 && O.DefinitionId == Offer.DefinitionId).OrderBy(O => O.TotalPrice).FirstOrDefault();
                    Session.SendData(CatalogMarketplaceBuyOfferResultComposer.Compose(MarketplaceError.UpdateItem, Offer.Id, Offer.TotalPrice, OfferId));
                }
                else
                {
                    SerializeOffers(Session, UserCache.MinPrice, UserCache.MaxPrice, UserCache.SearchQuery, UserCache.FilterMode);

                    Session.SendData(CatalogMarketplaceBuyOfferResultComposer.Compose(MarketplaceError.AllSoldOut));
                }

                return;
            }

            if (ServerSettings.MarketplaceBoostProtectionEnabled && Offer.UserId == Session.CharacterInfo.Id)
            {
                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("catalog_marketplace_boosting_error")));
                return;
            }

            if (Session.CharacterInfo.CreditsBalance < Offer.TotalPrice)
            {
                Session.SendData(CatalogMarketplaceBuyOfferResultComposer.Compose(MarketplaceError.NoCredits));
                return;
            }

            ItemDefinition UserItem = ItemDefinitionManager.GetDefinition(Offer.DefinitionId);
            if (UserItem == null)
            {
                return;
            }

            MySqlClient.SetParameter("offerid", OfferId);
            MySqlClient.ExecuteNonQuery("UPDATE catalog_marketplace_offers SET state = '2' WHERE offer_id = @offerid LIMIT 1");

            #region MarketAverages
            MySqlClient.SetParameter("sprite_id", Offer.Sprite);
            MySqlClient.SetParameter("item_type", Offer.ItemType);
            MySqlClient.SetParameter("extra_data", Offer.ExtraData);
            MySqlClient.SetParameter("sold_price", Offer.TotalPrice);
            MySqlClient.SetParameter("sold_timestamp", UnixTimestamp.GetCurrent());
            MySqlClient.ExecuteQueryTable("INSERT INTO catalog_marketplace_data (sprite_id, item_type, extra_data, sold_price, sold_timestamp) VALUES (@sprite_id, @item_type, @extra_data, @sold_price, @sold_timestamp)");
            CatalogManager.ReloadMarketplaceData(MySqlClient);
            #endregion

            Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, -Offer.TotalPrice);
            Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));

            Dictionary<int, List<uint>> NewItems = new Dictionary<int, List<uint>>();
            List<Item> GeneratedGenericItems = new List<Item>();
            GeneratedGenericItems.Add(ItemFactory.CreateItem(MySqlClient, UserItem.Id,
                    Session.CharacterInfo.Id, Offer.ExtraData, Offer.ExtraData, 0));

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
            Session.SendData(CatalogMarketplaceBuyOfferResultComposer.Compose(MarketplaceError.Sucess));
        }

        public static void SellItem(Session Session, uint ItemID, int ItemType, int SellingPrice)
        {
            MarketplaceSellOk ErrorCode = MarketplaceSellOk.TechinicalError;

            if (ServerSettings.MarketplaceEnabled)
            {
                Item UserItem = Session.InventoryCache.GetItem(ItemID);
                if (UserItem == null || SellingPrice > ServerSettings.MarketplaceMaxPrice || !UserItem.CanSell)
                {
                    ErrorCode = MarketplaceSellOk.TechinicalError;
                }
                else
                {
                    int Comission = CalculateComissionPrice(SellingPrice);
                    int TotalPrice = Comission + SellingPrice;

                    using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                    {
                        MySqlClient.SetParameter("def_id", UserItem.DefinitionId);
                        MySqlClient.SetParameter("user_id", Session.CharacterInfo.Id);
                        MySqlClient.SetParameter("price", SellingPrice);
                        MySqlClient.SetParameter("total_price", TotalPrice);
                        MySqlClient.SetParameter("public_name", UserItem.Definition.Name);
                        MySqlClient.SetParameter("sprite_id", UserItem.Definition.SpriteId);
                        MySqlClient.SetParameter("item_type", ItemType);
                        MySqlClient.SetParameter("timestamp", UnixTimestamp.GetCurrent());
                        MySqlClient.SetParameter("extra_data", UserItem.Flags);
                        string RawId = MySqlClient.ExecuteScalar("INSERT INTO catalog_marketplace_offers (item_id, user_id, asking_price, total_price, public_name, sprite_id, item_type, timestamp, extra_data) VALUES (@def_id, @user_id, @price, @total_price, @public_name, @sprite_id, @item_type, @timestamp, @extra_data); SELECT LAST_INSERT_ID();").ToString();

                        uint.TryParse(RawId, out uint Id);
                        if (Id > 0)
                        {
                            ErrorCode = MarketplaceSellOk.SellOK;
                            UserItem.RemovePermanently(MySqlClient);
                            Session.CharacterInfo.UpdateMarketplaceTokens(MySqlClient, -1);
                            Session.InventoryCache.RemoveItem(ItemID);
                            Session.SendData(InventoryRefreshComposer.Compose());

                            MarketplaceFiltersCache UserCache = Session.MarketplaceFiltersCache;
                            SerializeOffers(Session, UserCache.MinPrice, UserCache.MaxPrice, UserCache.SearchQuery, UserCache.FilterMode);

                            Session.SendData(CatalogMarketplaceSerializeOwnOffersComposer.Compose(Session.CharacterId));
                        }
                        else
                        {
                            ErrorCode = MarketplaceSellOk.TechinicalError;
                        }
                    }
                }
            }
            else
            {
                ErrorCode = MarketplaceSellOk.MarketplaceDisabled;
            }

            Session.SendData(CatalogMarketplaceSellItemComposer.Compose(ErrorCode));
        }

        public static void CancelOffer(Session Session, SqlDatabaseClient MySqlClient, uint OfferId)
        {
            MarketplaceOffers Offer = TryGetOffer(OfferId);
            if (Offer == null || Offer.UserId != Session.CharacterId || Offer.State == 2)
            {
                Session.SendData(CatalogMarketplaceTakeBackComposer.Compose(OfferId, false));
                return;
            }

            ItemDefinition UserItem = ItemDefinitionManager.GetDefinition(Offer.DefinitionId);
            if (UserItem == null)
            {
                Session.SendData(CatalogMarketplaceTakeBackComposer.Compose(OfferId, false));
                return;
            }

            Dictionary<int, List<uint>> NewItems = new Dictionary<int, List<uint>>();
            List<Item> GeneratedGenericItems = new List<Item>();
            GeneratedGenericItems.Add(ItemFactory.CreateItem(MySqlClient, UserItem.Id,
                    Session.CharacterInfo.Id, Offer.ExtraData, Offer.ExtraData, 0));

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

            MySqlClient.SetParameter("offerid", OfferId);
            MySqlClient.ExecuteQueryTable("DELETE FROM catalog_marketplace_offers WHERE offer_id = @offerid LIMIT 1");
            Session.SendData(CatalogMarketplaceTakeBackComposer.Compose(OfferId, true));

            MarketplaceFiltersCache UserCache = Session.MarketplaceFiltersCache;
            SerializeOffers(Session, UserCache.MinPrice, UserCache.MaxPrice, UserCache.SearchQuery, UserCache.FilterMode);
        }

        public static void RedeemCredits(Session Session)
        {
            IEnumerable<MarketplaceOffers> UserOfferList = CatalogManager.MarketplaceOffers.Values.Where(O => O.UserId == Session.CharacterId);
            int Profit = UserOfferList.Where(O => O.State == 2).Sum(O => O.AskingPrice);

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                if (Profit >= 1)
                {
                    Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, Profit);
                    Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));

                    MySqlClient.SetParameter("id", Session.CharacterId);
                    MySqlClient.ExecuteQueryTable("DELETE FROM catalog_marketplace_offers WHERE user_id = @id AND state = '2'");
                }
            }
        }

        public static void SerializeOffers(Session Session, int MinCost, int MaxCost, string SearchQuery, int FilterMode)
        {
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                CatalogManager.ReloadMarketplaceData(MySqlClient);
            }

            IEnumerable<MarketplaceOffers> rawList = null;
            if (MinCost >= 0 && MaxCost >= 0)
            {
                rawList = CatalogManager.MarketplaceOffers.Values.Where(O => O.State == 1 && O.Timestamp >= FormatTimestamp() && O.TotalPrice > MinCost && O.TotalPrice < MaxCost && O.Definition.PublicName.Contains(SearchQuery));
            }
            else if (MinCost >= 0)
            {
                rawList = CatalogManager.MarketplaceOffers.Values.Where(O => O.State == 1 && O.Timestamp >= FormatTimestamp() && O.TotalPrice > MinCost && O.Definition.PublicName.Contains(SearchQuery));
            }
            else if (MaxCost >= 0)
            {
                rawList = CatalogManager.MarketplaceOffers.Values.Where(O => O.State == 1 && O.Timestamp >= FormatTimestamp() && O.TotalPrice < MaxCost && O.Definition.PublicName.Contains(SearchQuery));
            }
            else
            {
                rawList = CatalogManager.MarketplaceOffers.Values.Where(O => O.State == 1 && O.Timestamp >= FormatTimestamp() && O.Definition.PublicName.Contains(SearchQuery));
            }

            List<IGrouping<uint, MarketplaceOffers>> offers = null;
            switch (FilterMode)
            {
                case 1: //most expensive first
                    offers = rawList.OrderBy(o => o.TotalPrice).GroupBy(o => o.GroupBy()).Reverse().ToList();
                    break;

                case 2: //most cheap first
                    offers = rawList.OrderBy(o => o.TotalPrice).GroupBy(o => o.GroupBy()).ToList();
                    break;

                case 3: //most traded today
                    offers = rawList.OrderBy(o => o.GetTotalSoldsToday()).GroupBy(o => o.GroupBy()).Reverse().ToList();
                    break;

                case 4: //lest traded today
                    offers = rawList.OrderBy(o => o.GetTotalSoldsToday()).GroupBy(o => o.GroupBy()).ToList();
                    break;

                case 5: //most offers avaible
                    offers = rawList.GroupBy(o => o.GroupBy()).OrderBy(g => g.Count()).Reverse().ToList();
                    break;

                case 6: //leasts offers avaible
                    offers = rawList.GroupBy(o => o.GroupBy()).OrderBy(g => g.Count()).ToList();
                    break;

                default:
                    offers = new List<IGrouping<uint, MarketplaceOffers>>();
                    break;
            }

            Session.SendData(CatalogMarketplaceSerializeOffersComposer.Compose(offers));
        }

        public static MarketplaceOffers TryGetOffer(uint OfferId)
        {
            CatalogManager.MarketplaceOffers.TryGetValue(OfferId, out MarketplaceOffers Offer);
            return Offer;
        }
    }
}
