using Microsoft.VisualBasic;
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
        private int mGoal;
        private List<uint> mGoalData;
        private List<uint> mGoalDataHelper;
        private ItemBehavior mGoalBehavior;
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

        public int Goal
        {
            get
            {
                return mGoal;
            }
        }

        public List<uint> GoalData
        {
            get
            {
                return mGoalData;
            }
        }

        public List<uint> GoalDataHelper
        {
            get
            {
                return mGoalDataHelper;
            }
        }

        public ItemBehavior GoalBehavior
        {
            get
            {
                return mGoalBehavior;
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
                    case QuestType.FIND_SPECIFIC_FURNI:
                    case QuestType.FIND_FURNI_BEHAVIOR:

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

        public Quest(uint Id, string Category, int Number, int Goal, QuestType GoalType, string GoalData,
            string GoalDataHelper, ItemBehavior GoalBehavior, string Name, SeasonalCurrencyList SeasonalCurrency,
            int Reward, string DataBit, string Achievement)
        {
            mId = Id;
            mCategory = Category;
            mSeriesNumber = Number;
            mGoal = Goal;
            mGoalType = GoalType;
            mGoalData = new List<uint>();
            mGoalDataHelper = new List<uint>();
            mGoalBehavior = GoalBehavior;
            mName = Name;
            mSeasonalCurrency = SeasonalCurrency;
            mReward = Reward;
            mDataBit = DataBit;
            mAchievement = Achievement;


            string[] GoalDataIds = GoalData.Split(',');

            foreach (string StringId in GoalDataIds)
            {
                if (StringId != string.Empty)
                {
                    uint.TryParse(StringId, out uint GoalId);

                    if (!mGoalData.Contains(GoalId))
                    {
                        mGoalData.Add(GoalId);
                    }
                }
            }

            string[] GoalDataHelperIds = GoalDataHelper.Split(',');

            foreach (string StringId in GoalDataHelperIds)
            {
                if (StringId != string.Empty)
                {
                    uint.TryParse(StringId, out uint GoalHelperId);

                    if (!mGoalDataHelper.Contains(GoalHelperId))
                    {
                        mGoalDataHelper.Add(GoalHelperId);
                    }
                }
            }
        }

        public bool IsCompleted(int UserProgress)
        {
            return (UserProgress >= mGoal);
        }

        public bool IsGoalItem(uint GoalId)
        {
            return mGoalData.Contains(GoalId);
        }

        public bool IsGoalHelper(uint GoalId)
        {
            return mGoalDataHelper.Contains(GoalId);
        }
    }
}
