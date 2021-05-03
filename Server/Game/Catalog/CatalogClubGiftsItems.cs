using Snowlight.Communication;
using Snowlight.Game.Items;
using Snowlight.Game.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Game.Catalog
{
    class CatalogClubGiftsItems
    {
        public string Name;
        public int BaseItem;
        public int DaysNeed;
        public bool IsVip;

        public CatalogClubGiftsItems(string mName, int mBaseItem, int mDaysNeed, bool mIsVip)
        {
            this.Name = mName;
            this.BaseItem = mBaseItem;
            this.DaysNeed = mDaysNeed;
            this.IsVip = mIsVip;
        }
    }
}
