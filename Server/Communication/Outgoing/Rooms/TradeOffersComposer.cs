using System;
using System.Collections.Generic;
using System.Linq;

using Snowlight.Game.Rooms.Trading;
using Snowlight.Game.Items;

namespace Snowlight.Communication.Outgoing
{
    public static class TradeOffersComposer
    {
        public static ServerMessage Compose(Trade Trade)
        {
            List<Item> UserOneItems = Trade.UserOneOffers.Values.ToList();
            List<Item> UserTwoItems = Trade.UserTwoOffers.Values.ToList();

            ServerMessage Message = new ServerMessage(OpcodesOut.ROOM_TRADE_OFFERS);
            Message.AppendUInt32(Trade.UserOne);
            Message.AppendInt32(UserOneItems.Count);

            foreach (Item Item in UserOneItems)
            {
                SerializeTradeOffer(Message, Item);
            }
            
            Message.AppendUInt32(Trade.UserTwo);
            Message.AppendInt32(UserTwoItems.Count);

            foreach (Item Item in UserTwoItems)
            {
                SerializeTradeOffer(Message, Item);
            }

            return Message;
        }

        private static void SerializeTradeOffer(ServerMessage Message, Item Item)
        {
            int TypeNumber = 1;
            uint SecondaryId = 0;

            switch (Item.Definition.Behavior)
            {
                default:
                case ItemBehavior.StaticItem:

                    TypeNumber = 1;
                    break;

                case ItemBehavior.Wallpaper:

                    TypeNumber = 2;
                    break;

                case ItemBehavior.Floor:

                    TypeNumber = 3;
                    break;

                case ItemBehavior.Landscape:

                    TypeNumber = 4;
                    break;

                case ItemBehavior.Poster:

                    TypeNumber = 6;
                    break;

                case ItemBehavior.Gift:

                    string[] CurrentFlags = Item.DisplayFlags.Split('|');

                    if (CurrentFlags.Length > 1)
                    {
                        if (Item.Definition.BehaviorData == 2)
                        {
                            string Flag = (uint.Parse(CurrentFlags[2]) * 1000 + uint.Parse(CurrentFlags[3])).ToString();
                            uint.TryParse(Flag, out SecondaryId);
                        }
                    }
                    break;

                case ItemBehavior.MusicDisk:

                    TypeNumber = 8;
                    uint.TryParse(Item.DisplayFlags, out SecondaryId);
                    break;
            }

            // thq}bCsphq}bCZTMSBIFozzie	21-06-2011	RERA[vGH
            Message.AppendUInt32(Item.Id);
            Message.AppendStringWithBreak(Item.Definition.TypeLetter.ToUpper());
            Message.AppendUInt32(Item.Id);
            Message.AppendUInt32(Item.Definition.SpriteId);
            Message.AppendInt32(TypeNumber);
            Message.AppendBoolean(Item.Definition.AllowInventoryStack); 
            Message.AppendStringWithBreak(Item.DisplayFlags);
            Message.AppendInt32(0); // DD useless
            Message.AppendInt32(0); // MM useless
            Message.AppendInt32(0); // YYYY useless

            if (Item.Definition.Type == ItemType.FloorItem)
            {
                Message.AppendUInt32(SecondaryId);
            }
        }
    }
}
