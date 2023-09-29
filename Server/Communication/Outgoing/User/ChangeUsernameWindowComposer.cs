using System;

namespace Snowlight.Communication.Outgoing
{
    public static class ChangeUsernameWindowComposer
    {
        public static ServerMessage Compose()
        {
            return new ServerMessage(OpcodesOut.CHANGE_USERNAME_WINDOW);
        }
    }
}
