using Snowlight.Util;
using System;

namespace Snowlight.Game.Achievements
{
    public class AchievementLevel
    {
        private int mLevel;
        private int mRewardActivityPoints;
        private SeasonalCurrencyList mSeasonalCurrency;
        private int mRewardPoints;
        private int mRequirement;

        public int Number
        {
            get
            {
                return mLevel;
            }
        }

        public SeasonalCurrencyList SeasonalCurrency
        {
            get
            {
                return mSeasonalCurrency;
            }
        }

        public int ActivityPointsReward
        {
            get
            {
                return mRewardActivityPoints;
            }
        }

        public int PointsReward
        {
            get
            {
                return mRewardPoints;
            }
        }

        public int Requirement
        {
            get
            {
                return mRequirement;
            }
        }

        public AchievementLevel(int Level, int ActivityPointsReward, SeasonalCurrencyList SeasonalPointsType, int PointReward, int Requirement)
        {
            mLevel = Level;
            mRewardActivityPoints = ActivityPointsReward;
            mSeasonalCurrency = SeasonalPointsType;
            mRewardPoints = PointReward;
            mRequirement = Requirement;
        }
    }
}
