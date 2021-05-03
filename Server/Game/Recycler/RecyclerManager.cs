using System;
using Snowlight.Storage;
using System.Collections.Generic;
using System.Data;

using System.Threading;
using Snowlight.Game.Sessions;
using Snowlight.Communication;
using Snowlight.Game.Catalog;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Rooms;
using Snowlight.Game.Items;
using Snowlight.Communication.Incoming;
using Snowlight.Util;

namespace Snowlight.Game.Recycler
{
    public static class RecyclerManager
    {
        private static Dictionary<int, List<uint>> mRewards;
        private static bool mEnabled;
        private static object mSyncRoot;

        public static void Initialize(SqlDatabaseClient MySqlClient)
        {
            mRewards = new Dictionary<int, List<uint>>();
            mSyncRoot = new object();

            ReloadRewards(MySqlClient);

            DataRouter.RegisterHandler(OpcodesIn.CATALOG_GET_RECYCLER_REWARDS, new ProcessRequestCallback(GetRewardsList));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_GET_RECYCLER_CONFIG, new ProcessRequestCallback(GetConfig));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_RECYCLE_ITEMS, new ProcessRequestCallback(RecycleItems));
        }

        public static void ReloadRewards(SqlDatabaseClient MySqlClient)
        {
            lock (mSyncRoot)
            {
                DataTable Table = MySqlClient.ExecuteQueryTable("SELECT * FROM recycler_rewards");

                int UniqueLevels = 0;

                foreach (DataRow Row in Table.Rows)
                {
                    int RewardLevel = (int)Row["chance_level"];

                    if (!mRewards.ContainsKey(RewardLevel))
                    {
                        mRewards.Add(RewardLevel, new List<uint>());
                        UniqueLevels++;
                    }

                    mRewards[RewardLevel].Add((uint)Row["item_id"]);
                }

                if (UniqueLevels != 5)
                {
                    Output.WriteLine("Recycler is not configured correctly and will *not* work properly. Please ensure there are 5 unique reward levels.", OutputLevel.Warning);
                    mEnabled = false;
                }
                else
                {
                    mEnabled = true;
                }
            }
        }

        public static uint GetRandomReward()
        {
            lock (mSyncRoot)
            {
                int Level = 1;

                if (RandomGenerator.GetNext(1, 2000) == 2000)
                {
                    Level = 5;
                }
                else if (RandomGenerator.GetNext(1, 200) == 200)
                {
                    Level = 4;
                }
                else if (RandomGenerator.GetNext(1, 40) == 40)
                {
                    Level = 3;
                }
                else if (RandomGenerator.GetNext(1, 4) == 4)
                {
                    Level = 2;
                }

                if (!mRewards.ContainsKey(Level))
                {
                    return 0;
                }

                return mRewards[Level][RandomGenerator.GetNext(0, (mRewards[Level].Count - 1))];
            }
        }

        private static void GetRewardsList(Session Session, ClientMessage Message)
        {
            ServerMessage Response = (CatalogManager.CacheEnabled ? CatalogManager.CacheController.TryGetResponse(0, Message)
                : null);

            if (Response != null)
            {
                Session.SendData(Response);
            }

            lock (mSyncRoot)
            {
                Response = CatalogRecyclerRewardsComposer.Compose(mRewards);
            }

            if (CatalogManager.CacheEnabled)
            {
                CatalogManager.CacheController.AddIfNeeded(0, Message, Response);
            }

            Session.SendData(Response);
        }

        private static void GetConfig(Session Session, ClientMessage Message)
        {
            Session.SendData(CatalogRecyclerConfigComposer.Compose(mEnabled, false));
        }

        private static void RecycleItems(Session Session, ClientMessage Message)
        {
            if (!mEnabled)
            {
                Session.SendData(NotificationMessageComposer.Compose("The recycler is temporarily disabled. Please check back later."));
                return;
            }

            int Amount = Message.PopWiredInt32();

            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || Amount != 5)
            {
                return;
            }

            ItemDefinition Reward = ItemDefinitionManager.GetDefinition(GetRandomReward());

            if (Reward == null)
            {
                return;
            }

            List<Item> ItemsToRecycle = new List<Item>();

            for (int i = 0; i < 5; i++)
            {
                Item Item = Session.InventoryCache.GetItem(Message.PopWiredUInt32());

                if (Item == null || !Item.Definition.AllowRecycle || ItemsToRecycle.Contains(Item))
                {
                    return;
                }

                ItemsToRecycle.Add(Item);
            }

            Dictionary<int, List<uint>> NewItems = new Dictionary<int, List<uint>>();
            List<Item> GeneratedGenericItems = new List<Item>();

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                foreach (Item Item in ItemsToRecycle)
                {
                    Session.InventoryCache.RemoveItem(Item.Id);
                    Item.RemovePermanently(MySqlClient);

                    Session.SendData(InventoryItemRemovedComposer.Compose(Item.Id));
                }

                GeneratedGenericItems.Add(ItemFactory.CreateItem(MySqlClient, 1506, Session.CharacterId, 
                    string.Empty, string.Empty, 0));

                foreach (Item GeneratedItem in GeneratedGenericItems)
                {
                    Session.InventoryCache.Add(GeneratedItem);

                    int TabId = GeneratedItem.Definition.Type == ItemType.FloorItem ? 1 : 2;

                    if (!NewItems.ContainsKey(TabId))
                    {
                        NewItems.Add(TabId, new List<uint>());
                    }

                    NewItems[TabId].Add(GeneratedItem.Id);
                    Session.SendData(RecyclerResultComposer.Compose(true, GeneratedItem.Id));
                    DataTable Table = MySqlClient.ExecuteQueryTable("INSERT INTO user_gifts(item_id, base_id, amount, extra_data) VALUES('" + GeneratedItem.Id + "', '" + Reward.Id + "', '1', '')");
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
        }
    }
}
