using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Game.Items
{
    public enum ActionToReward
    {
        None = -1,
        Buying = 0,
        Placing = 1
    }

    public class ItemAchievements
    {
        private uint mDefinitionId;
        private string mAchievementCode;

        public uint DefinitionId
        {
            get 
            {
                return mDefinitionId; 
            }
        }

        public string AchievementCode
        {
            get
            {
                return mAchievementCode;
            }
        }

        public ItemAchievements(uint DefinitionId, string AchievementCode)
        {
            mDefinitionId = DefinitionId;
            mAchievementCode = AchievementCode;
        }
    }
}
