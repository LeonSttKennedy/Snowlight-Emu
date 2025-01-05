using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Rooms;
using Snowlight.Game.Items;
using Snowlight.Game.Rights;
using Snowlight.Game.Catalog;
using Snowlight.Game.Sessions;
using Snowlight.Communication;
using Snowlight.Communication.Outgoing;
using Snowlight.Communication.Incoming;
using System.Web.UI;

namespace Snowlight.Game.Catalog
{
    public class CatalogSubGifts
    {
        private static object mSyncRoot;

        private static Dictionary<uint, SubscriptionGifts> mClubGifts;

        public static void Initialize(SqlDatabaseClient MySqlClient)
        {
            mSyncRoot = new object();
            mClubGifts = new Dictionary<uint, SubscriptionGifts>();

            ReloadClubGifts(MySqlClient);

            DataRouter.RegisterHandler(OpcodesIn.CATALOG_CLUB_GIFTS, new ProcessRequestCallback(GetClubGifts));
            DataRouter.RegisterHandler(OpcodesIn.SELECT_CLUB_GIFT, new ProcessRequestCallback(RedeemClubGift));
        }
        public static void ReloadClubGifts(SqlDatabaseClient MySqlClient)
        {
            mClubGifts.Clear();

            int CountLoaded = 0;
            lock (mSyncRoot)
            {
                MySqlClient.SetParameter("enabled", "1");
                DataTable GiftsTable = MySqlClient.ExecuteQueryTable("SELECT * FROM catalog_subscriptions_gifts WHERE enabled = @enabled");

                foreach (DataRow Row in GiftsTable.Rows)
                {
                    uint GiftId = (uint)Row["id"];
                    int ParentId = (int)Row["parent_id"];

                    SubscriptionGifts Gift = new SubscriptionGifts(GiftId, (int)Row["definition_id"],
                                Row["item_name"].ToString(), Row["preset_flags"].ToString(), (int)Row["amount"],
                                (int)Row["days_need"], (Row["isvip"].ToString() == "1"), (Row["one_time_redeem"].ToString() == "1"));

                    if (ParentId == -1)
                    {
                        if (!mClubGifts.ContainsKey(GiftId))
                        {
                            mClubGifts.Add(GiftId, Gift);
                        }
                    }
                    else
                    {
                        uint _ParentId = uint.Parse(ParentId.ToString());
                        SubscriptionGifts ParentItem = GetSubscriptionGiftByAbsoluteId(_ParentId);
                        ParentItem.AddItem(Gift);
                    }

                    CountLoaded++;
                }
            }

            Output.WriteLine("Loaded " + CountLoaded + " club gift(s)", OutputLevel.DebugInformation);
        }
        private static SubscriptionGifts GetGiftByName(string Name)
        {
            foreach (SubscriptionGifts Gifts in mClubGifts.Values)
            {
                if (Gifts.ItemName.ToLower() == Name.ToLower())
                {
                    return Gifts;
                }
            }

            return null;
        }
        private static void GetClubGifts(Session Session, ClientMessage Message)
        {
            ServerMessage Response = (CatalogManager.CacheEnabled ? CatalogManager.CacheController.TryGetResponse(Session.CharacterId, Message)
                : null);

            if (Response != null)
            {
                Session.SendData(Response);
            }

            lock (mClubGifts)
            {
                Response = ClubGiftListComposer.Compose(Session.SubscriptionManager, mClubGifts.Values.ToList());
            }

            if (CatalogManager.CacheEnabled)
            {
                CatalogManager.CacheController.AddIfNeeded(Session.CharacterId, Message, Response);
            }

            Session.SendData(Response);
        }
        private static void RedeemClubGift(Session Session, ClientMessage Message)
        {
            string ItemName = Message.PopString();

            SubscriptionGifts SelectedItem = GetGiftByName(ItemName);
            if (SelectedItem == null || SelectedItem.DefinitionId > 0 && SelectedItem.Definition == null)
            {
                return;
            }

            Session.SubscriptionManager.AddGiftPoints();

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                #region Arrange To Delivery
                List<SubscriptionGifts> ItemsToDelivery = new List<SubscriptionGifts>();

                if (SelectedItem.IsDeal)
                {
                    ItemsToDelivery.AddRange(SelectedItem.DealItems);
                }
                else
                {
                    ItemsToDelivery.Add(SelectedItem);
                }
                #endregion

                foreach (SubscriptionGifts GiftToDelivery in ItemsToDelivery)
                {
                    ItemDefinition Def = ItemDefinitionManager.GetDefinition(GiftToDelivery.Definition.Id);

                    Dictionary<int, List<uint>> NewItems = new Dictionary<int, List<uint>>();
                    List<Item> GeneratedGenericItems = new List<Item>();

                    for (int i = 0; i < GiftToDelivery.Amount; i++)
                    {
                        GeneratedGenericItems.Add(ItemFactory.CreateItem(MySqlClient, Def.Id,
                                Session.CharacterInfo.Id, GiftToDelivery.PresetFlags, GiftToDelivery.PresetFlags, 0));

                        switch (Def.Behavior)
                        {
                            case ItemBehavior.Teleporter:

                                Item LinkedItem = ItemFactory.CreateItem(MySqlClient, Def.Id,
                                    Session.CharacterId, GeneratedGenericItems[0].Id.ToString(), string.Empty,
                                    0);

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
                    }

                    if (SelectedItem.OneTimeRedeem)
                    {
                        Session.SubscriptionManager.AddGiftRedeem(MySqlClient, SelectedItem.Id);
                        GetClubGifts(Session, new ClientMessage(OpcodesIn.CATALOG_CLUB_GIFTS));
                    }

                    Session.SendData(ClubGiftRedeemComposer.Compose(GiftToDelivery));
                }
            }
        }

        public static bool CanSelectGift(ClubSubscription Subscription, SubscriptionGifts Gift)
        {
            bool CanSelect = false;

            bool WasGiftRedeemed = Gift.OneTimeRedeem && Subscription.OneTimeGiftsRedeem.Contains(Gift.Id);

            switch (Gift.IsVip)
            {
                case true:
                    
                    if (Subscription.SubscriptionLevel == ClubSubscriptionLevel.VipClub &&
                        Subscription.PastVipTimeInDays >= Gift.DaysNeed &&
                        !WasGiftRedeemed)
                    {
                        CanSelect = true;
                    }
                    break;
                    
                case false:

                    int SubTotalDays = Subscription.PastHcTimeInDays + Subscription.PastVipTimeInDays;

                    if (Subscription.SubscriptionLevel >= ClubSubscriptionLevel.BasicClub &&
                        SubTotalDays >= Gift.DaysNeed && !WasGiftRedeemed)
                    {
                        CanSelect = true;
                    }
                    break;
                
                default:

                    CanSelect = false;
                    break;
            }

            return CanSelect;
        }

        public static SubscriptionGifts GetSubscriptionGiftByAbsoluteId(uint ItemId)
        {
            lock (mClubGifts)
            {
                if (mClubGifts.ContainsKey(ItemId))
                {
                    return mClubGifts[ItemId];
                }
            }

            return null;
        }
    }
}
