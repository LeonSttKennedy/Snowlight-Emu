using System;
using System.Collections.Generic;
using Snowlight.Game.Navigation;
using Snowlight.Game.Rooms;
using Snowlight.Game.Rooms.Events;

namespace Snowlight.Communication.Outgoing
{
    public static class NavigatorRoomListComposer
    {
        public static ServerMessage Compose(int CategoryId, int Mode, string UserQuery, List<RoomInstance> Rooms, NavigatorOfficialItem RoomAds, bool ShowEventData = false)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.NAVIGATOR_ROOM_LIST);
            Message.AppendInt32(CategoryId);
            Message.AppendInt32(Mode);
            Message.AppendStringWithBreak(UserQuery);
            Message.AppendInt32(Rooms.Count);

            foreach (RoomInstance Instance in Rooms)
            {
                SerializeRoom(Message, Instance.Info, (ShowEventData && Instance.HasOngoingEvent ? Instance.Event : null));
            }

            Message.AppendBoolean(RoomAds != null);

            if (RoomAds != null)
            {
                SerializeRoomAdvertsement(RoomAds, Message);
            }

            return Message;
        }

        public static ServerMessage Compose(int CategoryId, int Mode, string UserQuery, List<RoomInfo> Rooms, NavigatorOfficialItem RoomAds)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.NAVIGATOR_ROOM_LIST);
            Message.AppendInt32(CategoryId);
            Message.AppendInt32(Mode);
            Message.AppendStringWithBreak(UserQuery);
            Message.AppendInt32(Rooms.Count);

            foreach (RoomInfo Info in Rooms)
            {
                SerializeRoom(Message, Info);
            }

            Message.AppendBoolean(RoomAds != null);

            if (RoomAds != null)
            {
                SerializeRoomAdvertsement(RoomAds, Message);
            }

            return Message;
        }

        public static void SerializeRoom(ServerMessage Message, RoomInfo Info, RoomEvent EventData = null)
        {
            bool ShowingEvent = (EventData != null);

            Message.AppendUInt32(Info.Id);                                                  // Actual priv room ID
            Message.AppendBoolean(ShowingEvent);                                         // Unknown
            Message.AppendStringWithBreak(ShowingEvent ? EventData.Name : Info.Name);                                       // Name
            Message.AppendStringWithBreak(Info.OwnerName);                                  // Descr
            Message.AppendInt32((int)Info.AccessType);                                      // Room state                                    
            Message.AppendInt32(Info.CurrentUsers);           // Users now
            Message.AppendInt32(Info.MaxUsers);                                             // Users max
            Message.AppendStringWithBreak(ShowingEvent ? EventData.Description : Info.Description);                                // Descr
            Message.AppendInt32(0);                                                         // Unknown, but still useless
            Message.AppendBoolean(Info.CanTrade);                                                   // Enable trade
            Message.AppendInt32(Info.Score);                                                // Score
            Message.AppendInt32(ShowingEvent? EventData.CategoryId : Info.CategoryId);                                           // Category
            Message.AppendStringWithBreak(ShowingEvent ? EventData.TimeStartedString : string.Empty);                                              // Unknown, seems no effect
            Message.AppendInt32(Info.Tags.Count);                                           // Tag count

            foreach (string Tag in Info.Tags)
            {
                Message.AppendStringWithBreak(Tag);                                          
            }
            
            Message.AppendInt32(Info.Icon.BackgroundImageId);                               // Icon bg
            Message.AppendInt32(Info.Icon.OverlayImageId);                                  // Icon overlay
            Message.AppendInt32(Info.Icon.Objects.Count);                           // Icon fg count

            foreach (KeyValuePair<int, int> Data in Info.Icon.Objects)
            {
                Message.AppendInt32(Data.Key);                                                         
                Message.AppendInt32(Data.Value);                                                        
            }

            Message.AppendBoolean(Info.AllowPets);                                          // Added in R63; unknown. Seems to have no effect.
            Message.AppendBoolean(true);                                                    // Something relationed to room advertisement, maybe is used with habblet system
        }

        private static void SerializeRoomAdvertsement(NavigatorOfficialItem Item, ServerMessage Message)
        {
            RoomInstance Instance = null;
            RoomInfo Info = null;

            if (!Item.IsCategory)
            {
                Instance = Item.TryGetRoomInstance();
                Info = Item.GetRoomInfo();
            }

            int Type = 3;

            if (Item.IsCategory)
            {
                Type = 4;
            }
            else if (Info != null && Info.Type == RoomType.Flat)
            {
                Type = 2;
            }
            else if (Item.SearchTag != string.Empty)
            {
                Type = 1;
            }

            Message.AppendUInt32(Item.Id);
            Message.AppendStringWithBreak(Item.Name);
            Message.AppendStringWithBreak(Item.Descr);
            Message.AppendInt32((int)Item.DisplayType);
            Message.AppendStringWithBreak(Item.BannerLabel);
            Message.AppendStringWithBreak(Item.ImageType == NavigatorOfficialItemImageType.External ? Item.Image : string.Empty);
            Message.AppendUInt32(Item.ParentId);
            Message.AppendInt32(Item.GetTotalUsersInPublicRoom());
            Message.AppendInt32(Type);

            if (Item.SearchTag != string.Empty)
            {
                Message.AppendStringWithBreak(Item.SearchTag);
            }
            else if (Item.IsCategory)
            {
                Message.AppendBoolean(Item.CategoryAutoExpand); // Category auto expand
            }
            else if (Info != null && Info.Type == RoomType.Public)
            {
                Message.AppendStringWithBreak(Item.ImageType == NavigatorOfficialItemImageType.Internal ? Item.Image : string.Empty);
                Message.AppendInt32(0); // Appears to be nothing but junk!
                Message.AppendInt32(0); // Something to do with room parts (e.g. lido part 0 & 1) default 0
                Message.AppendStringWithBreak(Info.SWFs);
                Message.AppendInt32(Info.MaxUsers);
                Message.AppendUInt32(Info.Id);
            }
            else if (Info != null && Info.Type == RoomType.Flat)
            {
                SerializeRoom(Message, Info);
            }
        }
    }
}
