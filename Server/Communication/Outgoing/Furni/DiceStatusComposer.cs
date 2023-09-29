using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public static class DiceStatusComposer
    {
        public static ServerMessage Compose(uint ItemId, int Number)
        {
            ServerMessage message = new ServerMessage(OpcodesOut.DICE_STATUS);
            message.AppendUInt32(ItemId);
            message.AppendInt32(Number);
            return message;
        }
    }
}