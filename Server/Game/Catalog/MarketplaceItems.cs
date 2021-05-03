using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Game.Catalog
{
    class MarketplaceItems
    {   
        public int OfferID { get; set; }
        public int ItemType { get; set; }
        public int Sprite { get; set; }
        public int TotalPrice { get; set; }
        public int LimitedNumber { get; set; }
        public int LimitedStack { get; set; }

        public MarketplaceItems(int OfferID, int Sprite, int TotalPrice, int ItemType, int LimitedNumber, int LimitedStack)
        {
            this.OfferID = OfferID;
            this.Sprite = Sprite;
            this.ItemType = ItemType;
            this.TotalPrice = TotalPrice;
            this.LimitedNumber = LimitedNumber;
            this.LimitedStack = LimitedStack;
        }
    }
}
