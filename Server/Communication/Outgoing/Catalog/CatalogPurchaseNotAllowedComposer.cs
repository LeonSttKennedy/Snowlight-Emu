using System;

namespace Snowlight.Communication.Outgoing
{
    public enum PurchaseNotAllowedErrorCode
    {
        SomethingIllegal = 0,
        ClubMember = 1,
    }

    public static class CatalogPurchaseNotAllowedComposer
    {
        public static ServerMessage Compose(PurchaseNotAllowedErrorCode Code)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.CATALOG_PURCHASE_ERROR);
            Message.AppendInt32((int)Code);
            return Message;
        }
    }
}
