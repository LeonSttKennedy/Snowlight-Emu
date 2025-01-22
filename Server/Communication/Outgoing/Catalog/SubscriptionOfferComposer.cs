using Snowlight.Game.Catalog;
using Snowlight.Game.Items;
using Snowlight.Game.Rights;
using Snowlight.Game.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Communication.Outgoing
{
    public static class SubscriptionOfferComposer
    {
        public static ServerMessage Compose(ClubSubscription UserSubscription, SubscriptionOffer SubOffer)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.SUBSCRIPTION_OFFER);

            TimeSpan OfferExpire = SubOffer.TimestampExpire - DateTime.Now;
            DateTime ExpireTime = UnixTimestamp.GetDateTimeFromUnixTimestamp(UserSubscription.TimestampExpire + SubOffer.BaseOffer.LengthSeconds);

            Message.AppendUInt32(SubOffer.Id);
            Message.AppendStringWithBreak(SubOffer.BaseOffer.DisplayName);
            Message.AppendInt32(SubOffer.Price);                                                    // Cost in credits
            Message.AppendBoolean(false);                                                           // [??] Always 0?
            Message.AppendBoolean(SubOffer.BaseOffer.Level == ClubSubscriptionLevel.VipClub);       // Boolean 0 for basic, 1 for vip
            Message.AppendInt32(SubOffer.BaseOffer.LengthMonths);                                   // Months of membership
            Message.AppendInt32(SubOffer.BaseOffer.LengthDays);                                     // Actual days of membership (i.e. 31 per month)
            Message.AppendInt32(ExpireTime.Year);                                                   // Expire date/year
            Message.AppendInt32(ExpireTime.Month);                                                  // Expire date/month
            Message.AppendInt32(ExpireTime.Day);                                                    // Expire date/day
            Message.AppendInt32(SubOffer.BaseOffer.Price);
            Message.AppendInt32(OfferExpire.Days);
            return Message;
        }
    }
}
