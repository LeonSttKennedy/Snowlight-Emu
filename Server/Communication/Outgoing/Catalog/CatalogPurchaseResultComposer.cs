using System;

using Snowlight.Game.Items;
using Snowlight.Game.Catalog;
using System.Collections.Generic;
using System.Linq;

namespace Snowlight.Communication.Outgoing
{
    public static class CatalogPurchaseResultComposer
    {
        public static ServerMessage Compose(CatalogItem Item)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.CATALOG_PURCHASE_RESULT);
            Message.AppendUInt32(Item.Definition.Id);
            Message.AppendStringWithBreak(Item.DisplayName);
            Message.AppendInt32(Item.CostCredits);
            Message.AppendInt32(Item.CostActivityPoints);
            Message.AppendInt32((int)Item.SeasonalCurrency);
            Message.AppendInt32(Item.IsDeal ? Item.DealItems.Count : 1);
            if (Item.IsDeal)
            {
                foreach (CatalogItem DealItems in Item.DealItems)
                {
                    if (DealItems.Definition.TypeLetter == "s" || DealItems.Definition.TypeLetter == "i")
                    {
                        Message.AppendStringWithBreak(DealItems.Definition.TypeLetter);
                        Message.AppendUInt32(DealItems.Definition.SpriteId);
                        Message.AppendStringWithBreak(DealItems.ShowPresetFlags() ? Item.PresetFlags : string.Empty);
                        Message.AppendInt32(DealItems.Amount); // amount
                        Message.AppendInt32(-1); // ?????????????
                    }
                    else
                    {
                        throw new Exception("Only normal items are supported for deals");
                    }
                }
            }
            else
            {
                Message.AppendStringWithBreak(Item.Definition.TypeLetter);
                Message.AppendUInt32(Item.Definition.SpriteId);
                Message.AppendStringWithBreak(Item.ShowPresetFlags() ? Item.PresetFlags : string.Empty);
                Message.AppendInt32(Item.Amount); // ??????????????????????
                Message.AppendInt32(-1); // ?????????????
            }

            Message.AppendInt32(Item.ClubRestriction); // ?????????????????
            return Message;
        }

        public static ServerMessage Compose(CatalogClubOffer Item)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.CATALOG_PURCHASE_RESULT);
            Message.AppendUInt32(Item.Id);
            Message.AppendStringWithBreak(Item.DisplayName);
            Message.AppendInt32(Item.Price);
            Message.AppendInt32(0);
            Message.AppendInt32(0);
            Message.AppendInt32(0);
            Message.AppendInt32(0);
            return Message;
        }
    }
}
