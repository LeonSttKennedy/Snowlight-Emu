using Snowlight.Game.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Communication.Outgoing
{
    public static class WelcomeGiftComposer
    {
        public static ServerMessage Compose(string UserEmail, Item Item)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.WELCOME_GIFT_WINDOW);
            Message.AppendStringWithBreak(UserEmail);         // User Email
            Message.AppendBoolean(true);                      // Email Is Verified
            Message.AppendBoolean(true);                      // User can change her email
            Message.AppendUInt32(Item.Id);                    // Item Id
            Message.AppendBoolean(true);                      // Popup Window
            return Message;
        }
    }
}
