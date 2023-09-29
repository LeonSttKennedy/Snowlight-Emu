using System;

using Snowlight.Storage;
using Snowlight.Game.Rooms;
using Snowlight.Communication;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class WelcomeGiftHandler
    {
        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.WelcomeGift, new ItemEventHandler(HandleWelcomeGift));
        }

        private static bool HandleWelcomeGift(Session Session, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Interact:

                    Session.SendData(WelcomeGiftComposer.Compose("defaultuser@meth0d.org", Item));
                    break;
            }

            return true;
        }
    }
}
