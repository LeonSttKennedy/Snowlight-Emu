using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Pets;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    class EmptyCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "club_vip"; }
        }

        public string Parameters
        {
            get { return "<tab> <yes>"; }
        }

        public string Description
        {
            get { return "Emptys an tab from your inventory."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Params.Length <= 2)
            {
                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_empty_info")));
                return;
            }
            else
            {
                string InventoryTab = UserInputFilter.FilterString(Params[1].Trim());
                if (Params.Length == 3 && Params[2].ToString().ToLower() == "yes")
                {
                    switch (InventoryTab.ToLower())
                    {
                        case "inv":
                            {
                                Session.InventoryCache.ClearAndDeleteAllItems();
                                Session.SendData(InventoryRefreshComposer.Compose());
                                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_emptyinv_success")));
                                return;
                            }

                        case "pets":
                            {
                                foreach (Pet Pet in Session.InventoryCache.Pets.Values)
                                {
                                    Session.SendData(InventoryPetRemovedComposer.Compose(Pet.Id));
                                }

                                Session.InventoryCache.ClearAndDeleteAllPets();
                                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_emptypets_success")));
                                return;
                            }

                        default:
                            {
                                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_empty_tab_error", InventoryTab), 0, ChatType.Whisper));
                                return;
                            }
                    }
                }
                else if (Params.Length == 3 && Params[2].ToString().ToLower() != "yes")
                {
                    if (InventoryTab != "pets" && InventoryTab != "inv")
                    {
                        Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_empty_tab_error", InventoryTab), 0, ChatType.Whisper));
                        return;
                    }

                    Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_empty" + InventoryTab + "_confirm"), 0, ChatType.Whisper));
                }
            }
        }
    }
}
