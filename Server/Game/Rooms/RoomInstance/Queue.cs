using System;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Rooms
{
    public partial class RoomInstance : IDisposable
    {
        private RoomQueue mRoomQueue;

        public RoomQueue Queue
        {
            get 
            {
                return mRoomQueue; 
            }
        }

        public void QueueUpdate()
        {
            UpdateQueue(mRoomQueue.ClubSpectatorQueue, QueueType.Spectator);
            UpdateQueue(mRoomQueue.ClubVisitorQueue, QueueType.Visitor);

            UpdateQueue(mRoomQueue.NormalSpectatorQueue, QueueType.Spectator);
            UpdateQueue(mRoomQueue.NormalVisitorQueue, QueueType.Visitor);

            int Random = RandomGenerator.GetNext(0, 9);

            switch (Random)
            {
                case 0:
                case 1:
                case 2:
                case 4:
                case 6:
                case 8:

                    UpdateQueue(mRoomQueue.ClubSpectatorQueue, QueueType.Spectator, true);
                    UpdateQueue(mRoomQueue.ClubVisitorQueue, QueueType.Visitor, true);
                    break;

                case 3:
                case 5:
                case 7:
                case 9:

                    UpdateQueue(mRoomQueue.NormalSpectatorQueue, QueueType.Spectator, true);
                    UpdateQueue(mRoomQueue.NormalVisitorQueue, QueueType.Visitor, true);
                    break;
            }
        }

        private void UpdateQueue(List<Session> Sessions, QueueType QueueType, bool AllowEnter = false)
        {
            if (Sessions.Count == 0)
            {
                return;
            }

            foreach (Session Session in Sessions)
            {
                if (AllowEnter)
                {
                    bool CanVisitorEnter = Info.CurrentUsers < Info.MaxUsers;
                    bool CanSpectatorEnter = Info.CurrentSpectators < Info.MaxSpectators;

                    bool CanEnter = QueueType == QueueType.Spectator ? CanSpectatorEnter : CanVisitorEnter;

                    if (CanEnter && Sessions.IndexOf(Session) == 0)
                    {
                        if (QueueType == QueueType.Spectator)
                        {
                            Session.SpectatorQueue = true;
                            Session.SpectatorQueueRoomId = RoomId;
                        }

                        Queue.RemoveUserFromQueue(Session);
                        RoomHandler.PrepareRoom(Session, RoomId, string.Empty, false, true);
                        break;
                    }
                }

                Queue.SendQueue(Session, QueueType);
            }
        }
    }
}
