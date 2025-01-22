using System;
using System.Collections.Generic;

namespace Snowlight.Game.Rights
{
    public class InventoryBadge
    {
        private uint mId;
        private BadgeDefinition mBadgeDefinition;

        public uint Id
        {
            get
            {
                return mId;
            }
        }

        public BadgeDefinition Definition
        {
            get
            {
                return mBadgeDefinition;
            }
        }

        public InventoryBadge(uint Id, BadgeDefinition Definition)
        {
            mId = Id;
            mBadgeDefinition = Definition;
        }
    }
}
