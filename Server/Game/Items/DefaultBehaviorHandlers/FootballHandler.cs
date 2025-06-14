using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

using Snowlight.Storage;
using Snowlight.Specialized;

using Snowlight.Game.Bots;
using Snowlight.Game.Misc;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Game.Characters;
using Snowlight.Game.Pathfinding;
using Snowlight.Game.Achievements;

using Snowlight.Communication.Outgoing;
using Snowlight.Game.Rooms.Games;
using Snowlight.Util;
using System.Diagnostics.Eventing.Reader;


namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class FootballHandler
    {
        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.Football, new ItemEventHandler(HandleFootballBall));
        }

        private static bool HandleFootballBall(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch(Event)
            {
                case ItemEventType.Placed:
                case ItemEventType.InstanceLoaded:

                    break;

                case ItemEventType.Removing:

                    break;

                case ItemEventType.UpdateTick:

                    switch(Item.DisplayFlags)
                    {
                        case "16":

                            break;

                        case "15":

                            break;

                        case "14":

                            break;

                        case "13":

                            break;

                        case "12":

                            break;

                        case "11":

                            Item.Flags = "10";
                            Item.DisplayFlags = Item.Flags;

                            RoomManager.MarkWriteback(Item, true);

                            Item.BroadcastStateUpdate(Instance);
                            Instance.RegenerateRelativeHeightmap(true);

                            break;

                        case "10":

                            break;
                    }

                    break;

                case ItemEventType.WalkOnItem:

                    break;
            }

            return true;
        }
    }
}
