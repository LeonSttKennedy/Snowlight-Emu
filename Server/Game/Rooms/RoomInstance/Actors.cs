using System;
using System.Collections.Generic;

using Snowlight.Game.Sessions;
using Snowlight.Communication;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Items;
using Snowlight.Specialized;
using Snowlight.Game.Bots;
using Snowlight.Storage;
using System.Collections.ObjectModel;

using Snowlight.Game.Rooms.Trading;
using Snowlight.Config;
using Snowlight.Util;
using Snowlight.Game.Pets;
using System.Collections.Concurrent;
using System.Linq;

namespace Snowlight.Game.Rooms
{
    public partial class RoomInstance : IDisposable
    {
        private object mActorSyncRoot;
        private Dictionary<uint, RoomActor> mActors;
        private uint mActorIdGenerator;
        private object mActorIdGeneratorSyncLock;
        private bool mActorCountSyncNeeded;
        private int mCachedNavigatorUserCount;
        private List<uint> mUsersWithRights;
        private Dictionary<uint, double> mBannedUsers;
        private int mPetCount;

        public int ActorCount
        {
            get
            {
                return mActors.Count;
            }
        }

        public int HumanActorCount
        {
            get
            {
                int c = 0;

                lock (mActors)
                {
                    foreach (RoomActor Actor in mActors.Values)
                    {
                        if (Actor.IsBot)
                        {
                            continue;
                        }

                        c++;
                    }
                }

                return c;
            }
        }

        public int PetActorCount
        {
            get
            {
                int c = 0;

                lock (mActors)
                {
                    foreach (RoomActor Actor in mActors.Values)
                    {
                        if (!Actor.IsBot)
                        {
                            continue;
                        }

                        if (Actor.IsBot && ((Bot)Actor.ReferenceObject).IsPet)
                        {
                            c++;
                        }
                    }
                }

                return c;
            }
        }

        public ReadOnlyCollection<uint> UsersWithRights
        {
            get
            {
                lock (mUsersWithRights)
                {
                    List<uint> Copy = new List<uint>();
                    Copy.AddRange(mUsersWithRights);
                    return Copy.AsReadOnly();
                }
            }
        }

        public int CachedNavigatorUserCount
        {
            get
            {
                return mCachedNavigatorUserCount;
            }
        }

        public ReadOnlyCollection<RoomActor> Actors
        {
            get
            {
                lock (mActors)
                {
                    List<RoomActor> Copy = new List<RoomActor>();

                    foreach (RoomActor Actor in mActors.Values)
                    {
                        Copy.Add(Actor);
                    }

                    return Copy.AsReadOnly();
                }
            }
        }

        public bool ActorCountDatabaseWritebackNeeded
        {
            get
            {
                return mActorCountSyncNeeded;
            }

            set
            {
                mActorCountSyncNeeded = value;
            }
        }

        private uint GenerateActorId()
        {
            lock (mActorIdGeneratorSyncLock)
            {
                if (mActorIdGenerator >= uint.MaxValue)
                {
                    return 0;
                }

                return mActorIdGenerator++;
            }
        }

        public bool AddUserToRoom(Session Session)
        {
            if (Session.AbsoluteRoomId != RoomId || !Session.Authenticated)
            {
                return false;
            }

            uint ActorId = GenerateActorId();

            if (ActorId == 0)
            {
                return false;
            }

            Vector3 StartPosition = new Vector3(Model.DoorPosition.X, Model.DoorPosition.Y, Model.DoorPosition.Z);
            int StartRotation = Model.DoorRotation;

            RoomActor NewActor = RoomActor.TryCreateActor(ActorId, RoomActorType.UserCharacter, Session.CharacterId,
                Session.CharacterInfo, StartPosition, StartRotation, this);

            Item TargetTeleporter = null;

            if (Session.IsTeleporting)
            {
                TargetTeleporter = GetItem(Session.TargetTeleporterId);
                RoomTriggers TriggerTeleport = RoomTriggers.GetTrigger(Session.TriggerTeleporterId);

                if (TargetTeleporter != null && !TargetTeleporter.TemporaryInteractionReferenceIds.ContainsKey(2))
                {
                    NewActor.Position = new Vector3(TargetTeleporter.RoomPosition.X, TargetTeleporter.RoomPosition.Y,
                        TargetTeleporter.RoomPosition.Z);
                    NewActor.HeadRotation = TargetTeleporter.RoomRotation;
                    NewActor.BodyRotation = TargetTeleporter.RoomRotation;
                    NewActor.UpdateNeeded = true;

                    TargetTeleporter.TemporaryInteractionReferenceIds.Add(2, NewActor.Id);
                    TargetTeleporter.DisplayFlags = "2";
                    TargetTeleporter.RequestUpdate(3);
                }

                if (TriggerTeleport != null)
                {
                    NewActor.Position = new Vector3(TriggerTeleport.ToRoomPosition.X, TriggerTeleport.ToRoomPosition.Y,
                        TriggerTeleport.ToRoomPosition.Z);
                    NewActor.HeadRotation = TriggerTeleport.ToRoomRotation;
                    NewActor.BodyRotation = TriggerTeleport.ToRoomRotation;
                    NewActor.UpdateNeeded = true;
                }

                Session.TriggerTeleporterId = 0;
                Session.TargetTeleporterId = 0;
                Session.IsTeleporting = false;
            }

            if (NewActor == null)
            {
                return false;
            }

            AddActorToRoom(NewActor);

            if (TargetTeleporter != null)
            {
                TargetTeleporter.BroadcastStateUpdate(this);
            }

            if (CheckUserRights(Session, true))
            {
                NewActor.SetStatus("flatctrl", "useradmin");
                Session.SendData(RoomOwnerRightsComposer.Compose());
                Session.SendData(RoomRightsComposer.Compose());
            }
            else if (CheckUserRights(Session))
            {
                NewActor.SetStatus("flatctrl");
                Session.SendData(RoomRightsComposer.Compose());
            }

            if (Session.CurrentEffect > 0)
            {
                NewActor.ApplyEffect(Session.CurrentEffect);
            }

            NewActor.UpdateNeeded = true;
            return true;
        }

        private bool AddActorToRoom(RoomActor Actor)
        {
            lock (mActorSyncRoot)
            {
                if (mActors.ContainsKey(Actor.Id))
                {
                    return false;
                }

                mActors.Add(Actor.Id, Actor);
                BroadcastMessage(RoomUserObjectListComposer.Compose(mActors.Values.ToList()));
                MarkActorCountSyncNeeded();

                foreach (RoomActor _Actor in mActors.Values)
                {
                    if (_Actor.Type == RoomActorType.AiBot)
                    {
                        if (_Actor.Id == Actor.Id)
                        {
                            ((Bot)_Actor.ReferenceObject).Brain.OnSelfEnterRoom(this);
                        }
                        else
                        {
                            ((Bot)_Actor.ReferenceObject).Brain.OnUserEnter(this, Actor);
                        }
                    }
                }

                if (Actor.Type == RoomActorType.UserCharacter)
                {
                    WiredManager.HandleEnterRoom(Actor);
                }

                return true;
            }
        }

        public void DoActorCountSync()
        {
            int NewActorCount = HumanActorCount;

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                MySqlClient.SetParameter("id", RoomId);
                MySqlClient.SetParameter("usercount", NewActorCount);
                MySqlClient.ExecuteNonQuery("UPDATE rooms SET current_users = @usercount WHERE id = @id LIMIT 1");
            }

            mCachedNavigatorUserCount = NewActorCount;
            mActorCountSyncNeeded = false;
        }

        public void MarkActorCountSyncNeeded()
        {
            mActorCountSyncNeeded = true;
        }

        public void BanUser(uint UserId, double ExpireTimestamp = 0)
        {
            lock (mActorSyncRoot)
            {
                RoomActor Actor = GetActorByReferenceId(UserId);

                if (Actor == null || Actor.Type != RoomActorType.UserCharacter)
                {
                    return;
                }

                Session ActorSession = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                if (ActorSession != null)
                {
                    if (CheckUserRights(ActorSession, true))
                    {
                        return; // this is the room owner or a moderator, no banning allowed on this guy!
                    }
                }

                AddBanToDatabase(UserId, ExpireTimestamp);
            }

            SoftKickUser(UserId, true);
        }

        public void SoftKickUser(uint UserId, bool Forced = false, bool NotifyUser = false, bool OverrideOwner = false)
        {
            RoomActor Actor = GetActorByReferenceId(UserId);

            if (Actor == null || Actor.Type != RoomActorType.UserCharacter)
            {
                return;
            }

            Session ActorSession = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

            if (ActorSession != null)
            {
                if (!OverrideOwner && CheckUserRights(ActorSession, true))
                {
                    return; // this is the room owner or a moderator, no kicking allowed!
                }

                if (NotifyUser)
                {
                    ActorSession.SendData(GenericErrorComposer.Compose(4008));
                }
            }

            Actor.LeaveRoom(Forced);
        }

        public void HardKickUser(uint UserId)
        {
            RoomActor Actor = GetActorByReferenceId(UserId);

            if (Actor == null || Actor.Type != RoomActorType.UserCharacter)
            {
                return;
            }

            Session ActorSession = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

            RoomManager.RemoveUserFromRoom(ActorSession, true);
            RemoveCharacterFromRoom(UserId);
        }

        public bool IsUserBanned(uint UserId)
        {
            lock (mActorSyncRoot)
            {
                if (!mBannedUsers.ContainsKey(UserId))
                {
                    return false;
                }

                double TimespanExpire = mBannedUsers[UserId];
                bool RemoveBan = TimespanExpire > 0 && UnixTimestamp.GetCurrent() > TimespanExpire;

                if (RemoveBan)
                {
                    mBannedUsers.Remove(UserId);
                    CleanDatabaseBans();
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Removes a human character from the room instance. DO NOT CALL DIRECTLY (only from RoomManager).
        /// </summary>
        /// <param name="CharacterId">Id of the character to remove.</param>
        /// <returns>Boolean based on success of removal.</returns>
        public bool RemoveCharacterFromRoom(uint CharacterId)
        {
            uint ActorId = 0;

            lock (mActorSyncRoot)
            {
                foreach (RoomActor Actor in mActors.Values)
                {
                    if (Actor.Type == RoomActorType.UserCharacter && Actor.ReferenceId == CharacterId)
                    {
                        ActorId = Actor.Id;
                        break;
                    }
                }
            }

            if (ActorId > 0)
            {
                return RemoveActorFromRoom(ActorId);
            }

            return false;
        }

        /// <summary>
        /// Removes an actor from the room instance. DO NOT CALL DIRECTLY FOR HUMAN CHARACTERS.
        /// </summary>
        /// <param name="ActorId">Id of the actor to remove.</param>
        /// <returns>Boolean based on success of removal.</returns>
        public bool RemoveActorFromRoom(uint ActorId)
        {
            bool Success = false;
            List<uint> ActorsToRemove = new List<uint>();

            lock (mActorSyncRoot)
            {
                RoomActor Actor = GetActor(ActorId);

                if (Actor == null)
                {
                    return false;
                }

                if (Actor.Type == RoomActorType.UserCharacter)
                {
                    if (Actor.ReferenceId == Info.OwnerId && HasOngoingEvent)
                    {
                        StopEvent();
                    }

                    Trade Trade = TradeManager.GetTradeForUser(Actor.ReferenceId);

                    if (Trade != null)
                    {
                        TradeManager.StopTradeForUser(Trade.UserOne);
                        TradeManager.StopTradeForUser(Trade.UserTwo);

                        Session TargetSession = SessionManager.GetSessionByCharacterId(Actor.ReferenceId ==
                            Trade.UserOne ? Trade.UserTwo : Trade.UserOne);

                        if (TargetSession != null)
                        {
                            TargetSession.SendData(TradeAbortedComposer.Compose(Actor.ReferenceId));

                            RoomActor TargetActor = GetActorByReferenceId(TargetSession.CharacterId);

                            if (TargetActor != null)
                            {
                                TargetActor.RemoveStatus("trd");
                                TargetActor.UpdateNeeded = true;
                            }
                        }
                    }

                    ActorsToRemove.Add(Actor.Id);
                }

                foreach (RoomActor _Actor in mActors.Values)
                {
                    if (_Actor.Type == RoomActorType.AiBot)
                    {
                        Bot SelfBot = ((Bot)_Actor.ReferenceObject);

                        if (_Actor.Id == Actor.Id)
                        {
                            ActorsToRemove.Add(_Actor.Id);
                            SelfBot.Brain.OnSelfLeaveRoom(this);

                            if (SelfBot.IsPet)
                            {
                                mPetCount--;
                            }
                        }
                        else
                        {
                            if(SelfBot.IsPet && SelfBot.PetData.OwnerId.Equals(Actor.ReferenceId) && Info.OwnerId != Actor.ReferenceId)
                            {
                                ActorsToRemove.Add(_Actor.Id);
                                SelfBot.Brain.OnUserLeave(this, Actor);
                                
                                if (SelfBot.IsPet)
                                {
                                    mPetCount--;
                                }
                            }
                        }
                    }
                }

                if(ActorsToRemove.Count > 0)
                {
                    foreach (uint _ActorId in ActorsToRemove)
                    {
                        Success = mActors.Remove(_ActorId);
                        if (Success)
                        {
                            BroadcastMessage(RoomUserRemovedComposer.Compose(_ActorId));
                            MarkActorCountSyncNeeded();
                        }
                    }
                }
            }

            return Success;
        }

        public RoomActor GetActor(uint ActorId)
        {
            lock (mActors)
            {
                if (mActors.ContainsKey(ActorId))
                {
                    return mActors[ActorId];
                }
            }

            return null;
        }

        public RoomActor GetActorByReferenceId(uint ReferenceId, RoomActorType ReferenceType = RoomActorType.UserCharacter)
        {
            lock (mActors)
            {
                foreach (RoomActor Actor in mActors.Values)
                {
                    if (Actor.Type == ReferenceType && Actor.ReferenceId == ReferenceId)
                    {
                        return Actor;
                    }
                }
            }

            return null;
        }

        public RoomActor GetPetByName(string Name)
        {
            lock (mActors)
            {
                foreach (RoomActor Actor in mActors.Values)
                {
                    if (!Actor.IsBot)
                    {
                        continue;
                    }
                    
                    Bot Bot = (Bot)Actor.ReferenceObject;
                    if(Bot.PetData != null && Bot.PetData.Name == Name)
                    {
                        return Actor;
                    }
                }
            }

            return null;
        }

        public bool AddBotToRoom(Bot Bot)
        {
            uint ActorId = GenerateActorId();

            RoomActor BotActor = RoomActor.TryCreateActor(ActorId, RoomActorType.AiBot, Bot.Id, Bot,
                new Vector3(Bot.InitialPosition.X, Bot.InitialPosition.Y, Bot.InitialPosition.Z), Bot.Rotation, this);
            
            Bot.Brain.Initialize(Bot);

            if (Bot.BehaviorType == "guide")
            {
                Info.GuideBotIsCalled = true;
            }

            if (BotActor != null)
            {
                AddActorToRoom(BotActor);

                if (Bot.IsPet)
                {
                    Bot.PetData.SetVirtualId(ActorId);
                    mPetCount++;
                }

                return true;
            }

            return false;
        }

        public void KickBot(uint ActorId)
        {
            RoomActor Actor = GetActor(ActorId);

            if (Actor == null || Actor.Type != RoomActorType.AiBot)
            {
                return;
            }

            Bot Bot = (Bot)Actor.ReferenceObject;

            if (Bot == null || !Bot.Kickable)
            {
                return;
            }

            RemoveActorFromRoom(ActorId);
        }

        public bool CanPlacePet(bool IsOwner)
        {
            if (mPetCount >= ServerSettings.MaxPetsPerRoom)
            {
                return false;
            }

            return mInfo.AllowPets || IsOwner;
        }

        public void AddBanToDatabase(uint UserId, double ExpireTimestamp)
        {
            if (mBannedUsers.ContainsKey(UserId))
            {
                mBannedUsers[UserId] = ExpireTimestamp;
                return;
            }

            mBannedUsers.Add(UserId, ExpireTimestamp);

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                MySqlClient.SetParameter("userid", UserId);
                MySqlClient.SetParameter("roomid", RoomId);
                MySqlClient.SetParameter("expiretimestamp", ExpireTimestamp);
                MySqlClient.ExecuteNonQuery("INSERT INTO room_bans (user_id, room_id, expire) VALUES (@userid,@roomid,@expiretimestamp)");
            }
        }

        public void CleanDatabaseBans()
        {
            using(SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient()) 
            {
                MySqlClient.SetParameter("id", RoomId);
                MySqlClient.SetParameter("currenttimestamp", UnixTimestamp.GetCurrent());
                MySqlClient.ExecuteNonQuery("DELETE FROM room_bans WHERE room_id = @id AND expire > 0 AND expire < @currenttimestamp");
            }
        }
    }
}
