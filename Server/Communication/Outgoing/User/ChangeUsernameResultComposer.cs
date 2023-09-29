using System;

namespace Snowlight.Communication.Outgoing
{
    public static class ChangeUsernameResultComposer
    {
        public static ServerMessage Compose(ChangeNameErrorCode ErrorCode)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.CHANGE_USERNAME_RESULT);
            Message.AppendInt32((int)ErrorCode);
            return Message;
        }
    }
}
