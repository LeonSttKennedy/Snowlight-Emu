using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Snowlight.Communication;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Sessions;
using Snowlight.Storage;

using Snowlight.Game.Misc;
using Snowlight.Game.Rooms;
using Snowlight.Game.Items;
using Snowlight.Communication.ResponseCache;
using Snowlight.Game.Rights;
using Snowlight.Game.Pets;
using Snowlight.Communication.Incoming;
using Snowlight.Config;
using Snowlight.Util;

namespace Snowlight.Game.Catalog
{
    public static class CatalogManager
    {
        private static Marketplace Marketplace;
        private static Dictionary<int, CatalogPage> mPages;

        private static Dictionary<int, List<CatalogItem>> mCatalogItems;
        private static Dictionary<uint, CatalogItem> mCatalogItemsIdIndex;
        private static Dictionary<string, CatalogItem> mCatalogItemsNameIndex;

        private static Dictionary<uint, CatalogClubOffer> mClubOffers;

        private static ResponseCacheController mCacheController;

        public static bool CacheEnabled
        {
            get
            {
                return (mCacheController != null);
            }
        }

        public static ResponseCacheController CacheController
        {
            get
            {
                return mCacheController;
            }
        }

        public static void Initialize(SqlDatabaseClient MySqlClient)
        {
            Marketplace = new Marketplace();
            mPages = new Dictionary<int, CatalogPage>();
            mCatalogItems = new Dictionary<int, List<CatalogItem>>();
            mCatalogItemsIdIndex = new Dictionary<uint, CatalogItem>();
            mCatalogItemsNameIndex = new Dictionary<string, CatalogItem>();
            mClubOffers = new Dictionary<uint, CatalogClubOffer>();

            if ((bool)ConfigManager.GetValue("cache.catalog.enabled"))
            {
                mCacheController = new ResponseCacheController((int)ConfigManager.GetValue("cache.catalog.lifetime"));
            }

            RefreshCatalogData(MySqlClient, false);

            DataRouter.RegisterHandler(OpcodesIn.CATALOG_GET_INDEX, new ProcessRequestCallback(GetCatalogIndex));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_GET_PAGE, new ProcessRequestCallback(GetCatalogPage));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_GET_CLUB_OFFERS, new ProcessRequestCallback(GetClubOffers));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_GIFT_ITEM, new ProcessRequestCallback(PurchaseGift));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_GIFT_CONFIG, new ProcessRequestCallback(GetPresentsData));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_REDEEM_VOUCHER, new ProcessRequestCallback(RedeemVoucher));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_GET_PET_DATA, new ProcessRequestCallback(GetPetData));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_VERIFY_PET_NAME, new ProcessRequestCallback(CheckPetName));

            // Marketplace Handlers
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_MARKETPLACE_CONFIG, new ProcessRequestCallback(MarketplaceConfig));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_MARKETPLACE_REDEEM_CREDITS, new ProcessRequestCallback(MarketplaceClaimCredits));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_MARKETPLACE_PURCHASE, new ProcessRequestCallback(MarketplacePurchase));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_MARKETPLACE_TAKE_BACK, new ProcessRequestCallback(MarketplaceTakeBack));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_MARKETPLACE_POST_OFFER, new ProcessRequestCallback(MarketPlacePostItem));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_MARKETPLACE_GET_OFFERS, new ProcessRequestCallback(MarketplaceGetOffers));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_MARKETPLACE_OWN_OFFERS, new ProcessRequestCallback(MarketplaceGetOwnOffers));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_MARKETPLACE_CAN_SELL, new ProcessRequestCallback(MarketplaceCanSell));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_MARKETPLACE_ITEM_STATS, new ProcessRequestCallback(MarketplaceItemStats));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_MARKETPLACE_BUY_TICKETS, new ProcessRequestCallback(MarketplaceBuyTickets));
        }

        private static ServerMessage TryGetResponseFromCache(uint GroupId, ClientMessage Request)
        {
            return (CacheEnabled ? mCacheController.TryGetResponse(GroupId, Request) : null);
        }

        private static void AddToCacheIfNeeded(uint GroupId, ClientMessage Request, ServerMessage Response)
        {
            if (!CacheEnabled)
            {
                return;
            }

            mCacheController.AddIfNeeded(GroupId, Request, Response);
        }

         public static void ClearCacheGroup(uint GroupId)
        {
            if (!CacheEnabled)
            {
                return;
            }

            mCacheController.ClearCacheGroup(GroupId);
        }

        public static void RefreshCatalogData(SqlDatabaseClient MySqlClient, bool NotifyUsers = true)
        {
            int CountLoaded = 0;

            lock (mPages)
            {
                mCatalogItems.Clear();
                mCatalogItemsIdIndex.Clear();
                mCatalogItemsNameIndex.Clear();
                mPages.Clear();
                mClubOffers.Clear();

                mPages.Add(-1, new CatalogPage(-1, 0, string.Empty, 0, 0, string.Empty, true, true, false, string.Empty, null, null, new List<CatalogItem>())); // root category

                MySqlClient.SetParameter("enabled", "1");
                DataTable ItemTable = MySqlClient.ExecuteQueryTable("SELECT * FROM catalog_items WHERE enabled = @enabled ORDER BY name ASC");

                foreach (DataRow Row in ItemTable.Rows)
                {
                    int PageId = (int)Row["page_id"];

                    if (!mCatalogItems.ContainsKey(PageId))
                    {
                        mCatalogItems[PageId] = new List<CatalogItem>();
                    }

                    CatalogItem Item = new CatalogItem((uint)Row["id"], (uint)Row["base_id"], (string)Row["name"],
                        (int)Row["cost_credits"], (int)Row["cost_pixels"], (int)Row["amount"], (string)Row["preset_flags"],
                        (int)Row["club_restriction"]);

                    if (Item.Definition == null)
                    {
                        Output.WriteLine("Warning: Catalog item " + (uint)Row["id"] + " has an invalid base_id reference.", OutputLevel.Warning);
                        continue;
                    }

                    mCatalogItems[PageId].Add(Item);
                    mCatalogItemsIdIndex[Item.Id] = Item;
                    mCatalogItemsNameIndex[Item.DisplayName] = Item;
                }

                MySqlClient.SetParameter("enabled", "1");
                DataTable Table = MySqlClient.ExecuteQueryTable("SELECT * FROM catalog WHERE enabled = @enabled ORDER BY order_num ASC");

                foreach (DataRow Row in Table.Rows)
                {
                    List<string> PageStrings1 = new List<string>();
                    List<string> PageStrings2 = new List<string>();

                    foreach (string String in Row["page_strings_1"].ToString().Split('|')) PageStrings1.Add(String);
                    foreach (string String in Row["page_strings_2"].ToString().Split('|')) PageStrings2.Add(String);

                    int Id = (int)Row["id"];

                    mPages.Add(Id, new CatalogPage((int)Row["id"], (int)Row["parent_id"], (string)Row["title"],
                        (int)Row["icon"], (int)Row["color"], (string)Row["required_right"], (Row["visible"].ToString() == "1"),
                        (Row["dummy_page"].ToString() == "1"), (Row["coming_soon"].ToString() == "1"), (string)Row["template"], PageStrings1, PageStrings2,
                        mCatalogItems.ContainsKey(Id) ? mCatalogItems[Id] : new List<CatalogItem>()));

                    CountLoaded++;
                }

                DataTable ClubTable = MySqlClient.ExecuteQueryTable("SELECT * FROM catalog_subscriptions");

                foreach (DataRow Row in ClubTable.Rows)
                {
                    CatalogClubOfferType OfferType = CatalogClubOfferType.Basic;

                    switch ((string)Row["type"])
                    {
                        case "vip":

                            OfferType = CatalogClubOfferType.Vip;
                            break;

                        case "upgrade":

                            OfferType = CatalogClubOfferType.VipUpgrade;
                            break;
                    }

                    mClubOffers.Add((uint)Row["id"], new CatalogClubOffer((uint)Row["id"], (string)Row["name"],
                        (int)Row["cost_credits"], (int)Row["length_days"], OfferType));
                }
            }

            Output.WriteLine("Loaded " + CountLoaded + " catalog page(s).", OutputLevel.DebugInformation);

            if (NotifyUsers)
            {
                SessionManager.BroadcastPacket(CatalogUpdatedNotificationComposer.Compose());
            }
        }

        public static CatalogClubOffer GetClubOffer(uint ItemId)
        {
            return mClubOffers.ContainsKey(ItemId) ? mClubOffers[ItemId] : null;
        }

        public static CatalogPage GetCatalogPage(int PageId)
        {
            lock (mPages)
            {
                if (mPages.ContainsKey(PageId))
                {
                    return mPages[PageId];
                }
            }

            return null;
        }

        public static CatalogItem GetCatalogItemByAbsoluteId(uint ItemId)
        {
            lock (mCatalogItemsIdIndex)
            {
                if (mCatalogItemsIdIndex.ContainsKey(ItemId))
                {
                    return mCatalogItemsIdIndex[ItemId];
                }
            }

            return null;
        }

        public static CatalogItem GetCatalogItemByPage(int PageId, uint ItemId)
        {
            lock (mCatalogItems)
            {
                if (mCatalogItems.ContainsKey(PageId))
                {
                    foreach (CatalogItem Item in mCatalogItems[PageId])
                    {
                        if (Item.Id == ItemId)
                        {
                            return Item;
                        }
                    }
                }
            }

            return null;
        }

        public static void MarketplaceBuyTickets(Session Session, ClientMessage CMessage)
        {
            bool CreditsError = false;

            if (Session.CharacterInfo.CreditsBalance <= 0)
            {
                CreditsError = true;
            }

            if (CreditsError)
            {
                ServerMessage Message = new ServerMessage(68);
                Message.AppendBoolean(true); // Credits error
                Message.AppendBoolean(false); // Pixel error
                Session.SendData(Message);
                return;
            }
            else
            {
                using (SqlDatabaseClient dbClient = SqlDatabaseManager.GetClient())
                {
                    dbClient.ExecuteQueryTable("UPDATE characters SET marketplace_tickets = marketplace_tickets + 5 WHERE id = '" + Session.CharacterId + "'");
                    Session.CharacterInfo.UpdateCreditsBalance(dbClient, -1);
                    Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                }

                if (Session.CharacterInfo.MarketplaceTokensTotal < 0)
                {
                    Session.CharacterInfo.MarketplaceTokensTotal = 0;
                }

                Session.CharacterInfo.MarketplaceTokensTotal += 5;

                MarketplaceCanSell(Session, CMessage);
            }
        }

        public static void MarketplaceItemStats(Session Session, ClientMessage CMessage)
        {
            int int_ = CMessage.PopWiredInt32();
            int Sprite = CMessage.PopWiredInt32();
            ServerMessage Message = new ServerMessage(617);
            Message.AppendInt32(1);
            Message.AppendInt32(CatalogManager.Marketplace.OfferCountForSprite(Sprite));
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
            Message.AppendInt32(int_);
            Message.AppendInt32(Sprite);
            Session.SendData(Message);
        }

        public static void MarketplaceConfig(Session Session, ClientMessage Event)
        {
            Session.SendData(CatalogMarketplaceConfigComposer.Compose());
        }

        private static void MarketplaceGetOffers(Session Session, ClientMessage Message)
        {
            int MinPrice = Message.PopWiredInt32();
            int MaxPrice = Message.PopWiredInt32();
            string SearchQuery = Message.PopString();
            int FilterMode = Message.PopWiredInt32();

            Session.SendData(CatalogManager.Marketplace.SerializeOffersNew(MinPrice, MaxPrice, SearchQuery, FilterMode));
        }

        private static void MarketplaceTakeBack(Session Session, ClientMessage Message)
        {
            uint ItemId = Message.PopWiredUInt32();
            DataRow Row = null;

            using (SqlDatabaseClient dbClient = SqlDatabaseManager.GetClient())
            {
                Row = dbClient.ExecuteQueryRow("SELECT * FROM catalog_marketplace_offers WHERE offer_id = '" + ItemId + "' LIMIT 1");
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
                GeneratedGenericItems.Add(ItemFactory.CreateItem(dbClient, UserItem.Id,
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
                        Session.NewItemsCache.MarkNewItem(dbClient, NewItemData.Key, NewItem);
                    }
                }

                if (NewItems.Count > 0)
                {
                    Session.SendData(InventoryNewItemsComposer.Compose(new Dictionary<int, List<uint>>(NewItems)));
                }
                Session.SendData(InventoryRefreshComposer.Compose());

                dbClient.ExecuteQueryTable("DELETE FROM catalog_marketplace_offers WHERE offer_id = '" + ItemId + "' LIMIT 1");
            }

            Session.SendData(CatalogMarketplaceTakeBackComposer.Compose((uint)Row["offer_id"]));
        }

        private static void MarketplaceCanSell(Session Session, ClientMessage Event)
        {
            if (Session.CharacterInfo.MarketplaceTokensTotal < 0)
            {
                Session.CharacterInfo.MarketplaceTokensTotal = 0;
            }

            Session.SendData(CatalogMarketplaceCanSellComposer.Compose(Session.CharacterInfo.MarketplaceTokensTotal));
        }
        private static void MarketplacePurchase(Session Session, ClientMessage Event)
        {
            uint ItemId = Event.PopWiredUInt32();
            DataRow Row = null;

            using (SqlDatabaseClient dbClient = SqlDatabaseManager.GetClient())
            {
                Row = dbClient.ExecuteQueryRow("SELECT * FROM catalog_marketplace_offers WHERE offer_id = '" + ItemId + "' LIMIT 1");

                if (Row == null || (string)Row["state"] != "1" || (double)Row["timestamp"] <= CatalogManager.Marketplace.FormatTimestamp())
                {
                    Session.SendData(NotificationMessageComposer.Compose("Sorry, this offer has expired."));
                    return;
                }

                if ((uint)Row["user_id"] == Session.CharacterInfo.Id)
                {
                    Session.SendData(NotificationMessageComposer.Compose("To prevent average boosting you cannot purchase your own marketplace offers."));
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

                dbClient.ExecuteQueryTable("UPDATE catalog_marketplace_offers SET state = '2' WHERE offer_id = '" + ItemId + "' LIMIT 1");

                DataRow MarketData = dbClient.ExecuteQueryRow("SELECT * FROM catalog_marketplace_data WHERE daysago = 0 AND sprite_id = '" + (int)Row["sprite_id"] + "' LIMIT 1;");
                if (MarketData != null)
                {
                    dbClient.ExecuteQueryTable("UPDATE catalog_marketplace_data SET sold = sold + 1, avgprice = (avgprice + " + (int)Row["total_price"] + ") WHERE id = " + (int)MarketData["id"] + " LIMIT 1;");
                }
                else
                {
                    dbClient.ExecuteQueryTable("INSERT INTO catalog_marketplace_data (sprite_id, sold, avgprice, daysago) VALUES ('" + UserItem.SpriteId + "', 1, " + (int)Row["total_price"] + ", 0)");
                }

                if (CatalogManager.Marketplace.MarketAverages.ContainsKey((int)UserItem.SpriteId) && CatalogManager.Marketplace.MarketCounts.ContainsKey((int)UserItem.SpriteId))
                {
                    int num3 = CatalogManager.Marketplace.MarketCounts[(int)UserItem.SpriteId];
                    int num4 = CatalogManager.Marketplace.MarketAverages[(int)UserItem.SpriteId];
                    num4 += (int)Row["total_price"];
                    CatalogManager.Marketplace.MarketAverages.Remove((int)UserItem.SpriteId);
                    CatalogManager.Marketplace.MarketAverages.Add((int)UserItem.SpriteId, num4);
                    CatalogManager.Marketplace.MarketCounts.Remove((int)UserItem.SpriteId);
                    CatalogManager.Marketplace.MarketCounts.Add((int)UserItem.SpriteId, num3 + 1);
                }
                else
                {
                    if (!CatalogManager.Marketplace.MarketAverages.ContainsKey((int)UserItem.SpriteId))
                    {
                        CatalogManager.Marketplace.MarketAverages.Add((int)UserItem.SpriteId, (int)Row["total_price"]);
                    }
                    if (!CatalogManager.Marketplace.MarketCounts.ContainsKey((int)UserItem.SpriteId))
                    {
                        CatalogManager.Marketplace.MarketCounts.Add((int)UserItem.SpriteId, 1);
                    }
                }

                Session.CharacterInfo.UpdateCreditsBalance(dbClient, -(int)Row["total_price"]);
                Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));

                Dictionary<int, List<uint>> NewItems = new Dictionary<int, List<uint>>();
                List<Item> GeneratedGenericItems = new List<Item>();
                GeneratedGenericItems.Add(ItemFactory.CreateItem(dbClient, UserItem.Id,
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
                        Session.NewItemsCache.MarkNewItem(dbClient, NewItemData.Key, NewItem);
                    }
                }

                if (NewItems.Count > 0)
                {
                    Session.SendData(InventoryNewItemsComposer.Compose(new Dictionary<int, List<uint>>(NewItems)));
                }
                Session.SendData(InventoryRefreshComposer.Compose());
                Session.SendData(CatalogManager.Marketplace.SerializeOffersNew(-1, -1, "", 1));
            }
        }

        private static void MarketPlacePostItem(Session Session, ClientMessage Message) 
        {
            if (Session.InventoryCache != null)
            {
                if (Session.InRoom)
                {
                    RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);
                    if (Instance.TradeManager.UserHasActiveTrade(Session.CharacterId))
                    {
                        return;
                    }
                }
                int sellingPrice = Message.PopWiredInt32();
                int junk = Message.PopWiredInt32();
                uint itemId = Message.PopWiredUInt32();

                Item UserItem = Session.InventoryCache.GetItem(itemId);
                if (UserItem != null && Marketplace.CanSellItem(UserItem))
                { 
                    CatalogManager.Marketplace.SellItem(Session, itemId, sellingPrice); 
                }
            }
        }

        private static void MarketplaceClaimCredits(Session Session, ClientMessage Message)
        {
            DataTable Results = null;

            using (SqlDatabaseClient dbClient = SqlDatabaseManager.GetClient())
            {
                Results = dbClient.ExecuteQueryTable("SELECT asking_price FROM catalog_marketplace_offers WHERE user_id = '" + Session.CharacterId + "' AND state = '2'");
                if (Results != null)
                {
                    int Profit = 0;

                    foreach (DataRow Row in Results.Rows)
                    {
                        Profit += (int)Row["asking_price"];
                        if (Profit >= 1)
                        {
                            Session.CharacterInfo.UpdateCreditsBalance(dbClient, +Profit);
                            Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                        }
                    }
                    dbClient.ExecuteQueryTable("DELETE FROM catalog_marketplace_offers WHERE user_id = '" + Session.CharacterId + "' AND state = '2'");
                }
            }
        }

        private static void MarketplaceGetOwnOffers(Session Session, ClientMessage Message)
        {
            Session.SendData(CatalogMarketplaceSerializeOwnOffersComposer.Compose(Session.CharacterId));
        }

        private static void GetCatalogIndex(Session Session, ClientMessage Message)
        {
            ServerMessage Response = TryGetResponseFromCache(Session.CharacterId, Message);

            if (Response != null)
            {
                Session.SendData(Response);
                return;
            }

            lock (mPages)
            {
                Response = CatalogIndexComposer.Compose(Session, mPages);
                //Response = CatalogIndexComposer.ComposeTextIndex();
            }

            AddToCacheIfNeeded(Session.CharacterId, Message, Response);
            Session.SendData(Response);
        }

        private static void GetCatalogPage(Session Session, ClientMessage Message)
        {
            int Id = Message.PopWiredInt32();

            if (!mPages.ContainsKey(Id))
            {
                return;
            }

            CatalogPage Page = mPages[Id];

            if (Page.DummyPage || Page.ComingSoon)
            {
                return;
            }

            ServerMessage Response = TryGetResponseFromCache(0, Message);

            if (Response != null)
            {
                Session.SendData(Response);
                return;
            }

            Response = CatalogPageComposer.Compose(Page);
            AddToCacheIfNeeded(0, Message, Response);
            Session.SendData(Response);
        }
        private static void PurchaseGift(Session Session, ClientMessage Message)
        {
            int PageId = Message.PopWiredInt32();
            uint ItemId = Message.PopWiredUInt32();
            string ExtraData = Message.PopString();
            string GiftUser = UserInputFilter.FilterString(Message.PopString());
            string GiftMessage = UserInputFilter.FilterString(Message.PopString());
            int SpriteId = Message.PopWiredInt32();
            int Ribbon = Message.PopWiredInt32();
            int Colour = Message.PopWiredInt32();

            CatalogPage Page = CatalogManager.GetCatalogPage(PageId);
            if (Page == null || Page.DummyPage || Page.ComingSoon || !Page.Visible || (Page.RequiredRight.Length > 0 &&
                !Session.HasRight(Page.RequiredRight)))
            {
                Session.SendData(CatalogGiftsWrappingErrorComposer.Composer());
                return;
            }

            CatalogItem Item = Page.GetItem(ItemId);
            if (Item == null || !Item.Definition.AllowGift || Item.Definition.TypeLetter.ToLower() == "e" || 
            (Item.ClubRestriction == 1 && !Session.HasRight("club_regular")) || 
            (Item.ClubRestriction == 2 && !Session.HasRight("club_vip")))
            {
                Session.SendData(CatalogGiftsWrappingErrorComposer.Composer());
                return;
            }

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                CatalogPurchaseHandler.HandlePurchaseGift(MySqlClient, Session, Item, ExtraData, GiftUser, GiftMessage, SpriteId, Ribbon, Colour);
            }
        }
        private static void GetPresentsData(Session Session, ClientMessage Message)
        {
            Session.SendData(CatalogGiftsConfigComposer.Compose());
        }
        private static void GetClubOffers(Session Session, ClientMessage Message)
        {
            ServerMessage Response = TryGetResponseFromCache(Session.CharacterId, Message);

            if (Response != null)
            {
                Session.SendData(Response);
                return;
            }

            ClubSubscription Subscription = Session.SubscriptionManager;
            List<CatalogClubOffer> CorrectedOffers = new List<CatalogClubOffer>();

            foreach (CatalogClubOffer Offer in mClubOffers.Values)
            {
                if ((Offer.IsUpgrade && Subscription.SubscriptionLevel != ClubSubscriptionLevel.BasicClub) ||
                    (Offer.Level == ClubSubscriptionLevel.BasicClub && Subscription.SubscriptionLevel ==
                    ClubSubscriptionLevel.VipClub) || (Offer.Level == ClubSubscriptionLevel.VipClub
                    && !Offer.IsUpgrade && Subscription.SubscriptionLevel == ClubSubscriptionLevel.BasicClub))
                {
                    continue;
                }

                CorrectedOffers.Add(Offer);
            }      

            Response = CatalogClubOffersComposer.Compose(CorrectedOffers,
                Session.SubscriptionManager.IsActive ?
                Session.SubscriptionManager.TimestampExpire : UnixTimestamp.GetCurrent());
            AddToCacheIfNeeded(Session.CharacterId, Message, Response);
            Session.SendData(Response);
        }

        private static void RedeemVoucher(Session Session, ClientMessage Message)
        {
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                Session.SendData(VoucherManager.TryRedeemVoucher(MySqlClient, Session, Message.PopString()) ? 
                    CatalogRedeemOkComposer.Compose() : CatalogRedeemErrorComposer.Compose(0));
            }
        }

        private static void GetPetData(Session Session, ClientMessage Message)
        {
            CatalogItem PetItem = null;
            string ItemName = Message.PopString();

            if (mCatalogItemsNameIndex.ContainsKey(ItemName))
            {
                PetItem = mCatalogItemsNameIndex[ItemName];
            }

            if (PetItem == null || PetItem.Definition.Behavior != ItemBehavior.Pet)
            {
                return;
            }

            int PetType = PetItem.Definition.BehaviorData;

            Session.SendData(CatalogPetDataComposer.Compose(PetItem, PetDataManager.GetRaceDataForType(PetType), PetType));
        }

        private static void CheckPetName(Session Session, ClientMessage Message)
        {
            Session.SendData(CatalogVerifyPetNameResultComposer.Compose((int)PetName.VerifyPetName(Message.PopString())));
        }
    }
}
