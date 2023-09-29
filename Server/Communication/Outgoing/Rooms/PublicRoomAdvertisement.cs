using System;

using Snowlight.Game.Advertisements;

namespace Snowlight.Communication.Outgoing
{
    public static class PublicRoomAdvertisement
    {
        public static ServerMessage Compose(Interstitial Interstitial)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.PUBLIC_ROOM_ADS);
            Message.AppendStringWithBreak(Interstitial == null ? string.Empty : Interstitial.Image);
            Message.AppendStringWithBreak(Interstitial == null ? string.Empty : Interstitial.Url);
            return Message;
        }
    }
}
