using System;
using System.Collections.Generic;

using Snowlight.Storage;
using Snowlight.Game.Misc;
using Snowlight.Game.Rooms;
using Snowlight.Specialized;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class GameCounterHandler
    {
        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.GameCounter, new ItemEventHandler(HandleGameCounter));
        }

        private static bool HandleGameCounter(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            GameManager GamesManager = Instance.GameManager;
            GameType GameItem = (GameType)Item.Definition.BehaviorData;

            switch (Event)
            {
                case ItemEventType.Placed:
                case ItemEventType.InstanceLoaded:

                    if(!GamesManager.GameStarted.ContainsKey(GameItem))
                    {
                        GamesManager.GameStarted.Add(GameItem, false);
                    }

                    break;

                case ItemEventType.Removing:

                    Item.TimmerRunning = false;
                    GamesManager.GameStarted[GameItem] = Item.TimmerRunning;

                    Item.RequestUpdate(0);

                    break;

                case ItemEventType.Interact:

                    int CurrentState;
                    int NewState;

                    Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                    if (!Instance.CheckUserRights(Session))
                    {
                        return true;
                    }

                    switch(RequestData)
                    {
                        case 0: // Interaction called by Wired
                        case 1: // Interaction called by User

                            Item.TimmerRunning = !Item.TimmerRunning;
                            Item.RequestUpdate(0);

                            break;

                        case 2: // Add Time

                            int.TryParse(Item.Flags, out CurrentState);

                            NewState = CurrentState + 30;

                            if (CurrentState < 0 || CurrentState >= 720)
                            {
                                NewState = 0;
                            }

                            if (CurrentState != NewState)
                            {
                                Item.Flags = NewState.ToString();
                                Item.DisplayFlags = Item.Flags;

                                RoomManager.MarkWriteback(Item, true);

                                Item.BroadcastStateUpdate(Instance);
                            }

                            break;
                    }

                    break;

                case ItemEventType.UpdateTick:

                    int.TryParse(Item.Flags, out int CurrentTime);

                    if (GameItem == GameType.Freeze)
                    {
                        GamesManager.CicleFreezeGameActors(Item.TimmerRunning);
                    }

                    if (Item.TimmerRunning)
                    {
                        if (CurrentTime > 0)
                        {
                            if (!GamesManager.GameStarted[GameItem])
                            {
                                GamesManager.GameStarted[GameItem] = Item.TimmerRunning;
                                GamesManager.StartGame(GameItem);
                            }

                            CurrentTime--;
                            Item.RequestUpdate(2);
                        }
                        else
                        {
                            CurrentTime = 0;
                            Item.TimmerRunning = false;

                            if (GamesManager.GameStarted[GameItem])
                            {
                                GamesManager.StopGame(GameItem);
                                GamesManager.GameStarted[GameItem] = Item.TimmerRunning;
                                Item.RequestUpdate(1);
                            }
                        }
                    }
                    else
                    {
                        if (GamesManager.GameStarted[GameItem])
                        {
                            GamesManager.GameStarted[GameItem] = Item.TimmerRunning;
                        }
                    }

                    if (CurrentTime >= 0)
                    {
                        Item.Flags = CurrentTime.ToString();
                        Item.DisplayFlags = Item.Flags;

                        RoomManager.MarkWriteback(Item, true);

                        Item.BroadcastStateUpdate(Instance);
                    }

                    break;
            }

            return true;
        }
    }
}
