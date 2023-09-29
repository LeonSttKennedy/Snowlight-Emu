using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public static class FireworksChargeInfoComposer
    {
        public static ServerMessage Compose(uint ItemId, int CurrentCharge, int CostCredits, int CostActivityPoints, int SeasonalCurrency, int ChargeAmount)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.FIREWORKS_CHARGE_INFO);
            Message.AppendUInt32(ItemId);
            Message.AppendInt32(CurrentCharge);
            Message.AppendInt32(CostCredits); // Cost of charges in credits
            Message.AppendInt32(CostActivityPoints); // Cost of charges in pixels/snowflakes
            Message.AppendInt32(SeasonalCurrency); // Currency type for costs (0 = Pixels, 1 = Snowflakes, 2 = Hearts)
            Message.AppendInt32(ChargeAmount); // Amount of charges that will be added
            return Message;
        }
    }
}
