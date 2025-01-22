using System;

using Snowlight.Game.Sessions;
using Snowlight.Communication;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Items;
using System.Collections.Generic;
using Snowlight.Specialized;
using Snowlight.Storage;
using Snowlight.Game.AvatarEffects;
using Snowlight.Util;
using Snowlight.Game.Achievements;
using Snowlight.Game.Rights;
using Snowlight.Game.Pets;
using Snowlight.Game.Rooms;
using Snowlight.Communication.Incoming;
using System.Data;
using Snowlight.Game.Characters;
using System.Linq;

namespace Snowlight.Game.Catalog
{
    public static class CatalogPurchaseHandler
    {
        private static object mPurchaseSyncRoot;

        public static void Initialize()
        {
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_CAN_GIFT, new ProcessRequestCallback(CheckCanGift));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_PURCHASE, new ProcessRequestCallback(OnPurchase));

            mPurchaseSyncRoot = new object();
        }

        #region Purchase handling and delivering

        #region Purchase
        public static void HandlePurchase(SqlDatabaseClient MySqlClient, Session Session, CatalogItem Item,
            string ItemFlags)
        {
            lock (mPurchaseSyncRoot)
            {
                int TotalCreditCost = Item.CostCredits;
                int TotalApCost = Item.CostActivityPoints;

                if (Session.CharacterInfo.CreditsBalance < TotalCreditCost ||
                    Session.CharacterInfo.ActivityPoints[(int)Item.SeasonalCurrency] < TotalApCost)
                {
                    return;
                }

                #region Arrange To Delivery
                List<CatalogItem> ItemsToDelivery = new List<CatalogItem>();

                if (Item.IsDeal)
                {
                    ItemsToDelivery.AddRange(Item.DealItems);
                }
                else
                {
                    ItemsToDelivery.Add(Item);
                }
                #endregion

                string[] PetData = null;

                foreach (CatalogItem ToDelivery in ItemsToDelivery)
                {
                    if (ToDelivery.PresetFlags.Length > 0)
                    {
                        ItemFlags = Item.PresetFlags;
                    }
                    else
                    {
                        switch (ToDelivery.Definition.Behavior)
                        {
                            case ItemBehavior.Pet:

                                PetData = ItemFlags.Split('\n');

                                if (PetData.Length != 3)
                                {
                                    return;
                                }

                                string Name = PetData[0];
                                string Color = PetData[2];

                                int Race = 0;
                                int.TryParse(PetData[1], out Race);

                                bool RaceOk = false;

                                List<PetRaceData> Races = PetDataManager.GetRaceDataForType(ToDelivery.Definition.BehaviorData);

                                foreach (PetRaceData RaceData in Races)
                                {
                                    if (RaceData.Breed == Race)
                                    {
                                        RaceOk = true;
                                        break;
                                    }
                                }

                                if (PetName.VerifyPetName(Name) != PetNameError.NameOk || !RaceOk)
                                {
                                    return;
                                }

                                break;

                            case ItemBehavior.PrizeTrophy:

                                if (ItemFlags.Length > 255)
                                {
                                    ItemFlags = ItemFlags.Substring(0, 255);
                                }

                                ItemFlags = Session.CharacterInfo.Username + Convert.ToChar(9) + DateTime.Now.Day + "-" +
                                    DateTime.Now.Month + "-" + DateTime.Now.Year + Convert.ToChar(9) +
                                    UserInputFilter.FilterString(ItemFlags.Trim(), true);
                                break;

                            default:

                                ItemFlags = string.Empty;
                                break;
                        }
                    }
                }

                if (TotalCreditCost > 0)
                {
                    Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, -TotalCreditCost);
                    Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                }

                if (TotalApCost > 0)
                {
                    Session.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, Item.SeasonalCurrency, -TotalApCost);
                    if (Item.SeasonalCurrency == SeasonalCurrencyList.Pixels)
                    {
                        Session.SendData(UpdatePixelsBalanceComposer.Compose(Session.CharacterInfo.ActivityPoints[0], -TotalApCost));
                    }
                    Session.SendData(UserActivityPointsBalanceComposer.Compose(Session.CharacterInfo.ActivityPoints));
                }

                Dictionary<int, List<uint>> NewItems = new Dictionary<int, List<uint>>();

                foreach (CatalogItem ToDelivery in ItemsToDelivery)
                {
                    for (int i = 0; i < ToDelivery.Amount; i++)
                    {
                        switch (ToDelivery.Definition.Type)
                        {
                            default:

                                List<Item> GeneratedGenericItems = new List<Item>();
                                double ExpireTimestamp = 0;

                                switch (ToDelivery.Definition.Behavior)
                                {
                                    case ItemBehavior.Rental:

                                        ExpireTimestamp = UnixTimestamp.GetCurrent() + 3600;
                                        break;

                                    case ItemBehavior.DuckHC:
                                    case ItemBehavior.DuckVIP:

                                        string BasicAchievement = "ACH_BasicClub";
                                        string VipAchievement = "ACH_VipClub";

                                        double Length = 86400 * ToDelivery.Definition.BehaviorData;

                                        ClubSubscriptionLevel Level = ToDelivery.Definition.Behavior == ItemBehavior.DuckVIP ?
                                            ClubSubscriptionLevel.VipClub : ToDelivery.Definition.Behavior == ItemBehavior.DuckHC ?
                                            ClubSubscriptionLevel.BasicClub : ClubSubscriptionLevel.None;

                                        if (Session.SubscriptionManager.IsActive &&
                                            Session.SubscriptionManager.SubscriptionLevel == ClubSubscriptionLevel.VipClub)
                                        {
                                            Level = ClubSubscriptionLevel.VipClub;
                                        }

                                        // Extend membership
                                        Session.SubscriptionManager.AddOrExtend((int)Level, Length);

                                        // Check if we need to manually award basic/vip badges
                                        bool NeedsBasicUnlock = !Session.BadgeCache.ContainsCodeWith(BasicAchievement)
                                            && Level >= ClubSubscriptionLevel.BasicClub;

                                        bool NeedsVipUnlock = !Session.BadgeCache.ContainsCodeWith(VipAchievement)
                                            && Level == ClubSubscriptionLevel.VipClub;

                                        // Reload the badge cache (reactivating any disabled subscription badges)
                                        Session.BadgeCache.ReloadCache(MySqlClient, Session.AchievementCache);

                                        // Virtually unlock the basic achievement without reward if needed
                                        if (NeedsBasicUnlock)
                                        {
                                            Achievement Achievement = AchievementManager.GetAchievement(BasicAchievement);

                                            if (Achievement != null)
                                            {
                                                UserAchievement UserAchievement = Session.AchievementCache.GetAchievementData(
                                                    BasicAchievement);

                                                if (UserAchievement != null)
                                                {
                                                    Session.SendData(AchievementUnlockedComposer.Compose(Achievement, UserAchievement.Level,
                                                        0 , 0, 0));
                                                }
                                            }
                                        }

                                        // Virtually unlock the VIP achievement without reward if needed
                                        if (NeedsVipUnlock)
                                        {
                                            Achievement Achievement = AchievementManager.GetAchievement(VipAchievement);

                                            if (Achievement != null)
                                            {
                                                UserAchievement UserAchievement = Session.AchievementCache.GetAchievementData(
                                                    VipAchievement);

                                                if (UserAchievement != null)
                                                {
                                                    Session.SendData(AchievementUnlockedComposer.Compose(Achievement, UserAchievement.Level,
                                                        0, 0, 0));
                                                }
                                            }
                                        }

                                        // Disable any VIP badges if they still aren't valid
                                        if (Session.SubscriptionManager.SubscriptionLevel < ClubSubscriptionLevel.VipClub)
                                        {
                                            Session.BadgeCache.DisableSubscriptionBadge(VipAchievement);
                                        }

                                        // Synchronize equipped badges if the user has unlocked anything
                                        if (NeedsVipUnlock || NeedsBasicUnlock)
                                        {
                                            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

                                            if (Instance != null)
                                            {
                                                Instance.BroadcastMessage(RoomUserBadgesComposer.Compose(Session.CharacterId,
                                                    Session.BadgeCache.EquippedBadges));
                                            }
                                        }

                                        Session.SubscriptionManager.UpdateUserBadge();

                                        // Clear catalog cache for user (in case of changes)
                                        CatalogManager.ClearCacheGroup(Session.CharacterId);

                                        // Send new data to client
                                        Session.SendData(FuseRightsListComposer.Compose(Session));
                                        Session.SendData(SubscriptionStatusComposer.Compose(Session.SubscriptionManager, true));

                                        if (Session.SubscriptionManager.GiftPoints > 0)
                                        {
                                            Session.SendData(ClubGiftReadyComposer.Compose(Session.SubscriptionManager.GiftPoints));
                                        }

                                        break;
                                }

                                GeneratedGenericItems.Add(ItemFactory.CreateItem(MySqlClient, ToDelivery.Definition.Id,
                                    Session.CharacterId, ItemFlags, ItemFlags, ExpireTimestamp));

                                switch (ToDelivery.Definition.Behavior)
                                {
                                    case ItemBehavior.Teleporter:

                                        Item LinkedItem = ItemFactory.CreateItem(MySqlClient, ToDelivery.Definition.Id,
                                            Session.CharacterId, GeneratedGenericItems[0].Id.ToString(), string.Empty,
                                            ExpireTimestamp);

                                        GeneratedGenericItems[0].Flags = LinkedItem.Id.ToString();
                                        GeneratedGenericItems[0].SynchronizeDatabase(MySqlClient, true);

                                        GeneratedGenericItems.Add(LinkedItem);
                                        break;
                                }

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

                                break;

                            case ItemType.AvatarEffect:

                                AvatarEffect Effect = null;

                                if (Session.AvatarEffectCache.HasEffect((int)ToDelivery.Definition.SpriteId))
                                {
                                    Effect = Session.AvatarEffectCache.GetEffect((int)ToDelivery.Definition.SpriteId);

                                    if (Effect != null)
                                    {
                                        Effect.AddToQuantity();
                                    }
                                }
                                else
                                {
                                    Effect = AvatarEffectFactory.CreateEffect(MySqlClient, Session.CharacterId, (int)ToDelivery.Definition.SpriteId, 3600);
                                    Session.AvatarEffectCache.Add(Effect);
                                }

                                if (Effect != null)
                                {
                                    Session.SendData(UserEffectAddedComposer.Compose(Effect));
                                }

                                break;

                            case ItemType.Pet:

                                Pet Pet = PetFactory.CreatePet(MySqlClient, Session.CharacterId, ToDelivery.Definition.BehaviorData, PetData[0], int.Parse(PetData[1]), PetData[2]);
                                Session.InventoryCache.Add(Pet);

                                Session.SendData(PetReceivedComposer.Compose(false, Pet));
                                Session.SendData(InventoryPetAddedComposer.Compose(Pet));

                                if (!NewItems.ContainsKey(3))
                                {
                                    NewItems.Add(3, new List<uint>());
                                }

                                NewItems[3].Add(Pet.Id);

                                AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_PetLover", 1);
                                break;
                        }
                    }
                }

                #region Delivery Badge
                if (Item.BadgeCode != string.Empty)
                {
                    BadgeDefinition BadgeToGive = RightsManager.GetBadgeDefinitionByCode(Item.BadgeCode);
                    if (BadgeToGive == null)
                    {
                        return;
                    }

                    if (!Session.BadgeCache.ContainsCode(Item.BadgeCode))
                    {
                        Session.BadgeCache.UpdateAchievementBadge(MySqlClient, BadgeToGive.Code, BadgeToGive, Session.AchievementCache, "static");

                        if (!NewItems.ContainsKey(4))
                        {
                            NewItems.Add(4, new List<uint>());
                        }

                        InventoryBadge UserBadge = Session.BadgeCache.GetBadge(Item.BadgeCode);

                        NewItems[4].Add(UserBadge.Id);
                    }
                }
                #endregion

                if (Item.Definition != null && !Item.Definition.Type.Equals(ItemType.Pet))
                {
                    Session.SendData(CatalogPurchaseResultComposer.Compose(Item));
                }

                Session.SendData(InventoryRefreshComposer.Compose());

                foreach (KeyValuePair<int, List<uint>> NewItemData in NewItems)
                {
                    foreach (uint NewItem in NewItemData.Value)
                    {
                        Session.NewItemsCache.MarkNewItem(MySqlClient, NewItemData.Key, NewItem);
                    }
                }

                if (NewItems.Count > 0)
                {
                    Session.NewItemsCache.SendNewItems(Session);
                }
            }
        }
        #endregion

        #region Gift Purchase
        public static void HandlePurchaseGift(SqlDatabaseClient MySqlClient, Session Session, CatalogItem Item,
            string ItemFlags, string GiftUser, string GiftMessage, int SpriteId, int GiftBoxId, int Ribbon)
        {
            lock (mPurchaseSyncRoot)
            {
                TimeSpan LastGiftSeconds = DateTime.Now - Session.CharacterInfo.LastGiftSent;

                if (LastGiftSeconds.TotalSeconds <= 15.0)
                {
                    Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("catalog_gift_sent_need_wait_15sec")));
                    Session.CharacterInfo.GiftWarningCounter += 1;
                    if (Session.CharacterInfo.GiftWarningCounter >= 20)
                    {
                        Session.CharacterInfo.AllowGifting = false;
                    }
                    return;
                }

                if (!Session.CharacterInfo.AllowGifting)
                {
                    return;
                }

                int TotalCreditCost = (SpriteId != 0 ? ServerSettings.GiftingSystemPrice + Item.CostCredits : Item.CostCredits);
                int TotalApCost = Item.CostActivityPoints;

                double ExpireTimestamp = 0;

                CharacterInfo GiftReceiverInfo = CharacterInfoLoader.GetCharacterInfo(MySqlClient, CharacterResolverCache.GetUidFromName(GiftUser));

                Session GiftReceiverSession = SessionManager.GetSessionByCharacterId(GiftReceiverInfo.Id);

                if (GiftReceiverInfo == null)
                {
                    Session.SendData(CatalogGiftsWrappingErrorComposer.Composer());
                    return;
                }

                if (!GiftReceiverInfo.AllowGifting)
                {
                    Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("target_user_disabled_receipt_gifts", GiftUser)));
                    Session.SendData(CatalogGiftsWrappingErrorComposer.Composer());
                    return;
                }

                if (Session.CharacterInfo.CreditsBalance < TotalCreditCost ||
                    Session.CharacterInfo.ActivityPoints[(int)Item.SeasonalCurrency] < TotalApCost)
                {
                    return;
                }

                if (Item.PresetFlags.Length > 0)
                {
                    ItemFlags = Item.PresetFlags;
                }
                else
                {
                    #region is catalog deal
                    if (Item.IsDeal)
                    {
                        foreach (CatalogItem DealItems in Item.DealItems)
                        {
                            switch (DealItems.Definition.Behavior)
                            {
                                case ItemBehavior.PrizeTrophy:

                                    if (ItemFlags.Length > 255)
                                    {
                                        ItemFlags = ItemFlags.Substring(0, 255);
                                    }

                                    ItemFlags = Session.CharacterInfo.Username + Convert.ToChar(9) + DateTime.Now.Day + "-" +
                                        DateTime.Now.Month + "-" + DateTime.Now.Year + Convert.ToChar(9) +
                                        UserInputFilter.FilterString(ItemFlags.Trim(), true);
                                    break;

                                case ItemBehavior.Rental:

                                    ExpireTimestamp = UnixTimestamp.GetCurrent() + 3600;
                                    break;

                                default:

                                    ItemFlags = string.Empty;
                                    break;
                            }
                        }
                    }
                    #endregion
                    else
                    {
                        switch (Item.Definition.Behavior)
                        {
                            case ItemBehavior.PrizeTrophy:

                                if (ItemFlags.Length > 255)
                                {
                                    ItemFlags = ItemFlags.Substring(0, 255);
                                }

                                ItemFlags = Session.CharacterInfo.Username + Convert.ToChar(9) + DateTime.Now.Day + "-" +
                                    DateTime.Now.Month + "-" + DateTime.Now.Year + Convert.ToChar(9) +
                                    UserInputFilter.FilterString(ItemFlags.Trim(), true);
                                break;

                            case ItemBehavior.Rental:

                                ExpireTimestamp = UnixTimestamp.GetCurrent() + 3600;
                                break;

                            default:

                                ItemFlags = string.Empty;
                                break;
                        }
                    }
                }

                if (SpriteId == 0)
                {
                    SpriteId = (int)ItemDefinitionManager.OldGiftDefinitions().SpriteId;
                }

                string ED = "!" + GiftMessage + "|" + SpriteId + "|" + GiftBoxId + "|" + Ribbon;

                Dictionary<int, List<uint>> NewItems = new Dictionary<int, List<uint>>();
                List<Item> GeneratedGenericItems = new List<Item>();

                ItemDefinition ItemDef = ItemDefinitionManager.GetDefinitionBySpriteId(uint.Parse(SpriteId.ToString()));
                GeneratedGenericItems.Add(ItemFactory.CreateItem(MySqlClient, ItemDef.Id,
                    GiftReceiverInfo.Id, ED, ED, ExpireTimestamp));

                #region Arrange To Delivery
                List<CatalogItem> ItemsToDelivery = new List<CatalogItem>();

                if (Item.IsDeal)
                {
                    ItemsToDelivery.AddRange(Item.DealItems);
                }
                else
                {
                    ItemsToDelivery.Add(Item);
                }
                #endregion

                foreach (Item GeneratedItem in GeneratedGenericItems)
                {
                    List<string> DefIds = new List<string>();
                    List<string> Amount = new List<string>();
                    foreach(CatalogItem DealItems in ItemsToDelivery)
                    {
                        DefIds.Add(DealItems.Definition.Id.ToString());
                        Amount.Add(DealItems.Amount.ToString());
                    }

                    MySqlClient.SetParameter("itemid", GeneratedItem.Id);
                    MySqlClient.SetParameter("baseids", string.Join("|", DefIds));
                    MySqlClient.SetParameter("amounts", string.Join("|", Amount));
                    MySqlClient.SetParameter("flags", ItemFlags);
                    DataTable Table = MySqlClient.ExecuteQueryTable("INSERT INTO user_gifts(item_id, base_ids, amounts, extra_data) VALUES(@itemid, @baseids, @amounts, @flags)");

                    if (GiftReceiverSession != null)
                    {
                        GiftReceiverSession.InventoryCache.Add(GeneratedItem);
                    }

                    if (!NewItems.ContainsKey(1))
                    {
                        NewItems.Add(1, new List<uint>());
                    }

                    NewItems[1].Add(GeneratedItem.Id);
                }

                if (TotalCreditCost > 0)
                {
                    Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, -TotalCreditCost);
                    Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                }

                if (TotalApCost > 0)
                {
                    Session.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, Item.SeasonalCurrency, -TotalApCost);
                    if (Item.SeasonalCurrency == SeasonalCurrencyList.Pixels)
                    {
                        Session.SendData(UpdatePixelsBalanceComposer.Compose(Session.CharacterInfo.ActivityPoints[0], -TotalApCost));
                    }

                    Session.SendData(UserActivityPointsBalanceComposer.Compose(Session.CharacterInfo.ActivityPoints));
                }

                if (GiftReceiverSession != null) 
                {
                    foreach (KeyValuePair<int, List<uint>> NewItemData in NewItems)
                    {
                        foreach (uint NewItem in NewItemData.Value)
                        {
                            GiftReceiverSession.NewItemsCache.MarkNewItem(MySqlClient, NewItemData.Key, NewItem);
                        }
                    }

                    if (NewItems.Count > 0)
                    {
                        GiftReceiverSession.SendData(InventoryNewItemsComposer.Compose(new Dictionary<int, List<uint>>(NewItems)));
                    }

                    GiftReceiverSession.SendData(InventoryRefreshComposer.Compose());
                }

                Session.SendData(CatalogPurchaseResultComposer.Compose(Item));

                Session.CharacterInfo.LastGiftSent = DateTime.Now;

                if (Session.CharacterInfo.Username != GiftReceiverInfo.Username)
                {
                    if (GiftReceiverInfo.HasLinkedSession)
                    {
                        AchievementManager.ProgressUserAchievement(MySqlClient, GiftReceiverSession, "ACH_GiftReceiver", 1);
                    }
                    else
                    {
                        AchievementManager.OfflineProgressUserAchievement(MySqlClient, GiftReceiverInfo.Id, "ACH_GiftReceiver", 1);
                    }

                    AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_GiftGiver", 1);
                }
            }
        }
        #endregion

        #endregion

        #region Request processors
        private static void CheckCanGift(Session Session, ClientMessage Message)
        {
            uint ItemId = Message.PopWiredUInt32();

            CatalogItem Item = CatalogManager.GetCatalogItemByAbsoluteId(ItemId);

            if (Item == null)
            {
                return;
            }

            Session.SendData(CatalogCanGiftComposer.Compose(Item.Id, Item.CanGift()));
        }

        private static void OnPurchase(Session Session, ClientMessage Message)
        {
            int PageId = Message.PopWiredInt32();
            uint ItemId = Message.PopWiredUInt32();
            string Data = Message.PopString();

            CatalogPage Page = CatalogManager.GetCatalogPage(PageId);

            if (Page == null || Page.DummyPage || Page.ComingSoon || !Page.Visible || (Page.RequiredRight.Length > 0 && 
                !Session.HasRight(Page.RequiredRight)))
            {
                return;
            }

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                switch (Page.Template)
                {
                    default:

                        CatalogItem Item = Page.GetItem(ItemId);

                        if (Item == null || (Item.ClubRestriction == 1 && !Session.HasRight("club_regular")) ||
                            (Item.ClubRestriction == 2 && !Session.HasRight("club_vip")))
                        {
                            return;
                        }

                        HandlePurchase(MySqlClient, Session, Item, Data);
                        break;

                    case "club_buy":

                        CatalogClubOffer Offer = CatalogManager.GetClubOffer(ItemId);

                        if (Offer == null || (Offer.Price > 0 && Session.CharacterInfo.CreditsBalance < Offer.Price)
                            || (int)Offer.Level < (int)Session.SubscriptionManager.SubscriptionLevel)
                        {
                            return;
                        }

                        string BasicAchievement = "ACH_BasicClub";
                        string VipAchievement = "ACH_VipClub";

                        // Extend membership and take credits
                        Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, -Offer.Price);
                        Session.SubscriptionManager.AddOrExtend((int)Offer.Level, Offer.LengthSeconds);

                        // Check if we need to manually award basic/vip badges
                        bool NeedsBasicUnlock = !Session.BadgeCache.ContainsCodeWith(BasicAchievement)
                            && Offer.Level >= ClubSubscriptionLevel.BasicClub;

                        bool NeedsVipUnlock = !Session.BadgeCache.ContainsCodeWith(VipAchievement)
                            && Offer.Level == ClubSubscriptionLevel.VipClub;

                        // Reload the badge cache (reactivating any disabled subscription badges)
                        Session.BadgeCache.ReloadCache(MySqlClient, Session.AchievementCache);

                        // Virtually unlock the basic achievement without reward if needed
                        if (NeedsBasicUnlock)
                        {
                            Achievement Achievement = AchievementManager.GetAchievement(BasicAchievement);

                            if (Achievement != null)
                            {
                                UserAchievement UserAchievement = Session.AchievementCache.GetAchievementData(
                                    BasicAchievement);

                                if (UserAchievement != null)
                                {
                                    Session.SendData(AchievementUnlockedComposer.Compose(Achievement, UserAchievement.Level,
                                        0, 0, 0));
                                }
                            }
                        }

                        // Virtually unlock the VIP achievement without reward if needed
                        if (NeedsVipUnlock)
                        {
                            Achievement Achievement = AchievementManager.GetAchievement(VipAchievement);

                            if (Achievement != null)
                            {
                                UserAchievement UserAchievement = Session.AchievementCache.GetAchievementData(
                                    VipAchievement);

                                if (UserAchievement != null)
                                {
                                    Session.SendData(AchievementUnlockedComposer.Compose(Achievement, UserAchievement.Level,
                                        0, 0, 0));
                                }
                            }
                        }

                        // Disable any VIP badges if they still aren't valid
                        if (Session.SubscriptionManager.SubscriptionLevel < ClubSubscriptionLevel.VipClub)
                        {
                            Session.BadgeCache.DisableSubscriptionBadge(VipAchievement);
                        }

                        // Synchronize equipped badges if the user has unlocked anything
                        if (NeedsVipUnlock || NeedsBasicUnlock)
                        {
                            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

                            if (Instance != null)
                            {
                                Instance.BroadcastMessage(RoomUserBadgesComposer.Compose(Session.CharacterId,
                                    Session.BadgeCache.EquippedBadges));
                            }
                        }

                        Session.SubscriptionManager.UpdateUserBadge();

                        SubscriptionOffer SubscriptionOffer = SubscriptionOfferManager.CheckForSubOffer(Session.SubscriptionManager.SubscriptionLevel, Session.CharacterId);
                        if (SubscriptionOffer != null && SubscriptionOffer.BaseOffer.Id == Offer.Id 
                            && !SubscriptionOffer.BasicSubscriptionReminder)
                        {
                            SubscriptionOffer.UpdateUserIdList(MySqlClient, Session.CharacterId);
                        }

                        // Clear catalog cache for user (in case of changes)
                        CatalogManager.ClearCacheGroup(Session.CharacterId);

                        // Send new data to client
                        Session.SendData(CatalogPurchaseResultComposer.Compose(Offer));
                        Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                        Session.SendData(FuseRightsListComposer.Compose(Session));
                        Session.SendData(SubscriptionStatusComposer.Compose(Session.SubscriptionManager, true));

                        if (Session.SubscriptionManager.GiftPoints > 0)
                        {
                            Session.SendData(ClubGiftReadyComposer.Compose(Session.SubscriptionManager.GiftPoints));
                        }

                        break;
                }
            }
        }
        #endregion
    }
}
