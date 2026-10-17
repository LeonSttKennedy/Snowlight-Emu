using Snowlight.Communication.Outgoing;
using Snowlight.Game.Items;
using Snowlight.Game.Sessions;
using Snowlight.Specialized;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Snowlight.Game.Rooms
{
    public class RoomQueue
    {
        private RoomInstance mRoomInstance;
        private RoomInfo mRoomInfo;

        private Dictionary<QueueType, List<Session>> mNormalQueue;
        private Dictionary<QueueType, List<Session>> mClubQueue;

        public int QueuesCount
        {
            get
            {
                int c = 0;

                if (mRoomInstance.Info.EnabledQueue)
                {
                    c++;
                }

                if (mRoomInstance.Info.EnabledSpectatorQueue)
                {
                    c++;
                }

                return c;
            }
        }

        public List<Session> NormalSpectatorQueue
        {
            get
            {
                List<Session> List = new List<Session>();

                if (mNormalQueue.ContainsKey(QueueType.Spectator))
                {
                    List.AddRange(mNormalQueue[QueueType.Spectator]);
                }

                return List;
            }
        }

        public List<Session> NormalVisitorQueue
        {
            get
            {
                List<Session> List = new List<Session>();

                if (mNormalQueue.ContainsKey(QueueType.Visitor))
                {
                    List.AddRange(mNormalQueue[QueueType.Visitor]);
                }

                return List;
            }
        }

        public List<Session> ClubSpectatorQueue
        {
            get
            {
                List<Session> List = new List<Session>();

                if (mClubQueue.ContainsKey(QueueType.Spectator))
                {
                    List.AddRange(mClubQueue[QueueType.Spectator]);
                }

                return List;
            }
        }

        public List<Session> ClubVisitorQueue
        {
            get
            {
                List<Session> List = new List<Session>();

                if (mClubQueue.ContainsKey(QueueType.Visitor))
                {
                    List.AddRange(mClubQueue[QueueType.Visitor]);
                }

                return List;
            }
        }

        public RoomQueue(RoomInstance RoomInstance)
        {
            mRoomInstance = RoomInstance;
            mRoomInfo = mRoomInstance.Info;
            mNormalQueue = new Dictionary<QueueType, List<Session>>();
            mClubQueue = new Dictionary<QueueType, List<Session>>();
        }

        public void SendQueue(Session Session,  QueueType QueueType)
        {
            bool HasClub = Session.HasRight("club_regular");
            List<Session> List = HasClub ? mClubQueue[QueueType] : mNormalQueue[QueueType];

            string QueueList = GetQueueString(HasClub);
            int QueueTypeId = GetQueueId(QueueType);
            string Username = Session.CharacterInfo.Username;

            int Position = List.FindIndex(S => S.CharacterInfo.Username == Username);

            bool SendOppositeQueue = QueuesCount > 1;
            int OppositeQueueId = SendOppositeQueue ? GetOpositeQueueId(QueueType) : 0;

            Session.SendData(RoomUserQueueComposer.Compose(QueuesCount, QueueList, QueueTypeId, Username, Position, OppositeQueueId));
        }

        public void SendQueueError(Session Session, string ErrorString)
        {
            Session.SendData(RoomJoinErrorComposer.Compose(RoomJoinErrorCode.RoomJoinFailedQueueString, ErrorString));
            Session.SendData(RoomKickedComposer.Compose());
        }

        public void AddUserToQueue(Session Session, QueueType Type = QueueType.Spectator)
        {
            bool UserHasClub = Session.HasRight("club_regular");

            Dictionary<QueueType, List<Session>> Queue = UserHasClub ? mClubQueue : mNormalQueue;

            if (mRoomInfo.EnabledQueue)
            {
                EnsureQueueExists(Queue, QueueType.Visitor);
            }

            if (mRoomInfo.EnabledSpectatorQueue)
            {
                EnsureQueueExists(Queue, QueueType.Spectator);
            }

            EnsureQueueExists(Queue, Type);

            if (!Queue[Type].Contains(Session))
            {
                Queue[Type].Add(Session);
            }
        }


        public void ChangeUserQueue(Session Session, QueueType NewType)
        {
            bool UserHasClub = Session.HasRight("club_regular");

            Dictionary<QueueType, List<Session>> Queue = UserHasClub ? mClubQueue : mNormalQueue;

            foreach (List<Session> Users in Queue.Values)
            {
                Users.Remove(Session);
            }

            EnsureQueueExists(Queue, NewType);

            if (!Queue[NewType].Contains(Session))
            {
                Queue[NewType].Add(Session);
            }
        }

        public bool RemoveUserFromQueue(Session Session)
        {
            if (RemoveUserFromQueue(Session, QueueType.Spectator) ||
                RemoveUserFromQueue(Session, QueueType.Visitor))
            {
                return true;
            }

            return false;
        }

        public bool RemoveUserFromQueue(Session Session, QueueType Type = QueueType.Spectator)
        {
            bool UserHasClub = Session.HasRight("club_regular");

            Dictionary<QueueType, List<Session>> Queue = UserHasClub ? mClubQueue : mNormalQueue;

            if (Queue.TryGetValue(Type, out List<Session> Users))
            {
                return Users.Remove(Session);
            }

            return false;
        }


        private void EnsureQueueExists(Dictionary<QueueType, List<Session>> Queue, QueueType Type)
        {
            if (!Queue.ContainsKey(Type))
            {
                Queue.Add(Type, new List<Session>());
            }
        }

        public string GetQueueString(bool HasClub)
        {
            string Queue = string.Empty;

            switch (HasClub)
            {
                case true:

                    Queue = "c";
                    break;

                case false:

                    Queue = "d";
                    break;
            }

            return Queue;
        }

        public int GetQueueId(QueueType Type)
        {
            int QueueId = 0;

            switch (Type)
            {
                case QueueType.Spectator:

                    QueueId = 1;
                    break;

                case QueueType.Visitor:

                    QueueId = 2;
                    break;
            }

            return QueueId;
        }

        public int GetOpositeQueueId(QueueType Type)
        {
            int QueueId = 0;

            switch (Type)
            {
                case QueueType.Spectator:

                    QueueId = GetQueueId(QueueType.Visitor);
                    break;

                case QueueType.Visitor:

                    QueueId = GetQueueId(QueueType.Spectator);
                    break;
            }

            return QueueId;
        }
    }
}
