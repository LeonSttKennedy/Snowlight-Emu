using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using System.Data;

using Snowlight.Specialized;
using Snowlight.Game.Sessions;
using Snowlight.Communication;
using Snowlight.Game.Characters;
using Snowlight.Communication.Outgoing;
using Snowlight.Storage;
using Snowlight.Game.Pathfinding;
using Snowlight.Game.Bots;
using Snowlight.Game.Rooms.Events;
using Snowlight.Game.Items;

using Snowlight.Game.Pets;
using Snowlight.Game.Music;
using Snowlight.Game.Rooms.Trading;
using Snowlight.Game.Items.Wired;
using Snowlight.Game.Advertisements;

namespace Snowlight.Game.Rooms
{

    public partial class RoomInstance : IDisposable
    {
        private uint mInstanceId;
        private RoomInfo mInfo;
        private bool mUnloaded;
        private double mUnloadedTimestamp;
        private int mMarkedEmptyRoom;

        private bool mDiagonalEnabled;

        public uint InstanceId
        {
            get
            {
                return mInstanceId;
            }
        }

        public RoomInfo Info
        {
            get
            {
                return mInfo;
            }
        }

        public uint RoomId
        {
            get
            {
                return mInfo.Id;
            }
        }

        public bool Unloaded
        {
            get
            {
                return mUnloaded;
            }
        }

        public double TimeUnloaded
        {
            get
            {
                return (Unloaded ? UnixTimestamp.GetCurrent() - mUnloadedTimestamp : 0);
            }
        }

        public int MarkedAsEmpty
        {
            get
            {
                return mMarkedEmptyRoom;
            }

            set
            {
                mMarkedEmptyRoom = value;
            }
        }

        public List<string> SearchableTags
        {
            get
            {
                List<string> Tags = new List<string>();
                Tags.AddRange(Info.Tags);

                if (HasOngoingEvent)
                {
                    Tags.AddRange(Event.Tags);
                }

                return Tags;
            }
        }
        public bool DisableDiagonal
        {
            get 
            {
                return mDiagonalEnabled;
            }

            set
            {
                mDiagonalEnabled = value;
            }
        }

        public RoomInstance(uint InstanceId, RoomInfo Info, RoomModel Model)
        {
            mActorSyncRoot = new object();
            mItemSyncRoot = new object();

            mInstanceId = InstanceId;
            mInfo = Info;
            mActors = new Dictionary<uint, RoomActor>();
            mCachedModel = Model;
            mRelativeHeightmap = string.Empty;
            mActorIdGenerator = 1;
            mActorIdGeneratorSyncLock = new object();
            mTileStates = new TileState[mCachedModel.Heightmap.SizeX, mCachedModel.Heightmap.SizeY];
            mUsersWithRights = new List<uint>();
            mBannedUsers = new Dictionary<uint, double>();
            mItems = new Dictionary<uint, Item>();
            mStaticObjects = new List<StaticObject>();
            mItemLimitCache = new Dictionary<ItemBehavior, int>();
            mUserGrid = new List<RoomActor>[mCachedModel.Heightmap.SizeX, mCachedModel.Heightmap.SizeY];
            mMusicController = new RoomMusicController();
            mTemporaryStickieRights = new Dictionary<uint, uint>();
            mTradeManager = new TradeManager();
            mRollerItems = new List<Item>[mCachedModel.Heightmap.SizeX, mCachedModel.Heightmap.SizeY];
            mRoomTriggers = new List<RoomTriggers>();
            mWiredManager = new WiredManager(this);

            mDiagonalEnabled = true;

            foreach (Bot Bot in BotManager.GenerateBotInstancesForRoom(RoomId))
            {
                AddBotToRoom(Bot);
            }

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                // Items
                MySqlClient.SetParameter("id", RoomId);
                DataTable ItemTable = MySqlClient.ExecuteQueryTable("SELECT * FROM items WHERE room_id = @id");

                foreach (DataRow Row in ItemTable.Rows)
                {
                    Item Item = ItemFactory.CreateFromDatabaseRow(Row, mWiredManager);

                    if (Item.PendingExpiration && Item.ExpireTimeLeft <= 0)
                    {
                        Item.RemovePermanently(MySqlClient);
                        continue;
                    }

                    if (Item.InSoundManager)
                    {
                        continue;
                    }

                    mItems.Add(Item.Id, Item);
                    IncrecementFurniLimitCache(Item.Definition.Behavior);

                    ItemEventDispatcher.InvokeItemEventHandler(null, Item, this, ItemEventType.InstanceLoaded);
                }

                // Static objects
                MySqlClient.SetParameter("id", RoomId);
                MySqlClient.SetParameter("enabled", "1");
                DataTable StaticObjectTable = MySqlClient.ExecuteQueryTable("SELECT * FROM static_objects WHERE room_id = @id AND enabled = @enabled");

                foreach (DataRow Row in StaticObjectTable.Rows)
                {
                    mStaticObjects.Add(new StaticObject((string)Row["name"], Vector3.FromString((string)Row["position"]),
                        (int)Row["size_x"], (int)Row["size_y"], (int)Row["rotation"], (float)Row["height"],
                         (Row["walkable"].ToString() == "1"), (Row["is_seat"].ToString() == "1")));
                }

                // Rights
                MySqlClient.SetParameter("id", RoomId);
                DataTable RightsTable = MySqlClient.ExecuteQueryTable("SELECT user_id FROM room_rights WHERE room_id = @id");

                foreach (DataRow Row in RightsTable.Rows)
                {
                    mUsersWithRights.Add((uint)Row["user_id"]);
                }

                // Tiles Trigger
                MySqlClient.SetParameter("id", RoomId);
                DataTable TriggersTable = MySqlClient.ExecuteQueryTable("SELECT * FROM room_triggers WHERE room_id = @id");

                foreach (DataRow Row in TriggersTable.Rows)
                {
                    RoomTriggerList Trigger = RoomTriggerList.ROLLER;
                    switch ((string)Row["action"])
                    {
                        case "roller":
                            Trigger = RoomTriggerList.ROLLER;
                            break;

                        case "teleport":
                            Trigger = RoomTriggerList.TELEPORT;
                            break;

                        case "infobusdoor":
                            Trigger = RoomTriggerList.INFOBUSDOOR;
                            break;
                    }

                    mRoomTriggers.Add(new RoomTriggers((uint)Row["id"], Vector3.FromString((string)Row["room_pos"]),
                        Trigger, Vector3.FromString((string)Row["to_room_pos"]), (uint)Row["to_room_id"],
                        (int)Row["to_room_dir"]));
                }

                // Pets
                MySqlClient.SetParameter("id", RoomId);
                DataTable PetsTable = MySqlClient.ExecuteQueryTable("SELECT * FROM user_pets WHERE room_id = @id");

                foreach (DataRow Row in PetsTable.Rows)
                {
                    Pet PetData = PetFactory.GetPetFromDatabaseRow(Row);

                    AddBotToRoom(BotManager.CreateNewInstance(BotManager.GetHandlerDefinitionForPetType(PetData.Type),
                        RoomId, PetData.RoomPosition, PetData));

                }

                // Room bans
                MySqlClient.SetParameter("id", RoomId);
                DataTable BansTable = MySqlClient.ExecuteQueryTable("SELECT * FROM room_bans WHERE room_id = @id");
                foreach (DataRow Row in BansTable.Rows)
                {
                    uint UserId = (uint)Row["user_id"];
                    double ExpireTimestamp = (double)Row["expire"];

                    if (ExpireTimestamp == 0 || UnixTimestamp.GetCurrent() < ExpireTimestamp)
                    {
                        if (mBannedUsers.ContainsKey(UserId))
                        {
                            mBannedUsers[UserId] = ExpireTimestamp;
                            continue;
                        }

                        mBannedUsers.Add(UserId, ExpireTimestamp);
                    }
                    else
                    {
                        mBannedUsers.Remove(UserId);
                    }
                }

                CleanDatabaseBans();
            }

            RegenerateRelativeHeightmap();
        }

        public static RoomInstance TryCreateRoomInstance(uint InstanceId, uint RoomId)
        {
            RoomInfo Info = RoomInfoLoader.GetRoomInfo(RoomId);

            if (Info == null)
            {
                return null;
            }

            RoomModel Model = Info.TryGetModel();

            if (Model == null)
            {
                return null;
            }

            return new RoomInstance(InstanceId, Info, Model);
        }
        public void Unload()
        {
            if (mUnloaded)
            {
                return;
            }

            mUnloaded = true;

            if (mActorCountSyncNeeded)
            {
                DoActorCountSync();
            }

            BroadcastMessage(RoomKickedComposer.Compose());

            lock (mActorSyncRoot)
            {
                mActors.Clear();
            }

            mUnloadedTimestamp = UnixTimestamp.GetCurrent();
        }

        public void Dispose()
        {
            if (!mUnloaded)
            {
                Unload();
            }

            mInfo = null;
        }
    }
}