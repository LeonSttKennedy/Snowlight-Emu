using System;
using System.Linq;
using Microsoft.VisualBasic.ApplicationServices;

using Snowlight.Util;
using Snowlight.Game.Catalog;
using Snowlight.Game.Rights;

namespace Snowlight.Communication.Outgoing
{
    public static class SubscriptionStatusComposer
    {
        public static ServerMessage Compose(ClubSubscription SubscriptionManager, bool BoughtFromCatalog = false)
        {
            // @Gclub_habboSGRBHJIH[MAHHHH

            // Display days
            int DisplayMonths = SubscriptionManager.DaysLeft / 31;
            int DisplayDays = SubscriptionManager.DaysLeft;

            if(DisplayMonths >= 1)
            {
                DisplayMonths--;
            }
            DisplayDays -= DisplayMonths * 31;

            // Subscription is vip
            bool IsVip = SubscriptionManager.IsActive && SubscriptionManager.SubscriptionLevel == ClubSubscriptionLevel.VipClub;

            // Subscription Offers
            SubscriptionOffer SubscriptionOffer = SubscriptionOfferManager.CheckForSubOffer(SubscriptionManager.SubscriptionLevel, SubscriptionManager.UserId);

            bool SendToClient = SubscriptionOffer != null && SubscriptionOffer.CatalogEnabled 
                && SubscriptionOffer.BasicSubscriptionReminder && DisplayDays <= ServerSettings.BasicSubscriptionReminder
                && SubscriptionManager.SubscriptionLevel == ClubSubscriptionLevel.BasicClub
                && SubscriptionOffer.BaseOffer.Level == ClubSubscriptionLevel.BasicClub;

            int RegularPrice = SendToClient ? SubscriptionOffer.BaseOffer.Price : 0;
            int YourPrice = SendToClient ? SubscriptionOffer.Price : 0;

            ServerMessage Message = new ServerMessage(OpcodesOut.SUBSCRIPTION_STATUS);
            Message.AppendStringWithBreak("habbo_club");
            Message.AppendInt32(DisplayDays);                                           // days left in the current month
            Message.AppendBoolean(SubscriptionManager.IsActive);                        // 1 if not expired
            Message.AppendInt32(DisplayMonths);                                         // months left after the current month
            Message.AppendInt32(BoughtFromCatalog ? 2 : 1);                             // was true even w/o subscription
            Message.AppendBoolean(SubscriptionManager.HasUserEverBeenMember());         // has ever been member
            Message.AppendBoolean(IsVip);                                               // Is VIP (boolean)
            Message.AppendInt32(SubscriptionManager.PastHcTimeInDays);                  // Past HC Days
            Message.AppendInt32(SubscriptionManager.PastVipTimeInDays);                 // Past VIP Days
            Message.AppendBoolean(SendToClient);                                        // Discount message enable (boolean)
            Message.AppendInt32(RegularPrice);                                          // Discount message => "Regular price"
            Message.AppendInt32(YourPrice);                                             // Discount message => "Your price"
            return Message;
        }
    }
}