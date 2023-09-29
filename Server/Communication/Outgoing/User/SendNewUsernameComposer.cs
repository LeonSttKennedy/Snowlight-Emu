using System;

namespace Snowlight.Communication.Outgoing
{
    public static class SendNewUsernameComposer
    {
        public static ServerMessage Compose(uint UserId, uint ActorId, string Username)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.SEND_NEW_USERNAME);
            Message.AppendUInt32(UserId);
            Message.AppendUInt32(ActorId);
            Message.AppendRawString(Username);
            return Message;
        }
    }
}
