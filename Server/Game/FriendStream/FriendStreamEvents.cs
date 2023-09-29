using Snowlight.Game.Characters;
using Snowlight.Game.Misc;
using Snowlight.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Game.FriendStream
{
    public enum EventStreamType
    {
        FriendMade = 0,
        RoomLiked = 1,
        AchievementEarned = 2,
        MottoChanged = 3,
        RoomDecorated = 4 // Never Used
    }
    public enum EventStreamLinkType
    {
        NoLink = 0,
        OpenMiniProfile = 1,
        VisitRoom = 2,
        OpenAchievements = 3,
        OpenMottoChanger = 4,
        FriendRequest = 5
    }

    public class FriendStreamEvents
    {
        private uint mId;
        private uint mUserId;
        private EventStreamType mEventType;
        private double mTimestamp;
        private EventStreamLinkType mEventLinkType;
        private List<uint> mUserLikedList;
        private string mExtraData;

        public uint Id
        {
            get
            {
                return mId;
            }
        }

        public CharacterInfo UserInfo
        {
            get
            {
                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                return CharacterInfoLoader.GetCharacterInfo(MySqlClient, mUserId);
            }
        }

        public EventStreamType EventType
        {
            get
            {
                return mEventType;
            }
        }

        public double Timestamp
        {
            get
            {
                return mTimestamp;
            }
        }
        public EventStreamLinkType LinkType
        {
            get
            {
                return mEventLinkType;
            }
        }
        public ReadOnlyCollection<uint> UserLikedList
        {
            get
            {
                List<uint> Copy = new List<uint>();
                Copy.AddRange(mUserLikedList);
                return Copy.AsReadOnly();
            }
        }
        public int LikeCount
        {
            get
            {
                return mUserLikedList.Count;
            }
        }
        public string ExtraData
        {
            get
            {
                return mExtraData;
            }
        }

        public FriendStreamEvents(uint Id, uint UserId, EventStreamType EventType, double Timestamp, 
            EventStreamLinkType LinkType, string UserLikedData, string EventData)
        {
            mId = Id;
            mUserId = UserId;
            mEventType = EventType;
            mTimestamp = Timestamp;
            mEventLinkType = LinkType;

            mUserLikedList = new List<uint>();

            string[] splittedUserIds = UserLikedData.Split('|');
            foreach(string String in splittedUserIds)
            {
                if(uint.TryParse(String, out UserId))
                {
                    if(!mUserLikedList.Contains(UserId))
                    {
                        mUserLikedList.Add(UserId);
                    }
                }
            }

            mExtraData = EventData;
        }

        public void UpdateLikeList(SqlDatabaseClient MySqlClient, uint UserId)
        {
            if (!mUserLikedList.Contains(UserId))
            {
                mUserLikedList.Add(UserId);
            }

            MySqlClient.SetParameter("id", mId);
            MySqlClient.SetParameter("list", string.Join("|", mUserLikedList));
            MySqlClient.ExecuteNonQuery("UPDATE friendstream_events SET user_ids_liked = @list WHERE id = @id");
        }
    }
}
