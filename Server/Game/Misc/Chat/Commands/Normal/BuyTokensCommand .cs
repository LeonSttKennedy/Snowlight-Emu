using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Pets;
using Snowlight.Game.Rooms;
using Snowlight.Game.Catalog;
using Snowlight.Game.Rights;
using Snowlight.Game.Sessions;
using Snowlight.Communication;
using Snowlight.Game.Advertisements;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Items;

namespace Snowlight.Game.Misc
{
    class BuyTokensCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return ""; }
        }

        public string Parameters
        {
            get { return "<help>"; }
        }

        public string Description
        {
            get { return "Buy multiple marketplace tokens at once."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Params.Length < 2)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_invalid_syntax") + " - :buytokens <help or quantity>", 0, ChatType.Whisper));
                return;
            }

            string Type = UserInputFilter.FilterString(Params[1].Trim());

            switch (Type.ToLower())
            {
                case "help":

                    Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_buytokens_help")));
                    break;

                default:

                    if (int.TryParse(Params[1], out int Amount))
                    {
                        int TokensPrice = ServerSettings.MarketplaceTokensPrice;
                        int TokensAmount = Session.HasRight("club_vip") ? ServerSettings.MarketplacePremiumTokens : ServerSettings.MarketplaceNormalTokens;

                        int Price = Amount * TokensPrice;
                        int Quantity = Amount * TokensAmount;

                        /*
                         * Just a limit to buy at once
                         */

                        if (Quantity > 100)
                        {
                            Quantity = 100;
                            Price = Quantity / TokensAmount;
                        }

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, -Price);
                            Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                            Session.CharacterInfo.UpdateMarketplaceTokens(MySqlClient, Quantity);
                        }

                        Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_buytokens_success", Quantity.ToString()), 0, ChatType.Whisper));
                    }
                    else
                    {
                        Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_buytokens_error"), 0, ChatType.Whisper));
                    }

                    break;
            }
        }
    }
}
