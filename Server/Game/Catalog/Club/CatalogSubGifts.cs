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

                    if (!mClubGifts.ContainsKey(GiftId))
                    {
                        mClubGifts.Add(GiftId, new SubscriptionGifts(GiftId, (uint)Row["definition_id"],
                                Row["item_name"].ToString(), Row["preset_flags"].ToString(),
                                (int)Row["days_need"], (Row["isvip"].ToString() == "1")));
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
            ServerMessage Response = (CatalogManager.CacheEnabled ? CatalogManager.CacheController.TryGetResponse(0, Message)
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
                CatalogManager.CacheController.AddIfNeeded(0, Message, Response);
            }

            Session.SendData(Response);
        }
        private static void RedeemClubGift(Session Session, ClientMessage Message)
        {
            string ItemName = Message.PopString();

            SubscriptionGifts SelectedItem = GetGiftByName(ItemName);
            if (SelectedItem == null || SelectedItem.Definition == null)
            {
                return;
            }

            Session.SubscriptionManager.UpdateGiftPoints();

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                ItemDefinition Def = ItemDefinitionManager.GetDefinition(SelectedItem.DefinitionId);

                Dictionary<int, List<uint>> NewItems = new Dictionary<int, List<uint>>();
                List<Item> GeneratedGenericItems = new List<Item>();

                GeneratedGenericItems.Add(ItemFactory.CreateItem(MySqlClient, Def.Id,
                        Session.CharacterInfo.Id, SelectedItem.PresetFlags, SelectedItem.PresetFlags, 0));

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
                Session.SendData(ClubGiftRedeemComposer.Compose(SelectedItem));
            }
        }

        public static bool CanSelectGift(ClubSubscription Subscription, SubscriptionGifts Gift)
        {
            bool CanSelect = false;

            switch(Gift.IsVip)
            {
                case true:
                    
                    if(Subscription.SubscriptionLevel == ClubSubscriptionLevel.VipClub &&
                        Subscription.PastVipTimeInDays >= Gift.DaysNeed)
                    {
                        CanSelect = true;
                    }
                    break;
                    
                case false:

                    int SubTotalDays = Subscription.PastHcTimeInDays + Subscription.PastVipTimeInDays;

                    if (Subscription.SubscriptionLevel >= ClubSubscriptionLevel.BasicClub &&
                        SubTotalDays >= Gift.DaysNeed)
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
    }
}
