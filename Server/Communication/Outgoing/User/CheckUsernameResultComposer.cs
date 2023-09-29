using System;

namespace Snowlight.Communication.Outgoing
{
    public enum ChangeNameErrorCode
    {
        Allowed = 0,
        UNKNOWN = 1,
        Username_Short = 2,
        Username_Long = 3,
        Invalid = 4,
        Taken = 5,
        Change_Not_Allowed = 6,
        Merge_Hotel_Down = 7
    }

    public static class CheckUsernameResultComposer
    {
        public static ServerMessage Compose(ChangeNameErrorCode ErrorCode, string Username)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.CHECK_USERNAME_RESULT);
            Message.AppendInt32((int)ErrorCode);

            switch(ErrorCode)
            {
                case ChangeNameErrorCode.Allowed:
                case ChangeNameErrorCode.Invalid:

                    Message.AppendRawString(Username);
                    break;

                case ChangeNameErrorCode.Taken:

                    Message.AppendStringWithBreak(Username);        // Selected username
                    Message.AppendInt32(2);                         // Total suggestions
                    Message.AppendStringWithBreak(Username+"1");    // Sugestion string exemple
                    Message.AppendStringWithBreak(Username+"2");    // Sugestion string exemple
                    break;
            }

            return Message;
        }
    }
}
