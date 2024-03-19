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
            Message.AppendInt32(Gift.IsDeal ? Gift.DealItems.Count : 1);

            if (Gift.IsDeal)
            {
                foreach (SubscriptionGifts DealGift in Gift.DealItems)
                {
                    if (DealGift.DefinitionId == -1)
                    {
                        continue;
                    }

                    if (DealGift.Definition.TypeLetter == "s" || DealGift.Definition.TypeLetter == "i")
                    {
                        Message.AppendStringWithBreak(DealGift.Definition.TypeLetter);                                              // Product Type
                        Message.AppendUInt32(DealGift.Definition.SpriteId);                                                         // Sprite Id
                        Message.AppendStringWithBreak(DealGift.ShowPresetFlags ? DealGift.PresetFlags : string.Empty);              // Extra Param
                        Message.AppendInt32(DealGift.Amount);                                                                       // Product Count
                        Message.AppendInt32(0);                                                                                     // Expiration
                    }
                    else
                    {
                        throw new Exception("Only normal items are supported for deals");
                    }
                }
            }
            else
            {
                Message.AppendStringWithBreak(Gift.Definition.TypeLetter);                                          // Product Type
                Message.AppendUInt32(Gift.Definition.SpriteId);                                                     // Sprite Id
                Message.AppendStringWithBreak(Gift.ShowPresetFlags ? Gift.PresetFlags : string.Empty);              // Extra Param
                Message.AppendInt32(Gift.Amount);                                                                   // Product Count
                Message.AppendInt32(0);                                                                             // Expiration
            }

            return Message;
        }
    }
}
