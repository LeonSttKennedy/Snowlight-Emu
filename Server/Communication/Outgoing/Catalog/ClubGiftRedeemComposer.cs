using Snowlight.Game.Catalog;
using System;

namespace Snowlight.Communication.Outgoing
{
    public static class ClubGiftRedeemComposer
    {
        public static ServerMessage Compose(SubscriptionGifts Gift)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.CATALOG_REEDEM_CLUB_GIFT);
            Message.AppendStringWithBreak(Gift.Definition.Name);
            Message.AppendBoolean(true);
            Message.AppendStringWithBreak(Gift.Definition.TypeLetter);
            Message.AppendUInt32(Gift.Definition.SpriteId);
            Message.AppendStringWithBreak("");
            Message.AppendBoolean(Gift.IsVip);
            Message.AppendBoolean(false); // canselect :D
            return Message;
        }
    }
}
