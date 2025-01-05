using System;

namespace Snowlight.Communication.Outgoing
{
    public enum MessengerErrorCode
    {
        UNKNOWN = 0,
        FriendListOwnLimit = 1,
        FriendListOfRequester = 2,
        FriendRequestDisabled = 3,
        RequestNotFound = 4
    }

    public static class MessengerErrorEvent
    {
        public static ServerMessage Compose(uint UserId, MessengerErrorCode ErrorCode)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.MESSENGER_ERROR_EVENT);
            Message.AppendUInt32(UserId);
            Message.AppendUInt32((uint)ErrorCode);
            return Message;
        }
    }
}
