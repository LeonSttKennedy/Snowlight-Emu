using System;

namespace Snowlight.Communication.Outgoing
{
    public enum RedeemError
    {
        InvalidCode = 0,
        TechnicalError = 1,
        ReedemHabboWeb = 3
    }
    public static class CatalogRedeemErrorComposer
    {
        public static ServerMessage Compose(RedeemError Code)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.CATALOG_REDEEM_ERROR);
            Message.AppendRawInt32((int)Code);
            return Message;
        }
    }
}
