using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public static class InfoFeedEnableMessageComposer
    {
        public static ServerMessage Compose(bool AllowStream)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.INFO_FEED_ENABLE);
            Message.AppendBoolean(AllowStream); // 1 = enabled     0 = disabled  (true/false)
            return Message;
        }
    }
}
