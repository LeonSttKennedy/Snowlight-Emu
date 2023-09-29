using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Pets;
using Snowlight.Game.Items;
using Snowlight.Game.Rooms;
using Snowlight.Game.Catalog;
using Snowlight.Game.Sessions;
using Snowlight.Game.Navigation;
using Snowlight.Game.Achievements;
using Snowlight.Game.Advertisements;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    class UpdateCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "hotel_admin"; }
        }

        public string Parameters
        {
            get { return "<variable>"; }
        }

        public string Description
        {
            get { return "Reloads a specific part of the hotel."; }
        }
        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_invalid_syntax") + " - :update <help>", 0, ChatType.Whisper));
                return;
            }

            string ToSend = string.Empty;
            string ToUpdate = Params[1];
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                switch (ToUpdate.ToLower())
                {
                    #region Development update commands

                    #region item_defs
                    case "item_defs":
                        {
                            DataTable Table = MySqlClient.ExecuteQueryTable("SELECT * FROM catalog");
                            int i = 1;
                            foreach (DataRow Row in Table.Rows)
                            {
                                if ((int)Row["parent_id"] == 27 && (int)Row["id"] != 33)
                                {
                                    MySqlClient.SetParameter("id", (int)Row["id"]);
                                    DataTable TableItems = MySqlClient.ExecuteQueryTable("SELECT * FROM catalog_items WHERE page_id = @id");
                                    foreach(DataRow RowItems in TableItems.Rows)
                                    {
                                        MySqlClient.SetParameter("def_id", uint.Parse(RowItems["item_ids"].ToString()));
                                        MySqlClient.SetParameter("allow_sell", "0");
                                        MySqlClient.ExecuteNonQuery("UPDATE item_definitions SET allow_gifting = @allow_sell WHERE id = @def_id LIMIT 1");
                                    }
                                    i += 1;
                                }
                            }
                            Console.WriteLine(i + " Items defs updated!");
                            return;
                        }
                    #endregion

                    #region hotel economy
                    case "hotel_economy":
                        {
                            // parent_id = 2 ( Pixels furniture catalog tab )
                            DataTable Table = MySqlClient.ExecuteQueryTable("SELECT * FROM catalog");
                            int i = 0;
                            foreach (DataRow Row in Table.Rows)
                            {
                                if ((int)Row["parent_id"] == 2)
                                {
                                    MySqlClient.SetParameter("pageid", (int)Row["id"]);
                                    DataTable TTable = MySqlClient.ExecuteQueryTable("SELECT * FROM catalog_items WHERE page_id = @pageid");
                                    foreach (DataRow RRow in TTable.Rows)
                                    {
                                        if ((int)RRow["cost_credits"] == 0 && (int)RRow["cost_activitypoints"] >= 1)
                                        {
                                            MySqlClient.SetParameter("itemid", (uint)RRow["id"]);
                                            MySqlClient.SetParameter("points", (int)RRow["cost_activitypoints"]);
                                            MySqlClient.ExecuteNonQuery("UPDATE catalog_items SET cost_credits = @points WHERE id = @itemid LIMIT 1");
                                            
                                            MySqlClient.SetParameter("itemid", (uint)RRow["id"]);
                                            MySqlClient.ExecuteNonQuery("UPDATE catalog_items SET cost_activitypoints = '0' WHERE id = @itemid LIMIT 1");

                                            Console.Write("Updated ITEM ID: " + (uint)RRow["id"]);
                                            i += 1;
                                        }
                                    }
                                }
                            }
                            Console.WriteLine(i + " catalog items updated");
                            return;
                        }
                    #endregion

                    #region catalog items
                    case "catalog_items":
                        {
                            DataTable Table = MySqlClient.ExecuteQueryTable("SELECT * FROM catalog_items");
                            int i = 0;
                            foreach (DataRow Row in Table.Rows)
                            {
                                MySqlClient.SetParameter("id", (uint)Row["id"]);
                                MySqlClient.SetParameter("base", (uint)Row["base_id"]);
                                MySqlClient.SetParameter("quantity", (int)Row["amount"]);
                                MySqlClient.ExecuteNonQuery("UPDATE catalog_items SET item_ids = @base, amounts = @quantity WHERE id = @id");
                                i += 1;
                            }
                            Console.WriteLine(i + " catalog items updated!");
                            return;
                        }
                    #endregion

                    #region catalog order
                    case "catalog_order":
                        {
                            // parent_id = 2 ( Pixels furniture catalog tab )
                            DataTable Table = MySqlClient.ExecuteQueryTable("SELECT * FROM catalog ORDER BY title ASC");
                            int i = 1;
                            foreach (DataRow Row in Table.Rows)
                            {
                                if ((int)Row["parent_id"] == 2)
                                {
                                    MySqlClient.SetParameter("order", i);
                                    MySqlClient.SetParameter("id", (int)Row["id"]);
                                    MySqlClient.ExecuteNonQuery("UPDATE catalog SET order_num = @order WHERE id = @id");
                                    i += 1;
                                }
                            }
                            Session.SendData(NotificationMessageComposer.Compose("Catalog order updated!"));
                            return;
                        }
                    #endregion

                    #endregion

                    case "achievements":
                        {
                            ToSend = ExternalTexts.GetValue("command_update_achievements");
                            AchievementManager.ReloadAchievements(MySqlClient);
                            goto End;
                        }

                    case "advertisement":
                        {
                            ToSend = ExternalTexts.GetValue("command_update_advertisement");
                            InterstitialManager.Initialize(MySqlClient);
                            goto End;
                        }

                    case "catalog":
                        {
                            ToSend = ExternalTexts.GetValue("command_update_catalog");
                            CatalogManager.RefreshCatalogData(MySqlClient);
                            CatalogSubGifts.ReloadClubGifts(MySqlClient);
                            goto End;
                        }

                    case "filter":
                        {
                            ToSend = ExternalTexts.GetValue("command_update_filter");
                            ChatWordFilter.Initialize(MySqlClient);
                            goto End;
                        }

                    case "items":
                        {
                            ToSend = ExternalTexts.GetValue("command_update_items");
                            ItemDefinitionManager.Initialize(MySqlClient);
                            goto End;
                        }

                    case "navigator":
                        {
                            ToSend = ExternalTexts.GetValue("command_update_navigator");
                            Navigator.ReloadFlatCategories(MySqlClient);
                            Navigator.ReloadOfficialItems(MySqlClient);
                            goto End;
                        }

                    case "pets":
                        {
                            ToSend = ExternalTexts.GetValue("command_update_pets");
                            PetDataManager.ReloadData(MySqlClient);
                            goto End;
                        }

                    case "polls":
                        {
                            ToSend = ExternalTexts.GetValue("command_update_roompolls");
                            RoomPollManager.Initialize(MySqlClient);
                            goto End;
                        }

                    case "settings":
                        {
                            ToSend = ExternalTexts.GetValue("command_update_serversettings");
                            ServerSettings.Initialize(MySqlClient);
                            goto End;
                        }

                    case "suboffers":
                        {
                            ToSend = ExternalTexts.GetValue("command_update_subscriptionoffers");
                            SubscriptionOfferManager.ReloadSubscriptionOffers(MySqlClient);
                            goto End;
                        }

                    case "texts":
                        {
                            ToSend = ExternalTexts.GetValue("command_update_texts");
                            ExternalTexts.Initialize(MySqlClient);
                            goto End;
                        }

                    case "help":
                    default:
                        {
                            Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_update_help")));
                            return;
                        }
                }
            }

            End:
            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_update_success", ToSend), 0, ChatType.Whisper));
        }
    }
}
