using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public static class OneWayGateStatusComposer
    {
        public static ServerMessage Compose(uint ItemId, bool CanPass)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.ONE_WAY_GATE_STATUS);
            Message.AppendUInt32(ItemId);           // ItemID
            Message.AppendBoolean(CanPass);         // Status
            return Message;
        }
    }
}
