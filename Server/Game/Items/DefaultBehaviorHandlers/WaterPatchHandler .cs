using Snowlight.Communication;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Misc;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Storage;
using Snowlight.Util;
using System;

namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class WaterPatchHandler
    {
        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.WaterPatch, new ItemEventHandler(HandleWaterPatch));
        }

        private static bool HandleWaterPatch(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.InstanceLoaded:
                case ItemEventType.Placed:
                case ItemEventType.Moved:

                    Item.RequestUpdate(1);
                    break;

                case ItemEventType.UpdateTick:

                    string GenerateFlags = WaterBorderHelper.GetFlagsString(Instance, Item);
                    
                    int.TryParse(Item.Flags, out int CurrentFlags);
                    int NewFlags = (int)ConversionUtil.BinaryToInt(GenerateFlags);

                    if (CurrentFlags != NewFlags)
                    {
                        Item.Flags = NewFlags.ToString();
                        Item.DisplayFlags = Item.Flags;
                        Item.BroadcastStateUpdate(Instance);

                        RoomManager.MarkWriteback(Item, true);
                    }

                    Item.RequestUpdate(1);
                    break;
            }

            return true;
        }
    }
}
