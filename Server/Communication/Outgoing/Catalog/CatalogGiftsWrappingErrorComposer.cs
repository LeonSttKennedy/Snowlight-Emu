using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication
{
    public static class CatalogGiftsWrappingErrorComposer
    {
        public static ServerMessage Composer()
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.GIFT_WRAPPING_ERROR);
            return Message;

        }
    }
}
