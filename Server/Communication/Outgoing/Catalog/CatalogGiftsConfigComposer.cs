using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public static class CatalogGiftsConfigComposer
    {
        public static ServerMessage Compose()
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.CATALOG_GIFTS_CONFIG);
            Message.AppendInt32(1);
            Message.AppendInt32(1);
            Message.AppendInt32(10);
            Message.AppendInt32(3372);
            Message.AppendInt32(3373);
            Message.AppendInt32(3374);
            Message.AppendInt32(3375);
            Message.AppendInt32(3376);
            Message.AppendInt32(3377);
            Message.AppendInt32(3378);
            Message.AppendInt32(3379);
            Message.AppendInt32(3380);
            Message.AppendInt32(3381);
            Message.AppendInt32(7);
            Message.AppendInt32(0);
            Message.AppendInt32(1);
            Message.AppendInt32(2);
            Message.AppendInt32(3);
            Message.AppendInt32(4);
            Message.AppendInt32(5);
            Message.AppendInt32(6);
            Message.AppendInt32(11);
            Message.AppendInt32(0);
            Message.AppendInt32(1);
            Message.AppendInt32(2);
            Message.AppendInt32(3);
            Message.AppendInt32(4);
            Message.AppendInt32(5);
            Message.AppendInt32(6);
            Message.AppendInt32(7);
            Message.AppendInt32(8);
            Message.AppendInt32(9);
            Message.AppendInt32(10);
            Message.AppendInt32(1);
            return Message;
        }
    }
}
