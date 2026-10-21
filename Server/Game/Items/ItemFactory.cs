using Snowlight.Communication.Outgoing;
using Snowlight.Game.Achievements;
using Snowlight.Game.Items.Wired;
using Snowlight.Game.Misc;
using Snowlight.Game.Sessions;
using Snowlight.Specialized;
using Snowlight.Storage;
using System;
using System.Collections.Generic;
using System.Data;

namespace Snowlight.Game.Items
{
    public static class ItemFactory
    {
        public static Item CreateItem(SqlDatabaseClient MySqlClient, uint DefinitionId, uint UserId, string Flags, string FlagsDisplay, double ExpireTimestamp, bool Untradable = false, WiredManager WiredManager = null)
        {
            MySqlClient.SetParameter("definitionid", DefinitionId);
            MySqlClient.SetParameter("userid", UserId);
            MySqlClient.SetParameter("flags", Flags);
            MySqlClient.SetParameter("flagsd", FlagsDisplay);
            MySqlClient.SetParameter("untradable", Untradable ? "1" : "0");
            MySqlClient.SetParameter("expiretimestamp", ExpireTimestamp);

            string RawId = MySqlClient.ExecuteScalar("INSERT INTO items (definition_id,user_id,flags,flags_display,untradable,expire_timestamp) VALUES (@definitionid,@userid,@flags,@flagsd,@untradable,@expiretimestamp); SELECT LAST_INSERT_ID();").ToString();

            uint.TryParse(RawId, out uint Id);

            if (Id == 0)
            {
                return null;
            }

            return new Item(Id, DefinitionId, UserId, 0, new Vector3(), string.Empty, 0, Flags, Flags, Untradable, 0, 0,
                ExpireTimestamp, false, WiredManager);         
        }

        public static Item CreateFromDatabaseRow(DataRow Row , WiredManager WiredManager = null)
        {
            return new Item((uint)Row["id"], (uint)Row["definition_id"], (uint)Row["user_id"], (uint)Row["room_id"],
                Vector3.FromString((string)Row["room_pos"]), (string)Row["room_wall_pos"], (int)Row["room_rot"],
                (string)Row["flags"], (string)Row["flags_display"], (Row["untradable"].ToString() == "1"),
                (uint)Row["soundmanager_id"], (int)Row["soundmanager_order"], (double)Row["expire_timestamp"],
                (Row["marketplace_listed"].ToString() == "1"), WiredManager);
        }

        public static void GenerateUserItems(SqlDatabaseClient MySqlClient, Session Session, ItemDefinition Definition,
            string Flags, double ExpireTimestamp, bool IgnoreAchievement = false, int Amount = 1)
        {
            if (Definition.Type == ItemType.ClubSubscription)
            {
                return;
            }

            List<Item> GeneratedGenericItems = new List<Item>();

            for (int a = 0; a < Amount; a++)
            {
                GeneratedGenericItems.Add(CreateItem(MySqlClient, Definition.Id,
                    Session.CharacterInfo.Id, Flags, Flags, ExpireTimestamp));
            }

            switch (Definition.Behavior)
            {
                case ItemBehavior.Teleporter:

                    Item LinkedItem = CreateItem(MySqlClient, Definition.Id,
                        Session.CharacterInfo.Id, GeneratedGenericItems[0].Id.ToString(), string.Empty,
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

                Session.NewItemsCache.MarkNewItem(MySqlClient, (NewItemsCategory)TabId, GeneratedItem.Id);

                if (!IgnoreAchievement)
                {
                    #region Delivery Achievement
                    ItemAchievements Achievement = ItemDefinitionManager.GetAchievement(ActionToReward.Buying, GeneratedItem.DefinitionId);

                    if (Achievement != null)
                    {
                        AchievementManager.ProgressUserAchievement(MySqlClient, Session, Achievement.AchievementCode, GeneratedGenericItems.Count);
                    }
                    #endregion
                }
            }

            Session.NewItemsCache.SendNewItems(Session);
        }
    }
}
