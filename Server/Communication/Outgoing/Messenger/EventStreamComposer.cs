using Snowlight.Game.Characters;
using Snowlight.Game.FriendStream;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Storage;
using System;
using System.Data;

namespace Snowlight.Communication.Outgoing
{
    public static class EventStreamComposer
    {
        public static ServerMessage Compose(Session Session)
        {
            // com.sulake.habbo.communication.messages.incoming.friendlist.EventStreamEvent;
            //SBsMlUtBH31266408pquadro20mhttp://www.habbo.com/habbo-imaging/avatar/hd-195-27.ch-215-62.lg-275-62.sh-295-62.ha-1004-62,s-2.g-0.d-2.h-2.a-0,04e3deb736d1e0067d943163b8a333c2.gifYIDQAHI36954090.:DarkSaske:.s{`UtBH31266408pquadro20mhttp://www.habbo.com/habbo-imaging/avatar/hd-195-27.ch-215-62.lg-275-62.sh-295-62.ha-1004-62,s-2.g-0.d-2.h-2.a-0,04e3deb736d1e0067d943163b8a333c2.gifYIDQAHI45789055..S3phiroth..ifqR}H31266408pquadro20mhttp://www.habbo.com/habbo-imaging/avatar/hd-195-27.ch-215-62.lg-275-62.sh-295-62.ha-1004-62,s-2.g-0.d-2.h-2.a-0,04e3deb736d1e0067d943163b8a333c2.gifZvOQAHI45485689.Scissionistaj^Y`{H31266408pquadro20mhttp://www.habbo.com/habbo-imaging/avatar/hd-195-27.ch-215-62.lg-275-62.sh-295-62.ha-1004-62,s-2.g-0.d-2.h-2.a-0,04e3deb736d1e0067d943163b8a333c2.gifY|OQAHI31837771.:ketti94:.klJ`{H31266408pquadro20mhttp://www.habbo.com/habbo-imaging/avatar/hd-195-27.ch-215-62.lg-275-62.sh-295-62.ha-1004-62,s-2.g-0.d-2.h-2.a-0,04e3deb736d1e0067d943163b8a333c2.gifY|OQAHI45746957Sparvierej^}_{H31266408pquadro20mhttp://www.habbo.com/habbo-imaging/avatar/hd-195-27.ch-215-62.lg-275-62.sh-295-62.ha-1004-62,s-2.g-0.d-2.h-2.a-0,04e3deb736d1e0067d943163b8a333c2.gifY|OQAHI41683680-.:TheRed:.-hI|O{H31266408pquadro20mhttp://www.habbo.com/habbo-imaging/avatar/hd-195-27.ch-215-62.lg-275-62.sh-295-62.ha-1004-62,s-2.g-0.d-2.h-2.a-0,04e3deb736d1e0067d943163b8a333c2.gifX}OQAHI33319042.:Habbina94:.tLxTqBJ31266408pquadro20mhttp://www.habbo.com/habbo-imaging/badge/ACH_SummerQuestCompleted100ef9b85a9ef6131096c94669a04cdea1.png[r[KHIACH_SummerQuestCompleted10twLkBH31266408pquadro20mhttp://www.habbo.com/habbo-imaging/avatar/hd-195-27.ch-215-62.lg-275-62.sh-295-62.ha-1004-62,s-2.g-0.d-2.h-2.a-0,04e3deb736d1e0067d943163b8a333c2.gifZr[QAHI36814141GoofyllllltvLkBH31266408pquadro20mhttp://www.habbo.com/habbo-imaging/avatar/hd-195-27.ch-215-62.lg-275-62.sh-295-62.ha-1004-62,s-2.g-0.d-2.h-2.a-0,04e3deb736d1e0067d943163b8a333c2.gifZr[QAHI29206050ricky94ctwMGMkBH31266408pquadro20mhttp://www.habbo.com/habbo-imaging/avatar/hd-195-27.ch-215-62.lg-275-62.sh-295-62.ha-1004-62,s-2.g-0.d-2.h-2.a-0,04e3deb736d1e0067d943163b8a333c2.gifZr[QAHI31329620,HiPNOTiiiC

            ServerMessage Message = new ServerMessage(OpcodesOut.GET_STREAM_EVENT);
            Message.AppendInt32(Session.FriendStreamEventsCache.StreamEvents.Count); // EVENTS COUNT
            
            foreach(FriendStreamEvents Events in Session.FriendStreamEventsCache.StreamEvents)
            {
                int LinkType = (int)Events.LinkType;

                string IMG = Events.EventType == EventStreamType.AchievementEarned ?
                    "http://127.0.0.1/cdn.classichabbo.com/r38/gordon/RELEASE63-35255-34886-201108111108_ce2d130905ba279edbfb4208cd5035c0/c_images/album1584/" + Events.ExtraData[0] + ".gif" :
                    "http://127.0.0.1/friendstream/index.gif?figure=" + Events.UserInfo.Figure + ".gif";

                TimeSpan ElapsedTime = DateTime.Now - UnixTimestamp.GetDateTimeFromUnixTimestamp(Events.Timestamp);

                switch(Events.EventType)
                {
                    case EventStreamType.FriendMade:
                        {
                            LinkType = Session.MessengerFriendCache.Friends.Contains(uint.Parse(Events.ExtraData)) ?
                                (int)EventStreamLinkType.OpenMiniProfile : 
                                (Session.MessengerFriendCache.RequestsSent.Contains(uint.Parse(Events.ExtraData)) ?
                                (int)EventStreamLinkType.NoLink :
                                (int)EventStreamLinkType.FriendRequest);
                            
                            break;
                        }
                }

                bool CanLike = !Events.UserLikedList.Contains(Session.CharacterId);

                Message.AppendUInt32(Events.Id); // EVENTS ID
                Message.AppendInt32((int)Events.EventType); // EVENT TYPE
                Message.AppendStringWithBreak(Events.UserInfo.Id.ToString()); // USER ID AS STRING
                Message.AppendStringWithBreak(Events.UserInfo.Username); // USER NAME
                Message.AppendStringWithBreak(Events.UserInfo.Gender == CharacterGender.Male ? "m" : "f"); // USER GENDER 
                Message.AppendStringWithBreak(IMG); // HEAD IMAGE OR ACH IMAGE ACORDING WITH EVENT TYPE
                Message.AppendInt32((int)ElapsedTime.TotalMinutes); // TIME AS MINUTES
                Message.AppendInt32(LinkType); // LINK ACTION
                Message.AppendInt32(Events.LikeCount); // LIKES
                Message.AppendBoolean(CanLike); // CAN LIKE

                switch (Events.EventType)
                {
                    case EventStreamType.FriendMade:
                        {
                            uint.TryParse(Events.ExtraData, out uint FriendMadeId);
                            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                            {
                                CharacterInfo FriendMadeInfo = CharacterInfoLoader.GetCharacterInfo(MySqlClient, FriendMadeId);
                                Message.AppendStringWithBreak(FriendMadeInfo != null ? FriendMadeInfo.Id.ToString() : string.Empty);
                                Message.AppendStringWithBreak(FriendMadeInfo != null ? FriendMadeInfo.Username : "User not found");
                            }

                            break;
                        }

                    case EventStreamType.RoomLiked:
                        {
                            uint.TryParse(Events.ExtraData, out uint RoomId);
                            RoomInfo RoomInfo = RoomInfoLoader.GetRoomInfo(RoomId);

                            Message.AppendStringWithBreak(RoomInfo != null ? RoomInfo.Id.ToString() : string.Empty);
                            Message.AppendStringWithBreak(RoomInfo != null ? RoomInfo.Name : "Room not found");
                            
                            break;
                        }

                    default:
                        {
                            Message.AppendStringWithBreak(Events.ExtraData);

                            break;
                        }
                }
            }

            return Message;
        }
    }
}
