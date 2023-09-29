using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Communication;
using Snowlight.Game.Characters;
using Snowlight.Game.Achievements;
using Snowlight.Communication.Incoming;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.FriendStream
{
    public static class FriendStreamHandler
    {
        public static void Initialize()
        {
            DataRouter.RegisterHandler(OpcodesIn.FRIEND_STREAM_GET_EVENTS, new ProcessRequestCallback(GetEventStream));
            DataRouter.RegisterHandler(OpcodesIn.ALLOW_FRIEND_STREAM_EVENTS, new ProcessRequestCallback(EnableEventStream));
            DataRouter.RegisterHandler(OpcodesIn.FRIEND_STREAM_LIKE_EVENT, new ProcessRequestCallback(LikeEventStream));
        }

        public static void SendAUpdatedStream(SqlDatabaseClient MySqlClient, Session SessionToUpdate)
        {
            SessionToUpdate.FriendStreamEventsCache.ReloadCache(MySqlClient, SessionToUpdate);
            SessionToUpdate.SendData(EventStreamComposer.Compose(SessionToUpdate));
        }

        public static void InsertNewEvent(uint UserId, EventStreamType EventType, string EventExtraData)
        {
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                int i = (int)EventType;

                MySqlClient.SetParameter("userid", UserId);
                MySqlClient.SetParameter("eventtype", i.ToString());
                MySqlClient.SetParameter("timestamp", UnixTimestamp.GetCurrent());
                MySqlClient.SetParameter("linktype", (int)GetLinkTypeFromEventType(EventType));
                MySqlClient.SetParameter("eventextradata", EventExtraData);

                MySqlClient.ExecuteNonQuery("INSERT INTO friendstream_events (user_id,event_type,timestamp,link_type,event_data) VALUES (@userid,@eventtype,@timestamp,@linktype,@eventextradata)");
            }
        }

        public static EventStreamLinkType GetLinkTypeFromEventType(EventStreamType Type)
        {
            EventStreamLinkType TypeToReturn;

            switch (Type)
            {
                default:
                case EventStreamType.FriendMade:
                    TypeToReturn = EventStreamLinkType.NoLink;
                    break;

                case EventStreamType.RoomLiked:
                    TypeToReturn = EventStreamLinkType.VisitRoom;
                    break;

                case EventStreamType.AchievementEarned:
                    TypeToReturn = EventStreamLinkType.OpenAchievements;
                    break;

                case EventStreamType.MottoChanged:
                    TypeToReturn = EventStreamLinkType.OpenMottoChanger;
                    break;
            }

            return TypeToReturn;
        }

        #region Handlers
        private static void GetEventStream(Session Session, ClientMessage Message)
        {
            // Anny user session sends a "1" back to server.
            int Junk = Message.PopWiredInt32(); 

            using(SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                SendAUpdatedStream(MySqlClient, Session);
            }
        }

        private static void EnableEventStream(Session Session, ClientMessage Message)
        {
            Session.CharacterInfo.AllowFriendStream = !Session.CharacterInfo.AllowFriendStream;

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                Session.CharacterInfo.UpdateFriendStreamPreference(MySqlClient);
            }
        }

        private static void LikeEventStream(Session Session, ClientMessage Message)
        {
            uint Id = Message.PopWiredUInt32();
            uint CharacterId = Message.PopWiredUInt32();

            using(SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                FriendStreamEvents Event = Session.FriendStreamEventsCache.StreamEvents.Where(O => O.Id.Equals(Id)).ToList().FirstOrDefault();

                if (Event != null)
                {
                    Event.UpdateLikeList(MySqlClient, CharacterId);
                }
            }
        }
        #endregion
    }
}
