using Snowlight.Game.Items;
using Snowlight.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Game.Quests
{
    public class Quest
    {
        private uint mId;
        private string mCategory;
        private int mSeriesNumber;
        private QuestType mGoalType;
        private uint mGoalData;
        private ItemBehavior mGoalBehavior;
        private int mGoalDataHelper;
        private string mName;
        private SeasonalCurrencyList mSeasonalCurrency;
        private int mReward;
        private string mDataBit;
        private string mAchievement;

        public uint Id
        {
            get
            {
                return mId;
            }
        }

        public string Category
        {
            get
            {
                return mCategory;
            }
        }

        public int Number
        {
            get
            {
                return mSeriesNumber;
            }
        }

        public QuestType GoalType
        {
            get
            {
                return mGoalType;
            }
        }

        public uint GoalData
        {
            get
            {
                return mGoalData;
            }
        }

        public ItemBehavior GoalBehavior
        {
            get
            {
                return mGoalBehavior;
            }
        }

        public int GoalDataHelper
        {
            get
            {
                return mGoalDataHelper;
            }
        }

        public string ActionName
        {
            get
            {
                switch (mGoalType)
                {
                    case QuestType.SOCIAL_WAVE:

                        return "WAVE";

                    case QuestType.SOCIAL_DANCE:

                        return "DANCE";

                    case QuestType.SOCIAL_RESPECT:

                        return "GIVE_RESPECT";

                    case QuestType.SOCIAL_FRIEND:

                        return "REQUEST_FRIEND";

                    case QuestType.SOCIAL_CHAT:

                        return "CHAT_WITH_SOMEONE";

                    case QuestType.SOCIAL_VISIT:

                        return "ENTER_OTHERS_ROOM";

                    case QuestType.PROFILE_BADGE:

                        return "WEAR_BADGE";

                    case QuestType.PROFILE_CHANGE_MOTTO:

                        return "CHANGE_MOTTO";

                    case QuestType.PROFILE_CHANGE_LOOK:

                        return "CHANGE_FIGURE";

                    case QuestType.FURNI_DECORATION_WALL:

                        return "PLACE_WALLPAPER";

                    case QuestType.FURNI_DECORATION_FLOOR:

                        return "PLACE_FLOOR";

                    case QuestType.FURNI_STACK:

                        return "STACK_ITEM";

                    case QuestType.FURNI_SWITCH:

                        return "SWITCH_ITEM_STATE";

                    case QuestType.FURNI_PICK:

                        return "PICKUP_ITEM";

                    case QuestType.FURNI_PLACE:

                        return "PLACE_ITEM";

                    case QuestType.FURNI_ROTATE:

                        return "ROTATE_ITEM";

                    case QuestType.FURNI_MOVE:

                        return "MOVE_ITEM";

                    case QuestType.GIVE_WATER_TO_PET:

                        return "PET_DRINK";

                    case QuestType.GIVE_FOOD_TO_PET:

                        return "PET_EAT";

                    default:
                    case QuestType.EXPLORE_FIND_SPECIFIC_ITEM:
                    case QuestType.EXPLORE_FIND_ITEM_BEHAVIOR:

                        return "FIND_STUFF";
                }
            }
        }

        public string QuestName
        {
            get
            {
                return mName;
            }
        }

        public SeasonalCurrencyList SeasonalCurrency
        {
            get
            {
                return mSeasonalCurrency;
            }
        }

        public int Reward
        {
            get
            {
                return mReward;
            }
        }

        public string DataBit
        {
            get
            {
                return mDataBit;
            }
        }

        public string Achievement
        {
            get
            {
                return mAchievement;
            }
        }

        public Quest(uint Id, string Category, int Number, QuestType GoalType, uint GoalData, ItemBehavior GoalBehavior,
            int GoalDataHelper, string Name, SeasonalCurrencyList SeasonalCurrency, int Reward, string DataBit)
        {
            mId = Id;
            mCategory = Category;
            mSeriesNumber = Number;
            mGoalType = GoalType;
            mGoalData = GoalData;
            mGoalBehavior = GoalBehavior;
            mGoalDataHelper = GoalDataHelper;
            mName = Name;
            mSeasonalCurrency = SeasonalCurrency;
            mReward = Reward;
            mDataBit = DataBit;
            mAchievement = string.Empty;
        }

        public bool IsCompleted(int UserProgress)
        {
            return (UserProgress >= mGoalData);
        }

        public void SetAchievement(string AchievementCode)
        {
            mAchievement = AchievementCode;
        }
    }
}
