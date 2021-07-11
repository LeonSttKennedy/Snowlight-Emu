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
        private static MarketplaceManager Marketplace;
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
            Marketplace = new MarketplaceManager();
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

        public static void MarketplaceBuyTickets(Session Session, ClientMessage Message)
        {
            bool CreditsError = false;

            if (Session.CharacterInfo.MarketplaceTokensTotal < 0)
            {
                Session.CharacterInfo.MarketplaceTokensTotal = 0;
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
                    Session.CharacterInfo.UpdateMarketplaceTokens(MySqlClient, Session.HasHcOrVip() ? ServerSettings.MarketplacePremiumTokens : ServerSettings.MarketplaceNormalTokens);
                }

                MarketplaceCanSell(Session, Message);
            }
        }

        public static void MarketplaceItemStats(Session Session, ClientMessage Message)
        {
            int ItemType = Message.PopWiredInt32();
            uint Sprite = Message.PopWiredUInt32();

            Session.SendData(CatalogMarketplaceItemStatsComposer.Compose(CatalogManager.Marketplace.OfferCountForSprite(Sprite), Sprite, ItemType));
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

            CatalogManager.Marketplace.SerializeOffers(Session, MinPrice, MaxPrice, SearchQuery, FilterMode);
        }

        private static void MarketplaceTakeBack(Session Session, ClientMessage Message)
        {
            uint ItemId = Message.PopWiredUInt32();
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                CatalogManager.Marketplace.CancelOffer(Session, MySqlClient, ItemId);
            }
        }

        private static void MarketplaceCanSell(Session Session, ClientMessage Message)
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
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                CatalogManager.Marketplace.Purchase(Session, MySqlClient, ItemId);
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
                if (UserItem != null && CatalogManager.Marketplace.CanSellItem(UserItem))
                {
                    CatalogManager.Marketplace.SellItem(Session, itemId, sellingPrice);
                }
            }
        }

        private static void MarketplaceClaimCredits(Session Session, ClientMessage Message)
        {
            CatalogManager.Marketplace.RedeemCredits(Session);
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
