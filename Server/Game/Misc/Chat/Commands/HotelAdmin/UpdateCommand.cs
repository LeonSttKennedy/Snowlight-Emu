using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Items;
using Snowlight.Game.Rooms;
using Snowlight.Game.Catalog;
using Snowlight.Game.Sessions;
using Snowlight.Game.Navigation;
using Snowlight.Game.Achievements;
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
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_invalid_syntax") + " - :update <a thing to update>", 0, ChatType.Whisper));
                return;
            }

            string ToSend = string.Empty;
            string ToUpdate = Params[1];
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                switch (ToUpdate.ToLower())
                {
                    #region Development update commands
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

                    case "achievements":
                        {
                            ToSend = ExternalTexts.GetValue("command_update_achievements");
                            AchievementManager.ReloadAchievements(MySqlClient);
                            goto End;
                        }

                    case "catalog":
                        {
                            ToSend = ExternalTexts.GetValue("command_update_catalog");
                            CatalogManager.RefreshCatalogData(MySqlClient);
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

                    case "serversettings":
                        {
                            ToSend = ExternalTexts.GetValue("command_update_serversettings");
                            ServerSettings.Initialize(MySqlClient);
                            goto End;
                        }

                    case "texts":
                        {
                            ToSend = ExternalTexts.GetValue("command_update_texts");
                            ExternalTexts.Initialize(MySqlClient);
                            goto End;
                        }

                    default:
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_update_error", ToUpdate), 0, ChatType.Whisper));
                            return;
                        }
                }
            }

            End:
            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_update_success", ToSend), 0, ChatType.Whisper));
        }
    }
}
