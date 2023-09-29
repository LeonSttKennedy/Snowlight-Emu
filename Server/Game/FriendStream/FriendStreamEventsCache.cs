using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Snowlight.Storage;
using Snowlight.Game.Rights;
using Snowlight.Game.Sessions;
using Snowlight.Game.Messenger;
using Snowlight.Communication.Outgoing;


namespace Snowlight.Game.FriendStream
{
    public class FriendStreamEventsCache : IDisposable
    {
        private uint mCharacterId;
        private List<FriendStreamEvents> mInner;

        private List<string> mQuery;

        public ReadOnlyCollection<FriendStreamEvents> StreamEvents
        {
            get
            {
                lock (mInner)
                {
                    List<FriendStreamEvents> Copy = new List<FriendStreamEvents>();
                    Copy.AddRange(mInner);
                    return Copy.AsReadOnly();
                }
            }
        }

        public FriendStreamEventsCache(SqlDatabaseClient MySqlClient, Session Session)
        {
            mCharacterId = Session.CharacterId;
            mInner = new List<FriendStreamEvents>();
            mQuery = new List<string>();

            ReloadCache(MySqlClient, Session);
        }

        public void ReloadCache(SqlDatabaseClient MySqlClient, Session Session)
        {
            lock (mInner)
            {
                string Where = string.Empty;

                mInner.Clear();
                mQuery.Clear();
                
                foreach(uint FriendId in Session.MessengerFriendCache.Friends.ToList())
                {
                    mQuery.Add(" user_id = " + FriendId);
                }

                if(mQuery.Count > 0)
                {
                    Where = " WHERE " + string.Join(" OR", mQuery);
                }

                DataTable StreamsData = MySqlClient.ExecuteQueryTable("SELECT * FROM friendstream_events" + Where + " ORDER BY timestamp DESC");

                foreach(DataRow Row in StreamsData.Rows)
                {
                    EventStreamType Type = (EventStreamType)int.Parse((Row["event_type"].ToString()));
                    string EventData = Row["event_data"].ToString();

                    if (Type == EventStreamType.FriendMade && EventData.Contains(mCharacterId.ToString()))
                    {
                        continue;
                    }
                    
                    mInner.Add(new FriendStreamEvents((uint)Row["id"], (uint)Row["user_id"], Type,
                        (double)Row["timestamp"], (EventStreamLinkType)int.Parse((Row["link_type"].ToString())),
                        Row["user_ids_liked"].ToString(), EventData));
                }
            }
        }

        public void Dispose()
        {
            if (mInner != null)
            {
                mInner.Clear();
                mInner = null;
            }

            if(mQuery != null)
            {
                mQuery.Clear();
                mQuery = null;
            }
        }
    }
}
