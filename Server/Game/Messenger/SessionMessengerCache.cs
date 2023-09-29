using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Snowlight.Communication.Outgoing;
using Snowlight.Communication;
using Snowlight.Game.Characters;
using Snowlight.Storage;
using Snowlight.Util;
using Snowlight.Game.Sessions;

namespace Snowlight.Game.Messenger
{
    public class SessionMessengerFriendCache : IDisposable
    {
        private uint mCharacterId;
        private List<uint> mInner;
        private List<uint> mInnerFriendRequestSent;
        private List<uint> mInnerFriendRequestReceived;
        private Dictionary<uint, int> mInnerUpdates;

        private List<MessengerCategories> mInnerCategories;

        public ReadOnlyCollection<uint> Friends
        {
            get
            {
                lock (mInner)
                {
                    List<uint> Copy = new List<uint>();
                    Copy.AddRange(mInner);
                    return Copy.AsReadOnly();
                }
            }
        }
        public ReadOnlyCollection<uint> RequestsReceived
        {
            get
            {
                lock (mInnerFriendRequestReceived)
                {
                    List<uint> Copy = new List<uint>();
                    Copy.AddRange(mInnerFriendRequestReceived);
                    return Copy.AsReadOnly();
                }
            }
        }
        public ReadOnlyCollection<uint> RequestsSent
        {
            get
            {
                lock (mInnerFriendRequestSent)
                {
                    List<uint> Copy = new List<uint>();
                    Copy.AddRange(mInnerFriendRequestSent);
                    return Copy.AsReadOnly();
                }
            }
        }
        public ReadOnlyCollection<MessengerCategories> Categories
        {
            get
            {
                lock (mInnerCategories)
                {
                    List<MessengerCategories> Copy = new List<MessengerCategories>();
                    Copy.AddRange(mInnerCategories);
                    return Copy.AsReadOnly();
                }
            }
        }

        public SessionMessengerFriendCache(SqlDatabaseClient MySqlClient, uint UserId)
        {
            mCharacterId = UserId;
            mInner = new List<uint>();
            mInnerFriendRequestReceived = new List<uint>();
            mInnerFriendRequestSent = new List<uint>();
            mInnerUpdates = new Dictionary<uint, int>();
            mInnerCategories = new List<MessengerCategories>();

            ReloadCache(MySqlClient);
        }

        public void ReloadCache(SqlDatabaseClient MySqlClient)
        {
            lock (mInner)
            {
                mInner.Clear();
                mInnerFriendRequestReceived.Clear();
                mInnerFriendRequestSent.Clear();
                mInnerUpdates.Clear();
                mInnerCategories.Clear();

                mInner.AddRange(MessengerHandler.GetFriendsForUser(MySqlClient, mCharacterId, 1));
                mInnerFriendRequestReceived.AddRange(MessengerHandler.GetFriendsForUser(MySqlClient, mCharacterId, 0));
                mInnerFriendRequestSent.AddRange(MessengerHandler.GetFriendRequestsByUser(MySqlClient, mCharacterId));
                mInnerCategories.AddRange(MessengerHandler.GetCategoriesForUser(MySqlClient, mCharacterId));
            }
        }

        public void Dispose()
        {
            if (mInner != null)
            {
                mInner.Clear();
                mInner = null;
            }
        }

        public void AddToCache(uint FriendId)
        {
            lock (mInner)
            {
                if (mInner.Contains(FriendId))
                {
                    return;
                }

                if(mInnerFriendRequestSent.Contains(FriendId))
                {
                    RemoveFromSentRequestCache(FriendId);
                }

                mInner.Add(FriendId);
                MarkUpdateNeeded(FriendId, 1);
            }
        }

        public void RemoveFromCache(uint FriendId)
        {
            lock (mInner)
            {
                if (mInner.Contains(FriendId))
                {
                    mInner.Remove(FriendId);
                }

                if (mInnerUpdates.ContainsKey(FriendId))
                {
                    mInnerUpdates.Remove(FriendId);
                }
            }
        }

        public void AddToSentRequestCache(uint FriendId)
        {
            lock (mInnerFriendRequestSent)
            {
                if (mInnerFriendRequestSent.Contains(FriendId))
                {
                    return;
                }

                mInnerFriendRequestSent.Add(FriendId);
            }
        }

        public void RemoveFromSentRequestCache(uint FriendId)
        {
            lock (mInnerFriendRequestSent)
            {
                if (mInnerFriendRequestSent.Contains(FriendId))
                {
                    mInnerFriendRequestSent.Remove(FriendId);
                }
            }
        }

        public void MarkUpdateNeeded(uint FriendId, int UpdateMode)
        {
            lock (mInnerUpdates)
            {
                if (mInnerUpdates.ContainsKey(FriendId))
                {
                    return;
                }

                mInnerUpdates.Add(FriendId, UpdateMode);
            }
        }

        public ServerMessage ComposeUpdateList()
        {
            lock (mInnerUpdates)
            {
                List<MessengerUpdate> UpdateInfo = new List<MessengerUpdate>();

                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    foreach (uint FriendId in mInner)
                    {
                        if (!mInnerUpdates.ContainsKey(FriendId))
                        {
                            continue;
                        }

                        CharacterInfo Info = CharacterInfoLoader.GetCharacterInfo(MySqlClient, FriendId);

                        if (Info == null)
                        {
                            continue;
                        }

                        UpdateInfo.Add(new MessengerUpdate(mCharacterId, mInnerUpdates[FriendId], Info));
                    }

                    mInnerUpdates.Clear();
                }

                return MessengerUpdateListComposer.Compose(mInnerCategories, UpdateInfo);
            }
        }

        public void BroadcastToFriends(ServerMessage ServerMessage)
        {
            List<uint> Copy = new List<uint>();

            lock (mInner)
            {
                Copy.AddRange(mInner);
            }

            foreach (uint FriendId in Copy)
            {
                Session SessionObject = SessionManager.GetSessionByCharacterId(FriendId);

                if (SessionObject == null)
                {
                    continue;
                }

                SessionObject.SendData(ServerMessage);
            }
        }
    }
}