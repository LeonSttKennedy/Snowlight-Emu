using Snowlight.Game.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Communication.Outgoing
{
    public static class NotificationEventComposer
    {
        public static ServerMessage Compose(string MessageText)
        {
            /*
             * In habbo swf scripts, this event composer needs a external flash texts to works:
             * "notifications.text.room.messages.posted"
             * He has two variables to show what you want:
             * %room_name% = that receives room name variable from our composer
             * %messages_count% = that receives messages count variable from our composer
             */

            ServerMessage Message = new ServerMessage(OpcodesOut.ROOM_NOTIFICATION_EVENT);
            // this receive Room id, swf doesn't check room id, so we can send anything here
            Message.AppendInt32(0);

            /* 
             * this receive room name.
             * here we can send what we want and will display at %room_name% variable
             * remenber to insert in your external texts = notifications.text.room.messages.posted=%room_name% */
            Message.AppendStringWithBreak(MessageText);

            /*
             * this receive messages room count.
             * this we can send any number.
             * if isn't set %messages_count% in "notifications.text.room.messages.posted" will display nothing */
            Message.AppendInt32(0);                 
            return Message;
        }
    }
}
