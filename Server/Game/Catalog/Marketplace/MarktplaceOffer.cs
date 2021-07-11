using Snowlight.Game.Items;
using Snowlight.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Snowlight.Game.Catalog
{
    public class MarketplaceOffers
    {
        public int OfferID { get; set; }
        public uint ItemID { get; set; }
        public int ItemType { get; set; }
        public uint Sprite { get; set; }
        public int TotalPrice { get; set; }
        public int LimitedNumber { get; set; }
        public int LimitedStack { get; set; }

        public MarketplaceOffers(int OfferID, uint ItemID, uint Sprite, int TotalPrice, int ItemType, int LimitedNumber, int LimitedStack)
        {
            this.OfferID = OfferID;
            this.ItemID = ItemID;
            this.Sprite = Sprite;
            this.ItemType = ItemType;
            this.TotalPrice = TotalPrice;
            this.LimitedNumber = LimitedNumber;
            this.LimitedStack = LimitedStack;
        }

        public ItemDefinition GetItemDef()
        {
            return ItemDefinitionManager.GetDefinition(ItemID);
        }

        public int GetSoldsTodayBySpriteId()
        {
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                MySqlClient.SetParameter("sprite", Sprite.ToString());
                DataRow Row = MySqlClient.ExecuteQueryRow("SELECT * FROM catalog_marketplace_data WHERE sprite_id = @sprite LIMIT 1");

                return (Row != null ? (int)Row["daily_sold"] : 0);
            }
        }
    }
}