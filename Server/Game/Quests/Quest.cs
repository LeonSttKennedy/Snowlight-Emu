using Snowlight.Game.Items;
using Snowlight.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Game.Quests
{
    public enum QuestType
    {
        FURNI_MOVE = 0,
        FURNI_ROTATE = 1,
        FURNI_PLACE = 2,
        FURNI_PICK = 3,
        FURNI_SWITCH = 4,
        FURNI_STACK = 5,
        FURNI_DECORATION_FLOOR = 6,
        FURNI_DECORATION_WALL = 7,
        SOCIAL_VISIT = 8,
        SOCIAL_CHAT = 9,
        SOCIAL_FRIEND = 10,
        SOCIAL_RESPECT = 11,
        SOCIAL_DANCE = 12,
        SOCIAL_WAVE = 13,
        PROFILE_CHANGE_LOOK = 14,
        PROFILE_CHANGE_MOTTO = 15,
        PROFILE_BADGE = 16,
        EXPLORE_FIND_SPECIFIC_ITEM = 17,
        EXPLORE_FIND_ITEM_BEHAVIOR = 18,
        GIVE_FOOD_TO_PET = 19,
        GIVE_WATER_TO_PET = 20,
        LEVEL_UP_A_PET = 21,
        PET_TO_OTHER_ROOM = 22,
        PETS_IN_ROOM = 23,
        SCRATCH_A_PET = 24,
        FIND_A_PET_TYPE = 25
    }

    public class Quest
    {
        private uint mId;
        private string mCategory;
        private int mSeriesNumber;
        private QuestType mGoalType;
        private uint mGoalData;
        private ItemBehavior mGoalDataBehavior;
        private string mName;
        private SeasonalCurrencyList mSeasonalCurrency;
        private int mReward;
        private string mDataBit;

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

        public ItemBehavior GoalDataBehavior
        {
            get
            {
                return mGoalDataBehavior;
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

        public Quest(uint Id, string Category, int Number, QuestType GoalType, uint GoalData, ItemBehavior GoalDataBehavior,
            string Name, SeasonalCurrencyList SeasonalCurrency, int Reward, string DataBit)
        {
            mId = Id;
            mCategory = Category;
            mSeriesNumber = Number;
            mGoalType = GoalType;
            mGoalData = GoalData;
            mGoalDataBehavior = GoalDataBehavior;
            mName = Name;
            mSeasonalCurrency = SeasonalCurrency;
            mReward = Reward;
            mDataBit = DataBit;
        }

        public bool IsCompleted(int UserProgress)
        {
            switch (mGoalType)
            {
                default:

                    return (UserProgress >= mGoalData);

                case QuestType.EXPLORE_FIND_SPECIFIC_ITEM:
                case QuestType.EXPLORE_FIND_ITEM_BEHAVIOR:
                case QuestType.FIND_A_PET_TYPE:

                    return (UserProgress >= 1);
            }
        }
    }
}
