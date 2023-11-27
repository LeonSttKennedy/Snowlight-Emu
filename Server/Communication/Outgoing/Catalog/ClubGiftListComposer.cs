using Snowlight.Game.Catalog;
using Snowlight.Game.Items;
using Snowlight.Game.Rights;
using System;
using System.Collections.Generic;

namespace Snowlight.Communication.Outgoing
{
    public static class ClubGiftListComposer
    {
        public static ServerMessage Compose(ClubSubscription Subscription, List<SubscriptionGifts> GiftList)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.CATALOG_CLUB_GIFT);
            Message.AppendInt32(Subscription.IsActive ? Subscription.NextGiftPoint : 0); // Days until next gift.
            Message.AppendInt32(Subscription.IsActive ? Subscription.GiftPoints : 0);    // Gifts available.

            Message.AppendInt32(GiftList.Count); // Items Count
            foreach (SubscriptionGifts Gift in GiftList)
            {
                Message.AppendUInt32(Gift.Id);
                Message.AppendStringWithBreak(Gift.ItemName);
                Message.AppendInt32(0);
                Message.AppendInt32(0);
                Message.AppendInt32(0);
                Message.AppendInt32(Gift.IsDeal ? Gift.DealItems.Count : 1);

                if (Gift.IsDeal)
                {
                    foreach (SubscriptionGifts DealItems in Gift.DealItems)
                    {
                        if(DealItems.DefinitionId == -1)
                        {
                            continue;
                        }

                        if (DealItems.Definition.TypeLetter == "s" || DealItems.Definition.TypeLetter == "i")
                        {
                            Message.AppendStringWithBreak(DealItems.Definition.TypeLetter);
                            Message.AppendUInt32(DealItems.Definition.SpriteId);
                            Message.AppendStringWithBreak(DealItems.ShowPresetFlags ? DealItems.PresetFlags : string.Empty); // unknown
                            Message.AppendInt32(DealItems.Amount); // amount
                            Message.AppendInt32(-1);
                        }
                        else
                        {
                            throw new Exception("Only normal items are supported for deals");
                        }
                    }
                }
                else
                {
                    Message.AppendStringWithBreak(Gift.Definition.TypeLetter);
                    Message.AppendUInt32(Gift.Definition.SpriteId);
                    Message.AppendStringWithBreak(Gift.ShowPresetFlags ? Gift.PresetFlags : string.Empty); // unknown
                    Message.AppendInt32(Gift.Amount);   // amount
                    Message.AppendInt32(-1);
                }

                Message.AppendInt32(-1);                                                    // Added in RELEASE63-34159-34129-201106010852
            }

            Message.AppendInt32(GiftList.Count);
            foreach (SubscriptionGifts Gift in GiftList)
            {
                Message.AppendUInt32(Gift.Id);                                              // Offer ID
                Message.AppendBoolean(Gift.IsVip);                                          // is item vip
                Message.AppendInt32(Gift.DaysNeed);                                         // days to unlock
                Message.AppendBoolean(CatalogSubGifts.CanSelectGift(Subscription, Gift));   // canselect
            }
            return Message;
        }
    }
}
