using System;
using System.Collections.Generic;

namespace Snowlight.Communication.Outgoing
{
    public static class UserActivityPointsBalanceComposer
    {
        public static ServerMessage Compose(Dictionary<int, int> ActivityPoints)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.SAZONAL_ACTIVITY_POINTS_BALANCE);
            Message.AppendInt32(ActivityPoints.Count); //count
            foreach(KeyValuePair<int, int> APData in ActivityPoints)
            {
                Message.AppendInt32(APData.Key); //id
                Message.AppendInt32(APData.Value); //amount
            }

            return Message;
        }
    }
}
