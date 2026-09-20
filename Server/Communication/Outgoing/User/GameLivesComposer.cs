using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Communication.Outgoing
{
    public static class GameLivesComposer
    {
        public static ServerMessage Compose(uint ActorId, int Lives)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.FREEZE_GAME_LIVES);
            Message.AppendUInt32(ActorId);
            Message.AppendInt32(Lives);
            return Message;
        }
    }
}
