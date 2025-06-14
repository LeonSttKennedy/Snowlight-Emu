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
    public static class GameScoreboardHandler
    {
        public static void Register()
        {
            /* This needs to be implemented
             * 
             * ItemEventDispatcher.RegisterEventHandler(ItemBehavior.BattleBallScore, new ItemEventHandler(HandleGameScoreboard));
             * ItemEventDispatcher.RegisterEventHandler(ItemBehavior.FreezeScore, new ItemEventHandler(HandleGameScoreboard));
             */

            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.FootballScore, new ItemEventHandler(HandleGameScoreboard));
        }

        private static bool HandleGameScoreboard(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Interact:
                case ItemEventType.ItemInvoking:

                    if (Event == ItemEventType.Interact)
                    {
                        Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                        if (!Instance.CheckUserRights(Session))
                        {
                            break;
                        }

                        if (!Distance.TilesTouching(Actor.Position.GetVector2(), Item.RoomPosition.GetVector2()))
                        {
                            Actor.MoveToItemAndInteract(Item, RequestData);
                            break;
                        }
                    }

                    int.TryParse(Item.DisplayFlags, out int Val);

                    if (RequestData == 1)
                    {
                        Val--;

                        if (Val < 0)
                        {
                            Val = 99;
                        }
                    }
                    else if (RequestData == 2)
                    {
                        Val++;

                        if (Val > 99)
                        {
                            Val = 0;
                        }
                    }
                    else
                    {
                        Val = (Val == -1 ? 0 : -1);
                    }

                    Item.DisplayFlags = Val.ToString();
                    Item.BroadcastStateUpdate(Instance);
                    break;
            }

            return true;
        }
    }
}
