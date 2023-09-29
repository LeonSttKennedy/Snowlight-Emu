using System;
using System.Collections.Generic;
using Snowlight.Game.Sessions;

namespace Snowlight.Communication.Outgoing
{
    public static class FuseRightsListComposer
    {
        public static ServerMessage Compose(Session Session)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.USER_RIGHTS);

            Message.AppendInt32((int)Session.SubscriptionManager.SubscriptionLevel);
            
            // TODO: Dig in to the mod tool and other staff stuff further to figure out how much this does

            Message.AppendInt32(Session.HasRight("hotel_admin") ? 1000 : 0);

            return Message;
        }
    }
}
