using System;
using System.Collections.Generic;
using System.Diagnostics;
using Snowlight.Game.Items;
using Snowlight.Game.Pets;

namespace Snowlight.Communication.Outgoing
{
    public static class CatalogPetDataComposer
    {
        public static ServerMessage Compose(CatalogItem Item, int PetType)
        {
            // L{a0 pet12RAPCHIHPCIIHPCJIHPCKIHPCPAIHPCQAIH
            ServerMessage Message = new ServerMessage(OpcodesOut.CATALOG_PET_DATA);
            Message.AppendStringWithBreak(Item.DisplayName);
            Message.AppendInt32(Item.DealRaces.Count);

            foreach (PetRaceData Race in Item.DealRaces)
            {
                Message.AppendInt32(PetType);                           // Pet type
                Message.AppendInt32(Race.Breed);                        // Pet color / breed
                Message.AppendBoolean(Race.Sellable);                   // Sellable
                Message.AppendBoolean(Race.IsRare);                     // Is Rare
            }

            return Message;
        }
    }
}
