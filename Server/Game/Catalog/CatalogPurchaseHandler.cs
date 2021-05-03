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
        public static void HandlePurchase(SqlDatabaseClient MySqlClient, Session Session, CatalogItem Item,
            string ItemFlags, bool MarketplaceTakeBack = false, bool IsMarketPrice = false, int MarketPrice = 0)
        {
            lock (mPurchaseSyncRoot)
            {
                int TotalCreditCost = (IsMarketPrice && MarketPrice > 0 ? MarketPrice : Item.CostCredits);
                int TotalApCost = Item.CostActivityPoints;

                if (Session.CharacterInfo.CreditsBalance < TotalCreditCost || Session.CharacterInfo.ActivityPointsBalance
                    < TotalApCost)
                {
                    return;
                }

                string[] PetData = null;

                if (Item.PresetFlags.Length > 0)
                {
                    ItemFlags = Item.PresetFlags;
                }
                else
                {
                    switch (Item.Definition.Behavior)
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

                            List<PetRaceData> Races = PetDataManager.GetRaceDataForType(Item.Definition.BehaviorData);

                            foreach (PetRaceData RaceData in Races)
                            {
                                if (RaceData.Data1 == Race)
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

                if (MarketplaceTakeBack == false)
                {
                    if (TotalCreditCost > 0)
                    {
                        Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, -TotalCreditCost);
                        Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                    }

                    if (TotalApCost > 0)
                    {
                        Session.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, -TotalApCost);
                        Session.SendData(ActivityPointsBalanceComposer.Compose(Session.CharacterInfo.ActivityPointsBalance, -TotalApCost));
                    }
                }

                Dictionary<int, List<uint>> NewItems = new Dictionary<int, List<uint>>();

                for (int i = 0; i < Item.Amount; i++)
                {
                    switch (Item.Definition.Type)
                    {
                        default:

                            List<Item> GeneratedGenericItems = new List<Item>();
                            double ExpireTimestamp = 0;

                            if (Item.Definition.Behavior == ItemBehavior.Rental)
                            {
                                ExpireTimestamp = UnixTimestamp.GetCurrent() + 3600;
                            }

                            GeneratedGenericItems.Add(ItemFactory.CreateItem(MySqlClient, Item.DefinitionId,
                                Session.CharacterId, ItemFlags, ItemFlags, ExpireTimestamp));

                            switch (Item.Definition.Behavior)
                            {
                                case ItemBehavior.Teleporter:

                                    Item LinkedItem = ItemFactory.CreateItem(MySqlClient, Item.DefinitionId,
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

                            if (Session.AvatarEffectCache.HasEffect((int)Item.Definition.SpriteId))
                            {
                                Effect = Session.AvatarEffectCache.GetEffect((int)Item.Definition.SpriteId);

                                if (Effect != null)
                                {
                                    Effect.AddToQuantity();
                                }
                            }
                            else
                            {
                                Effect = AvatarEffectFactory.CreateEffect(MySqlClient, Session.CharacterId, (int)Item.Definition.SpriteId, 3600);
                                Session.AvatarEffectCache.Add(Effect);
                            }

                            if (Effect != null)
                            {
                                Session.SendData(UserEffectAddedComposer.Compose(Effect));
                            }

                            break;

                        case ItemType.Pet:

                            Pet Pet = PetFactory.CreatePet(MySqlClient, Session.CharacterId, Item.Definition.BehaviorData, PetData[0], int.Parse(PetData[1]), PetData[2]);
                            Session.PetInventoryCache.Add(Pet);

                            Session.SendData(InventoryPetAddedComposer.Compose(Pet));

                            if (!NewItems.ContainsKey(3))
                            {
                                NewItems.Add(3, new List<uint>());
                            }

                            NewItems[3].Add(Pet.Id);

                            break;
                    }
                }
                if (MarketplaceTakeBack == false)
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
                    Session.SendData(InventoryNewItemsComposer.Compose(new Dictionary<int, List<uint>>(NewItems)));
                }
            }
        }
        public static void HandlePurchaseGift(SqlDatabaseClient MySqlClient, Session Session, CatalogItem Item,
            string ItemFlags, string GiftUser, string GiftMessage, int SpriteId, int Ribbon, int Colour)
        {
            lock (mPurchaseSyncRoot)
            {
                int TotalCreditCost = (SpriteId != 0? 1 + Item.CostCredits : Item.CostCredits);
                int TotalApCost = Item.CostActivityPoints;
                DataRow GiftUserRow = MySqlClient.ExecuteQueryRow("SELECT * FROM characters WHERE username = '" + GiftUser + "' LIMIT 1");
                Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(GiftUser));

                if (GiftUserRow == null)
                {
                    Session.SendData(CatalogGiftsWrappingErrorComposer.Composer());
                    return;
                }

                if (Session.CharacterInfo.CreditsBalance < TotalCreditCost || Session.CharacterInfo.ActivityPointsBalance
                    < TotalApCost)
                {
                    return;
                }

                if (Item.PresetFlags.Length > 0)
                {
                    ItemFlags = Item.PresetFlags;
                }
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

                        default:

                            ItemFlags = string.Empty;
                            break;
                    }
                }

                if (SpriteId == 0) 
                    SpriteId = (int)GeneratePresent().SpriteId;

                string ED = GiftUser + "|" + GiftMessage + "|" + Session.CharacterInfo.Id + "|" + Item.Definition.Id + "|" + SpriteId + "|" + Ribbon + "|" + Colour;
                string ED2 = "!" + GiftMessage;

                Dictionary<int, List<uint>> NewItems = new Dictionary<int, List<uint>>();
                List<Item> GeneratedGenericItems = new List<Item>();
                double ExpireTimestamp = 0;

                if (Item.Definition.Behavior == ItemBehavior.Rental)
                {
                    ExpireTimestamp = UnixTimestamp.GetCurrent() + 3600;
                }

                DataRow Row = MySqlClient.ExecuteQueryRow("SELECT * FROM item_definitions WHERE sprite_id = "+ SpriteId + " LIMIT 1");

                GeneratedGenericItems.Add(ItemFactory.CreateItem(MySqlClient, (uint)Row["id"],
                    (uint)GiftUserRow["id"], ED2, ED2, ExpireTimestamp));

                
                foreach (Item GeneratedItem in GeneratedGenericItems)
                {
                    DataTable Table = MySqlClient.ExecuteQueryTable("INSERT INTO user_gifts(item_id, base_id, amount, extra_data) VALUES('" + GeneratedItem.Id + "', '" + Item.Definition.Id + "', '" + Item.Amount + "', '" + ItemFlags + "')");
                    
                    if (TargetSession != null)
                        TargetSession.InventoryCache.Add(GeneratedItem);

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
                    Session.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, -TotalApCost);
                    Session.SendData(ActivityPointsBalanceComposer.Compose(Session.CharacterInfo.ActivityPointsBalance, -TotalApCost));
                }

                if (TargetSession != null) 
                {
                    foreach (KeyValuePair<int, List<uint>> NewItemData in NewItems)
                    {
                        foreach (uint NewItem in NewItemData.Value)
                        {
                            TargetSession.NewItemsCache.MarkNewItem(MySqlClient, NewItemData.Key, NewItem);
                        }
                    }

                    if (NewItems.Count > 0)
                    {
                        TargetSession.SendData(InventoryNewItemsComposer.Compose(new Dictionary<int, List<uint>>(NewItems)));
                    }

                    TargetSession.SendData(InventoryRefreshComposer.Compose());
                }

                Session.SendData(CatalogPurchaseResultComposer.Compose(Item));

                // Unlock Achievement
                if (Session.CharacterInfo.Username != GiftUser)
                {
                    if (TargetSession != null)
                    {
                        TargetSession.BadgeCache.ReloadCache(MySqlClient, TargetSession.AchievementCache);
                        AchievementManager.ProgressUserAchievement(MySqlClient, TargetSession, "ACH_GiftReceiver", 1);
                    }
                    Session.BadgeCache.ReloadCache(MySqlClient, Session.AchievementCache);
                    AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_GiftGiver", 1);
                }
            }
        }
        #endregion

        #region Request processors
        public static ItemDefinition GeneratePresent()
        {
            int Random = RandomGenerator.GetNext(0, 6);
            switch (Random)
            {
                case 1:
                    return ItemDefinitionManager.GetDefinition(169); // present_gen1

                case 2:
                    return ItemDefinitionManager.GetDefinition(170); // present_gen2

                case 3:
                    return ItemDefinitionManager.GetDefinition(171); // present_gen3

                case 4:
                    return ItemDefinitionManager.GetDefinition(172); // present_gen4

                case 5:
                    return ItemDefinitionManager.GetDefinition(173); // present_gen5

                case 6:
                    return ItemDefinitionManager.GetDefinition(174); // present_gen6

                case 0:
                default:
                    return ItemDefinitionManager.GetDefinition(168); // present_gen
            }
        }
        private static void CheckCanGift(Session Session, ClientMessage Message)
        {
            uint ItemId = Message.PopWiredUInt32();

            CatalogItem Item = CatalogManager.GetCatalogItemByAbsoluteId(ItemId);

            if (Item == null)
            {
                return;
            }

            Session.SendData(CatalogCanGiftComposer.Compose(Item.Id, Item.Definition.AllowGift));
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
                        bool NeedsBasicUnlock = !Session.BadgeCache.ContainsCodeWith(BasicAchievement);
                        bool NeedsVipUnlock = !Session.BadgeCache.ContainsCodeWith(VipAchievement);

                        // Reload the badge cache (reactivating any disabled subscription badges)
                        Session.BadgeCache.ReloadCache(MySqlClient, Session.AchievementCache);

                        // Calculate progress
                        int Progress = (int)Math.Ceiling((double)(Offer.LengthDays / 31));

                        if (Progress <= 0)
                        {
                            Progress = 1;
                        }

                        // Progress VIP achievement
                        if (Offer.Level >= ClubSubscriptionLevel.VipClub)
                        {
                            NeedsVipUnlock = !AchievementManager.ProgressUserAchievement(MySqlClient,
                                Session, VipAchievement, Progress) && NeedsVipUnlock;
                        }
                        else
                        {
                            NeedsVipUnlock = false;
                        }

                        // Progress basic achievement
                        NeedsBasicUnlock = !AchievementManager.ProgressUserAchievement(MySqlClient,
                            Session, BasicAchievement, Progress) && NeedsBasicUnlock; 

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
                                        0, 0));
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
                                        0, 0));
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

                        // Clear catalog cache for user (in case of changes)
                        CatalogManager.ClearCacheGroup(Session.CharacterId);

                        // Send new data to client
                        Session.SendData(CatalogPurchaseResultComposer.Compose(Offer));
                        Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                        Session.SendData(FuseRightsListComposer.Compose(Session));
                        Session.SendData(SubscriptionStatusComposer.Compose(Session.SubscriptionManager, true));
                        //Session.SendData(ClubGiftReadyComposer.Compose(1));
                        break;
                }
            }
        }
        #endregion
    }
}
