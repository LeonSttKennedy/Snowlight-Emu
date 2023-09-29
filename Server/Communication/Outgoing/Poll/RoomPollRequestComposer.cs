using Snowlight.Game.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Communication.Outgoing
{
    public static class RoomPollRequestComposer
    {
        public static ServerMessage Compose(RoomPoll Poll)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.ROOM_POLL_REQUEST);
            Message.AppendUInt32(Poll.RoomId);
            Message.AppendStringWithBreak(Poll.RequestMessage);
            return Message;

        }
    }
}
