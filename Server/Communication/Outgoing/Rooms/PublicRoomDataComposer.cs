using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public static class PublicRoomDataComposer
    {
        public static ServerMessage Compose(uint RoomId, string Swf)
        {
            ServerMessage PubRoomData = new ServerMessage(OpcodesOut.ROOM_PUBLIC_MODELDATA);
            PubRoomData.AppendUInt32(RoomId); // Unknown.
            PubRoomData.AppendStringWithBreak(Swf);
            PubRoomData.AppendUInt32(RoomId);
            return PubRoomData;
        }
    }
}
