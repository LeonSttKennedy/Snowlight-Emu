using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Items;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    class RedeemCoinsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "club_vip"; }
        }

        public string Parameters
        {
            get { return "<yes>"; }
        }

        public string Description 
        {
            get { return "Turns all exchange items in your hand back into coins."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Instance.TradeManager.UserHasActiveTrade(Session.CharacterId))
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_redeemcoins_trade_disable"), 4, ChatType.Whisper));
                return;
            }

            if (Params.Length == 1)
            {
                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_redeemcoins_info")));
            }
            else
            {
                if (Params.Length == 2 && Params[1].ToString() == "yes")
                {
                    using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                    {
                        int CoinsAmount = 0;

                        foreach (Item Items in Session.InventoryCache.Items.Values)
                        {
                            if (Items.Definition.Behavior == ItemBehavior.ExchangeItem && Items.RoomId == 0)
                            {
                                CoinsAmount += Items.Definition.BehaviorData;
                                Session.InventoryCache.RemoveItem(Items.Id);
                                Items.RemovePermanently(MySqlClient);
                            }
                        }

                        Session.InventoryCache.ReloadCache(MySqlClient);
                        Session.SendData(InventoryRefreshComposer.Compose());

                        if (CoinsAmount > 0)
                        {
                            Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, CoinsAmount);
                            Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_redeemcoins_success", CoinsAmount.ToString()), 0, ChatType.Whisper));
                        }
                        else
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_redeemcoins_error"), 4, ChatType.Whisper));
                        }
                    }
                }
                else if (Params.Length == 2 && Params[1].ToString() != "yes")
                {
                    Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_redeemcoins_confirm"), 0, ChatType.Whisper));
                }
            }
        }
    }
}
