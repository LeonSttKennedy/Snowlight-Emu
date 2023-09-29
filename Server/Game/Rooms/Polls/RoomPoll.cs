using Snowlight.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Game.Rooms
{
    public class RoomPoll
    {
        private uint mId;
        private uint mRoomId;
        private string mPollTitle;
        private string mRequestMessage;
        private string mPollThanksMessage;
        private int mCreditsReward;
        private SeasonalCurrencyList mActivityPointsType;
        private int mActivityPointsReward;
        private List<uint> mFurniReward;
        private string mBadgeReward;
        private double mExpireTimestamp;
        private Dictionary<uint, RoomPollQuestions> mPollQuestions;

        public uint Id
        {
            get 
            { 
                return mId; 
            }
        }
        public uint RoomId
        {
            get
            {
                return mRoomId;
            }
        }
        public string Title
        {
            get
            {
                return mPollTitle;
            }
        }
        public string RequestMessage
        {
            get
            {
                return mRequestMessage;
            }
        }
        public string ThanksMessage
        {
            get
            {
                return mPollThanksMessage;
            }
        }
        public int CreditsRewardValue
        {
            get
            {
                return mCreditsReward;
            }
        }
        public SeasonalCurrencyList ActivityPointsType
        {
            get
            {
                return mActivityPointsType;
            }
        }
        public int ActivityPointsRewardValue
        {
            get
            {
                return mActivityPointsReward;
            }
        }
        public List<uint> FurniReward
        {
            get
            {
                return mFurniReward;
            }
        }
        public string BadgeReward
        {
            get
            {
                return mBadgeReward;
            }
        }
        public DateTime ExpireDateTime
        {
            get
            {
                DateTime UnixDateTime = UnixTimestamp.GetDateTimeFromUnixTimestamp(mExpireTimestamp);
                return new DateTime(UnixDateTime.Year, UnixDateTime.Month, UnixDateTime.Day, 0, 0, 0);
            }
        }
        public Dictionary<uint, RoomPollQuestions> Questions
        {
            get
            {
                return mPollQuestions;
            }
        }
        public bool IsExpired
        {
            get
            {
                int Compare = DateTime.Compare(DateTime.Now, ExpireDateTime);
                return Compare > -1;
            }
        }

        public RoomPoll(uint Id, uint RoomId, string PollTitle, string RequestMessage, string ThanksMessage,
            int CreditsReward, SeasonalCurrencyList ActivityPointsType, int ActivityPointsReward, List<uint> FurniReward,
            string BadgeReward, double ExpireTimestamp)
        { 
            mId = Id;
            mRoomId = RoomId;
            mPollTitle = PollTitle;
            mRequestMessage = RequestMessage;
            mPollThanksMessage = ThanksMessage;
            mCreditsReward = CreditsReward;
            mActivityPointsType = ActivityPointsType;
            mActivityPointsReward = ActivityPointsReward;
            mFurniReward = FurniReward;
            mBadgeReward = BadgeReward;
            mExpireTimestamp = ExpireTimestamp;
            mPollQuestions = new Dictionary<uint, RoomPollQuestions>();
        }
        public static RoomPoll SetByRow(DataRow Row)
        {
            List<uint> FurniReward = new List<uint>();
            foreach (string String in Row["extra_reward"].ToString().Split('|'))
            {
                uint FurniId = uint.Parse(String);
                if (FurniId > 0)
                {
                    FurniReward.Add(FurniId);
                }
            }

            return new RoomPoll((uint)Row["id"], (uint)Row["room_id"], (string)Row["title"], (string)Row["request_message"],
                (string)Row["thanks_message"], (int)Row["credits_reward"], SeasonalCurrency.FromStringToEnum(Row["seasonal_currency"].ToString()),
                (int)Row["activitypoints_reward"], FurniReward, (string)Row["badge_reward"], (double)Row["expire_timestamp"]);
        }
        public void SetPollQuestions(Dictionary<uint, RoomPollQuestions> Dictionary)
        {
            mPollQuestions.Clear();
            mPollQuestions = Dictionary;
        }
        public bool IsLastQuestion(uint QuestionId)
        {
            bool LastQuestion = true;

            for(int i = 0; i < mPollQuestions.Count; i++)
            {
                if(i >= QuestionId)
                {
                    LastQuestion = false;
                }
            }

            return LastQuestion;
        }
    }
}
