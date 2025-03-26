using System;
using System.Collections.Generic;

using Snowlight.Game.Sessions;
using Snowlight.Game.Rooms;
using Snowlight.Game.Items.DefaultBehaviorHandlers;
using Snowlight.Game.Achievements;

namespace Snowlight.Game.Items
{
    public enum ItemEventType
    {
        Placed = 0,
        Moved = 1,
        Removing = 2,
        Interact = 3,
        UpdateTick = 4,
        InstanceLoaded = 5,
        WalkOnItem = 6,
        WalkOffItem = 7
    }

    public delegate bool ItemEventHandler(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Type, int RequestData);

    public static class ItemEventDispatcher
    {
        private static object mSyncRoot;
        private static Dictionary<ItemBehavior, List<ItemEventHandler>> mEventHandlers;

        public static void Initialize()
        {
            mSyncRoot = new object();
            mEventHandlers = new Dictionary<ItemBehavior, List<ItemEventHandler>>();

            RegisterDefaultEventHandlers();
        }

        public static void RegisterDefaultEventHandlers()
        {
            GenericHandler.Register();
            GateHandler.Register();
            ExchangeHandler.Register();
            TimedFurnitureHandler.Register();
            RandomizerHandler.Register();
            TeleporterHandler.Register();
            DispenserItemHandler.Register();
            FireworksHandler.Register();
            JukeboxHandler.Register();
            RollerHandler.Register();

            PetItemsHandler.Register();
            GameCounterHandler.Register();
            PuzzleboxHandler.Register();
            WelcomeGiftHandler.Register();
            WiredHandler.Register();
            TotemHandler.Register();
            RollerRinkHandler.Register();
            SkateRailHandler.Register();
            IceSkatingHandler.Register();
            BunnyRunHandler.Register();
        }

        public static void InvokeItemEventHandler(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Type, int RequestData = 0)
        {
            lock (mSyncRoot)
            {
                if (mEventHandlers.ContainsKey(Item.Definition.Behavior))
                {
                    foreach (ItemEventHandler EventHandler in mEventHandlers[Item.Definition.Behavior])
                    {
                        if (!EventHandler.Invoke(Actor, Item, Instance, Type, RequestData))
                        {
                            return;
                        }
                    }
                }
            }
        }

        public static void RegisterEventHandler(ItemBehavior BehaviorType, ItemEventHandler EventHandler)
        {
            lock (mSyncRoot)
            {
                if (!mEventHandlers.ContainsKey(BehaviorType))
                {
                    mEventHandlers.Add(BehaviorType, new List<ItemEventHandler>());
                }

                mEventHandlers[BehaviorType].Add(EventHandler);
            }
        }
    }
}