using System;

using Snowlight.Game.Sessions;
using Snowlight.Game.Rooms;
using Snowlight.Communication.Outgoing;
using Snowlight.Storage;

namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class ExchangeHandler
    {
        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.ExchangeItem, new ItemEventHandler(HandleExchangeRedemption));
        }

        private static bool HandleExchangeRedemption(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Interact:

                    int.TryParse(Item.Flags, out int ItemValue);

                    using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                    {
                        if (ItemValue != 0)
                        {
                            Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                            Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, ItemValue);
                            Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                        }

                        Item.RemovePermanently(MySqlClient);
                    }

                    Instance.TakeItem(Item.Id);
                    Instance.RegenerateRelativeHeightmap();
                    break;
            }

            return true;
        }
    }
}
