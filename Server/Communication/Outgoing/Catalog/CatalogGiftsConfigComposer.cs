using Snowlight.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Communication.Outgoing
{
    public static class CatalogGiftsConfigComposer
    {
        public static ServerMessage Compose()
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.CATALOG_GIFTS_CONFIG);
            Message.AppendBoolean(ServerSettings.NewGiftingSystem); // New gifting system enabled

            if (ServerSettings.NewGiftingSystem)
            {
                Message.AppendInt32(ServerSettings.GiftingSystemPrice); // Extra packages price

                Message.AppendInt32(ServerSettings.GiftingSystemSpriteIds.Count); // Total gift sprite ids
                foreach (uint GiftSpriteId in ServerSettings.GiftingSystemSpriteIds)
                {
                    Message.AppendUInt32(GiftSpriteId);
                }

                Message.AppendInt32(ServerSettings.GiftingSystemBoxCount); // Total gift boxes
                for (int i = 0; i < ServerSettings.GiftingSystemBoxCount; i++)
                {
                    Message.AppendInt32(i);
                }

                Message.AppendInt32(ServerSettings.GiftingSystemRibbonCount); // Total gift ribbons
                for (int i = 0; i < ServerSettings.GiftingSystemRibbonCount; i++)
                {
                    Message.AppendInt32(i);
                }
            }

            return Message;
        }
    }


}
