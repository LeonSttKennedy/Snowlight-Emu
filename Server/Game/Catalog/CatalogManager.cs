using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Config;
using Snowlight.Storage;
using Snowlight.Game.Misc;
using Snowlight.Game.Pets;
using Snowlight.Game.Rooms;
using Snowlight.Game.Items;
using Snowlight.Game.Rights;
using Snowlight.Communication;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Incoming;
using Snowlight.Communication.Outgoing;
using Snowlight.Communication.ResponseCache;

namespace Snowlight.Game.Catalog
{
    public static class CatalogManager
    {
        private static Dictionary<int, CatalogPage> mPages;

        private static Dictionary<int, List<CatalogItem>> mCatalogItems;
        private static Dictionary<uint, CatalogItem> mCatalogItemsIdIndex;
        private static Dictionary<string, CatalogItem> mCatalogItemsNameIndex;

        private static Dictionary<uint, MarketplaceOffers> mMarketplaceOffers;
        private static Dictionary<uint, List<MarketplaceAvarage>> mMarketplaceAvarages;

        private static Dictionary<uint, CatalogClubOffer> mClubOffers;

        private static Dictionary<int, ItemRotationSettings> mRotationPages;

        private static ResponseCacheController mCacheController;

        public static Dictionary<uint, CatalogClubOffer> ClubOffers
        {
            get
            {
                return mClubOffers;
            }
        }

        public static Dictionary<uint, MarketplaceOffers> MarketplaceOffers
        {
            get
            {
                return mMarketplaceOffers;
            }

            set
            {
                mMarketplaceOffers = value;
            }
        }

        public static Dictionary<uint, List<MarketplaceAvarage>> MarketplaceAvarages
        {
            get
            {
                return mMarketplaceAvarages;
            }

            set
            {
                mMarketplaceAvarages = value;
            }
        }
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
            mPages = new Dictionary<int, CatalogPage>();
            mCatalogItems = new Dictionary<int, List<CatalogItem>>();
            mCatalogItemsIdIndex = new Dictionary<uint, CatalogItem>();
            mCatalogItemsNameIndex = new Dictionary<string, CatalogItem>();

            mMarketplaceOffers = new Dictionary<uint, MarketplaceOffers>();
            mMarketplaceAvarages = new Dictionary<uint, List<MarketplaceAvarage>>();
            
            mClubOffers = new Dictionary<uint, CatalogClubOffer>();

            mRotationPages = new Dictionary<int, ItemRotationSettings>();

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
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_MARKETPLACE_POST_OFFER, new ProcessRequestCallback(MarketplacePostItem));
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
                mRotationPages.Clear();

                mPages.Add(-1, new CatalogPage(-1, 0, string.Empty, 0, 0, string.Empty, true, true, false, string.Empty, null, null, new List<CatalogItem>())); // root category

                MySqlClient.SetParameter("enabled", "1");
                DataTable ItemTable = MySqlClient.ExecuteQueryTable("SELECT * FROM catalog_items WHERE enabled = @enabled ORDER BY parent_id ASC, order_id ASC, name ASC");

                foreach (DataRow Row in ItemTable.Rows)
                {
                    int PageId = (int)Row["page_id"];
                    int ParentId = (int)Row["parent_id"];

                    if (ParentId == -1 && !mCatalogItems.ContainsKey(PageId))
                    {
                        mCatalogItems[PageId] = new List<CatalogItem>();
                    }

                    CatalogItem Item = new CatalogItem((uint)Row["id"], (int)Row["base_id"],
                        (string)Row["name"], (int)Row["cost_credits"], (int)Row["cost_activitypoints"],
                        SeasonalCurrency.FromStringToEnum(Row["seasonal_currency"].ToString()),
                        (int)Row["amount"], (string)Row["preset_flags"], (int)Row["club_restriction"],
                        (int)Row["order_id"], (string)Row["badge_code"]);

                    MySqlClient.SetParameter("enabled", "1");
                    MySqlClient.SetParameter("item_id", Item.Id);
                    DataTable RaceTable = MySqlClient.ExecuteQueryTable("SELECT * FROM catalog_pet_races WHERE catalog_item_id = @item_id AND enabled = @enabled");

                    foreach (DataRow PetRow in RaceTable.Rows)
                    {
                        PetRaceData RaceData = new PetRaceData((int)PetRow["pet_breed"],
                            (PetRow["sellable"].ToString() == "1"), (PetRow["breed_is_rare"].ToString() == "1"));

                        Item.AddPetRaceData(RaceData);
                    }

                    if (Item.DefinitionId > 0 && Item.Definition == null)
                    {
                        Output.WriteLine("Warning: Catalog item " + (uint)Row["id"] + " has an invalid base_id reference.", OutputLevel.Warning);
                        continue;
                    }

                    if (ParentId == -1)
                    {
                        mCatalogItems[PageId].Add(Item);
                        mCatalogItemsIdIndex[Item.Id] = Item;

                        string Identifier = Item.DisplayName.StartsWith("rare pet") ? "p" + Item.OrderId + " " + Item.DisplayName : Item.DisplayName;

                        mCatalogItemsNameIndex[Identifier] = Item;
                    }
                    else
                    {
                        CatalogItem ParentItem = GetCatalogItemByAbsoluteId((uint)ParentId);
                        ParentItem.AddItem(Item);
                    }
                }

                MySqlClient.SetParameter("enabled", "1");
                DataTable RotationSettingsTable = MySqlClient.ExecuteQueryTable("SELECT * FROM catalog_items_rotation_settings WHERE enabled = @enabled");
                
                foreach(DataRow Row in RotationSettingsTable.Rows)
                {
                    int PageId = (int)Row["page_id"];

                    RotationType Type;
                    switch (Row["rotation_type"].ToString().ToLower())
                    {
                        default:
                        case "day":

                            Type = RotationType.Daily;
                            break;

                        case "month":

                            Type = RotationType.Monthly;
                            break;
                    }

                    ItemRotationSettings RotationSettings = new ItemRotationSettings((uint)Row["id"],
                        (int)Row["page_id"], (int)Row["page_to_copy"], (int)Row["rotation_time"], Type,
                        (double)Row["timestamp_last_rotation"], (int)Row["last_index"]);

                    if(!mRotationPages.ContainsKey(PageId))
                    {
                        mRotationPages.Add(PageId, RotationSettings);
                    }

                    UpdateItemIndex(PageId);
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

                MySqlClient.SetParameter("enabled", "1");
                DataTable ClubTable = MySqlClient.ExecuteQueryTable("SELECT * FROM catalog_subscriptions WHERE enabled = @enabled");

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

                ReloadMarketplaceData(MySqlClient);
            }

            Output.WriteLine("Loaded " + CountLoaded + " catalog page(s).", OutputLevel.DebugInformation);

            if (NotifyUsers)
            {
                SessionManager.BroadcastPacket(CatalogUpdatedNotificationComposer.Compose());
            }
        }

        public static void ReloadMarketplaceData(SqlDatabaseClient MySqlClient)
        {
            mMarketplaceOffers.Clear();
            mMarketplaceAvarages.Clear();

            DataTable MarketplaceOffers = MySqlClient.ExecuteQueryTable("SELECT * FROM catalog_marketplace_offers");

            foreach (DataRow Row in MarketplaceOffers.Rows)
            {
                MarketplaceOffers Offer = new MarketplaceOffers((uint)Row["offer_id"], (uint)Row["user_id"],
                    (uint)Row["item_id"], int.Parse(Row["state"].ToString()), (uint)Row["sprite_id"],
                    (string)Row["extra_data"],(int)Row["asking_price"], (int)Row["total_price"],
                    int.Parse(Row["item_type"].ToString()),(double)Row["timestamp"], (int)Row["limited_number"],
                    (int)Row["limited_stack"]);

                Offer.CheckForExpiracy(MySqlClient);

                mMarketplaceOffers.Add((uint)Row["offer_id"], Offer);
            }

            DataTable MarketplaceAvarages = MySqlClient.ExecuteQueryTable("SELECT * FROM catalog_marketplace_data");
            foreach (DataRow Row in MarketplaceAvarages.Rows)
            {
                uint SpriteID = (uint)Row["sprite_id"];

                if (!mMarketplaceAvarages.ContainsKey(SpriteID))
                {
                    mMarketplaceAvarages[SpriteID] = new List<MarketplaceAvarage>();
                }

                MarketplaceAvarage MarketplaceData = new MarketplaceAvarage((uint)Row["id"], (uint)Row["sprite_id"], Row["extra_data"].ToString(),
                    (int)Row["sold_price"], int.Parse(Row["item_type"].ToString()), UnixTimestamp.GetDateTimeFromUnixTimestamp((double)Row["sold_timestamp"]));

                mMarketplaceAvarages[SpriteID].Add(MarketplaceData);
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
        public static void UpdateItemIndex(int PageId)
        {
            ItemRotationSettings Settings = mRotationPages[PageId];

            if (!mCatalogItems.ContainsKey(PageId))
            {
                mCatalogItems[PageId] = new List<CatalogItem>();
            }

            if (mCatalogItems.ContainsKey(PageId))
            {
                mCatalogItems[PageId].Clear();
            }

            if (Settings.ExecuteRotation)
            {
                Settings.LastIndex++;
                if (Settings.LastIndex >= mCatalogItems[Settings.PageToCopy].Count)
                {
                    Settings.LastIndex = 0;
                }

                Settings.UpdateInDatabase();
            }

            CatalogItem Item = mCatalogItems[Settings.PageToCopy][Settings.LastIndex];

            mCatalogItems[PageId].Add(Item);
        }

        public static void MarketplaceBuyTickets(Session Session, ClientMessage Message)
        {
            bool CreditsError = false;

            if (Session.CharacterInfo.MarketplaceTokens < 0)
            {
                Session.CharacterInfo.MarketplaceTokens = 0;
            }

            if (Session.CharacterInfo.CreditsBalance <= 0)
            {
                CreditsError = true;
            }

            if (CreditsError)
            {
                Session.SendData(CatalogPurchaseBalanceErrorComposer.Composer(CreditsError, false));
                return;
            }
            else
            {
                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, -ServerSettings.MarketplaceTokensPrice);
                    Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                    Session.CharacterInfo.UpdateMarketplaceTokens(MySqlClient, Session.HasRight("club_vip") ?
                        ServerSettings.MarketplacePremiumTokens : ServerSettings.MarketplaceNormalTokens);
                }

                MarketplaceCanSell(Session, Message);
            }
        }

        public static void MarketplaceItemStats(Session Session, ClientMessage Message)
        {
            /* 
             * Posters has same SpriteId, and still impossible count his offers by extradata.
             * 
             * Client side message example:
             * oLJYhO => Server has received this message
             * 
             * If we decoded this message example we had this:
             * oL  = 3020 > ClientMessage Header Id
             * J   = 2    > int ItemType
             * YhO = 4001 > uint Sprite
             */

            int ItemType = Message.PopWiredInt32();
            uint Sprite = Message.PopWiredUInt32();

            int OfferCount = MarketplaceManager.CountForSprite(Sprite);
            int Average = MarketplaceManager.AveragePriceForItem(ItemType, Sprite);

            Session.SendData(CatalogMarketplaceItemStatsComposer.Compose(Average, OfferCount, Sprite, ItemType));
        }

        public static void MarketplaceConfig(Session Session, ClientMessage Message)
        {
            Session.SendData(CatalogMarketplaceConfigComposer.Compose(Session));
        }

        private static void MarketplaceGetOffers(Session Session, ClientMessage Message)
        {
            int MinPrice = Message.PopWiredInt32();
            int MaxPrice = Message.PopWiredInt32();
            string SearchQuery = Message.PopString();
            int FilterMode = Message.PopWiredInt32();

            Session.MarketplaceFiltersCache.FillCache(MinPrice, MaxPrice, SearchQuery, FilterMode);
            MarketplaceManager.SerializeOffers(Session, MinPrice, MaxPrice, SearchQuery, FilterMode);
        }

        private static void MarketplaceTakeBack(Session Session, ClientMessage Message)
        {
            uint ItemId = Message.PopWiredUInt32();

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                MarketplaceManager.CancelOffer(Session, MySqlClient, ItemId);
            }
        }

        private static void MarketplaceCanSell(Session Session, ClientMessage Message)
        {
            if (Session.CharacterInfo.MarketplaceTokens < 0)
            {
                Session.CharacterInfo.MarketplaceTokens = 0;
            }

            Session.SendData(CatalogMarketplaceCanSellComposer.Compose(Session));
        }

        private static void MarketplacePurchase(Session Session, ClientMessage Message)
        {
            uint ItemId = Message.PopWiredUInt32();

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                MarketplaceManager.Purchase(Session, MySqlClient, ItemId);
            }
        }

        private static void MarketplacePostItem(Session Session, ClientMessage Message)
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

                int AskingPrice = Message.PopWiredInt32();
                int ItemType = Message.PopWiredInt32();
                uint Id = Message.PopWiredUInt32();

                Item UserItem = Session.InventoryCache.GetItem(Id);
                if (UserItem != null && UserItem.CanSell)
                {
                    MarketplaceManager.SellItem(Session, Id, ItemType, AskingPrice);
                }
            }
        }

        private static void MarketplaceClaimCredits(Session Session, ClientMessage Message)
        {
            MarketplaceManager.RedeemCredits(Session);
        }

        private static void MarketplaceGetOwnOffers(Session Session, ClientMessage Message)
        {
            MarketplaceManager.SerializeOffers(Session, -1, -1, "", 0);
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
            int GiftBoxId = Message.PopWiredInt32();
            int Ribbon = Message.PopWiredInt32();

            if (!ServerSettings.GiftingSystemEnabled)
            {
                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("catalog_gift_disabled")));
                return;
            }

            CatalogPage Page = CatalogManager.GetCatalogPage(PageId);
            if (Page == null || Page.DummyPage || Page.ComingSoon || !Page.Visible || (Page.RequiredRight.Length > 0 &&
                !Session.HasRight(Page.RequiredRight)))
            {
                Session.SendData(CatalogGiftsWrappingErrorComposer.Composer());
                return;
            }

            CatalogItem Item = Page.GetItem(ItemId);
            if (Item == null || !Item.CanGift() ||
            (Item.ClubRestriction == 1 && !Session.HasRight("club_regular")) ||
            (Item.ClubRestriction == 2 && !Session.HasRight("club_vip")))
            {
                Session.SendData(CatalogGiftsWrappingErrorComposer.Composer());
                return;
            }

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                CatalogPurchaseHandler.HandlePurchaseGift(MySqlClient, Session, Item, ExtraData, GiftUser, GiftMessage, SpriteId, GiftBoxId, Ribbon);
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
                string Code = Message.PopString();

                Session.SendData(VoucherManager.TryRedeemVoucher(MySqlClient, Session, Code) ?
                    CatalogRedeemOkComposer.Compose() : CatalogRedeemErrorComposer.Compose(VoucherManager.ErrorChecker(Code)));
            }
        }

        private static void GetPetData(Session Session, ClientMessage Message)
        {
            CatalogItem PetItem = null;

            string ItemName = Message.PopString();

            ItemRotationSettings Settings = mRotationPages[179];

            string PetIdentifier = ItemName.StartsWith("rare pet") ? "p" + Settings.LastIndex + " " + ItemName : ItemName;

            if (mCatalogItemsNameIndex.ContainsKey(PetIdentifier))
            {
                PetItem = mCatalogItemsNameIndex[PetIdentifier];
            }

            if (PetItem == null)
            {
                return;
            }

            ItemDefinition Def = PetItem.Definition;

            if (Def == null || Def.Behavior != ItemBehavior.Pet)
            {
                return;
            }

            int PetType = Def.BehaviorData;

            Session.SendData(CatalogPetDataComposer.Compose(PetItem, PetType));
        }

        private static void CheckPetName(Session Session, ClientMessage Message)
        {
            Session.SendData(CatalogVerifyPetNameResultComposer.Compose((int)PetName.VerifyPetName(Message.PopString())));
        }
    }
}
