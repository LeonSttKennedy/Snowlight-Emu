using System;

namespace Snowlight.Communication.Outgoing
{
    public enum RoomJoinErrorCode
    {
        RoomFull = 1,
        RoomJoinFailed = 2,
        RoomJoinFailedQueueString = 3,
        RoomBanned = 4,
    }

    public static class RoomJoinErrorComposer
    {
        public static ServerMessage Compose(RoomJoinErrorCode ErrorCode, string ErrorString = "")
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.ROOM_JOIN_ERROR);
            Message.AppendInt32((int)ErrorCode);
            if(ErrorCode == RoomJoinErrorCode.RoomJoinFailedQueueString)
            {
                Message.AppendStringWithBreak(ErrorString);
            }

            return Message;
        }
    }
}
