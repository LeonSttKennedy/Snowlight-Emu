﻿using System;
using System.Text;
using System.Collections.Generic;

using Snowlight.Game.Sessions;
using Snowlight.Communication;
using Snowlight.Game.Advertisements;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Navigation;
using Snowlight.Util;
using Snowlight.Storage;

using Snowlight.Game.Characters;
using Snowlight.Specialized;
using Snowlight.Game.Messenger;
using Snowlight.Game.Pathfinding;
using Snowlight.Game.Misc;
using Snowlight.Game.Items;
using Snowlight.Game.Moderation;
using Snowlight.Game.Achievements;
using Snowlight.Game.Quests;
using Snowlight.Communication.Incoming;
using Snowlight.Game.Rights;
using Snowlight.Game.Groups;
using Snowlight.Game.FriendStream;

namespace Snowlight.Game.Rooms
{
    class RoomHandler
    {
        public static void Initialize()
        {
            // Room loading/processing
            DataRouter.RegisterHandler(OpcodesIn.ROOM_OPEN_PUBLIC, new ProcessRequestCallback(OpenPublicConnection));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_OPEN_FLAT, new ProcessRequestCallback(OpenFlatConnection));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_CREATE_CHECK, new ProcessRequestCallback(CheckCanCreateRoom));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_CREATE, new ProcessRequestCallback(OnCreateRoom));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_GET_CAMPAIGNS, new ProcessRequestCallback(GetFurniCampaigns));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_GET_PUB_DATA, new ProcessRequestCallback(GetPublicRoomData));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_GET_OBJECTS, new ProcessRequestCallback(GetRoomObjects));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_GET_INTERSTITIAL_ADVERTISEMENT, new ProcessRequestCallback(GetInterstitial));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_GET_ROOM_ADVERTISEMENT, new ProcessRequestCallback(GetAdvertisement));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_GET_INFO, new ProcessRequestCallback(GetRoomInfo));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_ANSWER_DOORBELL, new ProcessRequestCallback(AnswerDoorbell));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_CONTINUE_LOAD, new ProcessRequestCallback(ContinueLoadingAfterDoorbell));

            // Room options etc
            DataRouter.RegisterHandler(OpcodesIn.ROOM_EXIT, new ProcessRequestCallback(UserExit));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_SET_HOME, new ProcessRequestCallback(SetHome));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_EDIT_GET_INFO, new ProcessRequestCallback(GetEditInfo));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_EDIT_SUBMIT, new ProcessRequestCallback(EditRoom));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_SET_ICON, new ProcessRequestCallback(SetIcon));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_RATE, new ProcessRequestCallback(RateRoom));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_DELETE, new ProcessRequestCallback(DeleteRoom));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_APPLY_DECORATION, new ProcessRequestCallback(ApplyDecoration));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_ADD_TO_STAFF_PICKED, new ProcessRequestCallback(AddToStaffPicked));

            // Room interactions
            DataRouter.RegisterHandler(OpcodesIn.ROOM_MOVE_TO, new ProcessRequestCallback(UserMoveTo));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_WAVE, new ProcessRequestCallback(UserWave));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_DANCE, new ProcessRequestCallback(UserDance));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_GET_USER_BADGES, new ProcessRequestCallback(GetUserBadges));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_CHANGE_ROTATION, new ProcessRequestCallback(UserChangeRotation));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_KICK, new ProcessRequestCallback(KickUser));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_BAN, new ProcessRequestCallback(BanUser));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_KICK_BOT, new ProcessRequestCallback(KickBot));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_RIGHTS_GIVE, new ProcessRequestCallback(GiveRights));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_RIGHTS_TAKE, new ProcessRequestCallback(TakeRights));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_RIGHTS_TAKE_ALL, new ProcessRequestCallback(TakeAllRights));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_GET_USER_TAGS, new ProcessRequestCallback(GetUserTags));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_IGNORE, new ProcessRequestCallback(IgnoreUser));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_LISTEN, new ProcessRequestCallback(UnignoreUser));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_RESPECT, new ProcessRequestCallback(RespectUser));

            // Chat
            DataRouter.RegisterHandler(OpcodesIn.ROOM_CHAT_TALK, new ProcessRequestCallback(UserChat));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_CHAT_SHOUT, new ProcessRequestCallback(UserChat));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_CHAT_WHISPER, new ProcessRequestCallback(UserWhisper));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_CHAT_START_TYPING, new ProcessRequestCallback(TypingStateChanged));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_CHAT_STOP_TYPING, new ProcessRequestCallback(TypingStateChanged));

            // Events
            DataRouter.RegisterHandler(OpcodesIn.ROOM_EVENT_CHECK_CAN_CREATE, new ProcessRequestCallback(CanCreateEvent));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_EVENT_CREATE, new ProcessRequestCallback(CreateOrEditEvent));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_EVENT_EDIT, new ProcessRequestCallback(CreateOrEditEvent));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_EVENT_STOP, new ProcessRequestCallback(StopEvent));

            // Furni
            DataRouter.RegisterHandler(OpcodesIn.ITEM_ACTIVATE_GENERIC_FLOOR, new ProcessRequestCallback(ActivateGeneric));
            DataRouter.RegisterHandler(OpcodesIn.ITEM_ACTIVATE_GENERIC_WALL, new ProcessRequestCallback(ActivateGeneric));
            DataRouter.RegisterHandler(OpcodesIn.ITEM_ACTIVATE_DICE_1, new ProcessRequestCallback(ActivateGeneric));
            DataRouter.RegisterHandler(OpcodesIn.ITEM_ACTIVATE_DICE_2, new ProcessRequestCallback(ActivateGeneric));
            DataRouter.RegisterHandler(OpcodesIn.ITEM_ACTIVATE_EXCHANGE, new ProcessRequestCallback(ActivateGeneric));
            DataRouter.RegisterHandler(OpcodesIn.ITEM_ACTIVATE_HABBO_WHEEL, new ProcessRequestCallback(ActivateGeneric));
            DataRouter.RegisterHandler(OpcodesIn.ITEM_ACTIVATE_LOVE_SHUFFLER, new ProcessRequestCallback(ActivateGeneric));
            DataRouter.RegisterHandler(OpcodesIn.ITEM_ACTIVATE_ONE_WAY_GATE, new ProcessRequestCallback(ActivateGeneric));
            DataRouter.RegisterHandler(OpcodesIn.SYNC_MUSIC, new ProcessRequestCallback(SyncSoundManager));

            // Groups
            DataRouter.RegisterHandler(OpcodesIn.ROOM_GET_GROUP_BADGES, new ProcessRequestCallback(GetGroupBadges));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_GROUP_INFO, new ProcessRequestCallback(GetGroupInfo));
            DataRouter.RegisterHandler(OpcodesIn.JOIN_GROUP, new ProcessRequestCallback(JoinGroup));
            DataRouter.RegisterHandler(OpcodesIn.SET_GROUP_FAVORITE, new ProcessRequestCallback(AddFavoriteGroup));
            DataRouter.RegisterHandler(OpcodesIn.REMOVE_GROUP_FAVORITE, new ProcessRequestCallback(RemoveFavoriteGroup));
            DataRouter.RegisterHandler(OpcodesIn.LOAD_USER_GROUPS, new ProcessRequestCallback(LoadUserGroups));

            // Change Username
            DataRouter.RegisterHandler(OpcodesIn.SAVE_SELECTED_USERNAME, new ProcessRequestCallback(SaveSelectedName));
            DataRouter.RegisterHandler(OpcodesIn.CHECK_ENTERED_USERNAME, new ProcessRequestCallback(CheckIfNameIsValid));

        }
        private static void SaveSelectedName(Session Session, ClientMessage Message)
        {
            string NewName = UserInputFilter.FilterString(Message.PopString());

            ChangeNameErrorCode ErrorCode = ChangeNameErrorCode.Allowed;

            if (!Session.CharacterInfo.AllowChangeName)
            {
                ErrorCode = ChangeNameErrorCode.Change_Not_Allowed;
            }

            if (NewName.Length < 3)
            {
                ErrorCode = ChangeNameErrorCode.Username_Short;
            }

            if (NewName.Length > 15)
            {
                ErrorCode = ChangeNameErrorCode.Username_Long;
            }

            if (NewName.Contains(" ") || ChatWordFilter.BlockedWords.Contains(NewName.ToLower()))
            {
                ErrorCode = ChangeNameErrorCode.Invalid;
            }

            if (CharacterResolverCache.GetUidFromName(NewName) > 0)
            {
                ErrorCode = ChangeNameErrorCode.Taken;
            }

            Session.SendData(CheckUsernameResultComposer.Compose(ErrorCode, NewName));

            RoomActor Actor = null;

            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);
            if (Instance != null)
            {
                Actor = Instance.GetActorByReferenceId(Session.CharacterId);
                Instance.BroadcastMessage(RoomUserObjectListComposer.Compose(new List<RoomActor>() { Actor }));
                Instance.BroadcastMessage(ChangeUsernameResultComposer.Compose(ErrorCode));
                Instance.BroadcastMessage(SendNewUsernameComposer.Compose(Session.CharacterId, Actor.Id, NewName));
            }

            Session.SendData(ChangeUsernameResultComposer.Compose(ErrorCode));
            Session.SendData(SendNewUsernameComposer.Compose(Session.CharacterId, Actor.Id, NewName));

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                Session.CharacterInfo.UpdateUsername(MySqlClient, NewName);
                Session.CharacterInfo.UpdateLastNameChange(MySqlClient);
                AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_Name", 1);
            }

            CharacterResolverCache.AddToCache(Session.CharacterId, NewName, true);
        }

        private static void CheckIfNameIsValid(Session Session, ClientMessage Message)
        {
            string NewName = UserInputFilter.FilterString(Message.PopString());

            ChangeNameErrorCode ErrorCode = ChangeNameErrorCode.Allowed;

            if (!Session.CharacterInfo.AllowChangeName)
            {
                ErrorCode = ChangeNameErrorCode.Change_Not_Allowed;
            }

            if (NewName.Length < 3)
            {
                ErrorCode = ChangeNameErrorCode.Username_Short;
            }

            if (NewName.Length > 15)
            {
                ErrorCode = ChangeNameErrorCode.Username_Long;
            }

            if(NewName.Contains(" ") || ChatWordFilter.BlockedWords.Contains(NewName.ToLower()))
            {
                ErrorCode = ChangeNameErrorCode.Invalid;
            }

            if(CharacterResolverCache.GetUidFromName(NewName) > 0)
            {
                ErrorCode = ChangeNameErrorCode.Taken;
            }

            Session.SendData(CheckUsernameResultComposer.Compose(ErrorCode, NewName));
        }

        private static void CheckCanCreateRoom(Session Session, ClientMessage Message)
        {
            Session.SendData(RoomCanCreateResult.Compose(Session.CharacterInfo.GetRoomCount() >= ServerSettings.MaxRoomsPerUser, ServerSettings.MaxRoomsPerUser));
        }
        
        private static void OnCreateRoom(Session Session, ClientMessage Message)
        {
            string Name = UserInputFilter.FilterString(Message.PopString());
            string ModelName = Message.PopString().ToLower();

            if (Name.Length < 3)
            {
                return;
            }

            RoomModel Model = RoomManager.GetModel(ModelName);
            
            if (Model == null || !Model.IsUsableBySession(Session) || Session.CharacterInfo.GetRoomCount() >= ServerSettings.MaxRoomsPerUser)
            {
                return;
            }

            uint RoomId = RoomManager.CreateRoom(Session.CharacterId, Name, ModelName);

            if (RoomId > 0)
            {
                Session.SendData(RoomCreateResultComposer.Compose(RoomId, Name));
            }
        }

        private static void GetPublicRoomData(Session Session, ClientMessage Message)
        {
            uint RoomId = Message.PopWiredUInt32();
            RoomInfo Info = RoomInfoLoader.GetRoomInfo(RoomId);

            if (Info == null) return;

            Session.SendData(PublicRoomDataComposer.Compose(Info.Id, Info.SWFs));
        }

        private static void OpenPublicConnection(Session Session, ClientMessage Message)
        {
            bool UnknownData1 = Message.PopWiredBoolean();
            uint RoomId = Message.PopWiredUInt32();
            bool UnknownData2 = Message.PopWiredBoolean();

            PrepareRoom(Session, RoomId, string.Empty);
        }

        private static void OpenFlatConnection(Session Session, ClientMessage Message)
        {
            uint RoomId = Message.PopWiredUInt32();
            string Password = Message.PopString();

            PrepareRoom(Session, RoomId, Password);
        }

        public static void PrepareRoom(Session Session, uint RoomId, string Password = "", bool BypassAuthentication = false)
        {
            // Remove user from any previous room
            RoomManager.RemoveUserFromRoom(Session, false);

            // Try to retrieve room information
            RoomInfo Info = RoomInfoLoader.GetRoomInfo(RoomId);

            // Room not found, send kick notif and stop
            if (Info == null)
            {
                Session.SendData(RoomKickedComposer.Compose());
                return;
            }

            // Try to retrieve room model information
            RoomModel Model = Info.TryGetModel();

            // Model information not found, send kick notif and stop
            if (Model == null)
            {
                Session.SendData(RoomKickedComposer.Compose());
                return;
            }

            // Load room instance into the server memory if needed
            if (!RoomManager.InstanceIsLoadedForRoom(Info.Id))
            {
                RoomManager.TryLoadRoomInstance(Info.Id);
            }

            // Attempt to retrieve the instance from the server memory
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Info.Id);

            // If the instance was not created or is unavailable, send kick notif and stop
            if (Instance == null)
            {
                Session.SendData(RoomKickedComposer.Compose());
                return;
            }

            // Check if the room capacity has been reached
            if (Info.CurrentUsers >= Info.MaxUsers && !Session.HasRight("enter_full_rooms"))
            {
                Session.SendData(RoomJoinErrorComposer.Compose(RoomJoinErrorCode.RoomFull));
                Session.SendData(RoomKickedComposer.Compose());
                return;
            }

            /*if (Info.ClubRoom && !Session.HasRight("club_regular"))
            {
                Session.SendData(RoomJoinErrorComposer.Compose(RoomJoinErrorCode.RoomJoinFailedQueueString, "c"));
                Session.SendData(RoomKickedComposer.Compose());
                return;
            }*/

            // Check if the user has been banned from this room
            if (Instance.IsUserBanned(Session.CharacterId))
            {
                Session.SendData(RoomJoinErrorComposer.Compose(RoomJoinErrorCode.RoomBanned));
                Session.SendData(RoomKickedComposer.Compose());
                return;
            }

            // Mark room as loading and check for initial authorization
            Session.AbsoluteRoomId = Info.Id;
            Session.RoomAuthed = (BypassAuthentication || Info.OwnerId == Session.CharacterId ||
                Session.HasRight("enter_locked_rooms"));
            Session.RoomJoined = false;

            // Send confirmation that the initial checks have been passed (if this is a flat)
            if (Info.Type == RoomType.Flat)
            {
                Session.SendData(RoomOpenFlatComposer.Compose());
            }

            // Try to accomplish authorization (if not already initially authorized by character rights)
            if (!Session.RoomAuthed)
            {
                // Check for valid password, if needed
                if (Info.AccessType == RoomAccessType.PasswordProtected)
                {
                    if (Info.Password != Password)
                    {
                        Session.SendData(GenericErrorComposer.Compose(-100002));
                        RoomManager.RemoveUserFromRoom(Session);
                        return;
                    }
                }
                // Send doorbell, if any users are in this room.
                else if (Info.AccessType == RoomAccessType.Locked)
                {
                    if (Instance.HumanActorCount > 0)
                    {
                        Session.SendData(RoomDoorbellComposer.Compose());
                        Instance.BroadcastMessage(RoomDoorbellComposer.Compose(Session.CharacterInfo.Username), true);
                        return;
                    }
                    else
                    {
                        Session.SendData(RoomDoorbellNoResponseComposer.Compose());
                        RoomManager.RemoveUserFromRoom(Session);
                        return;
                    }
                }
            }

            // If all these stages have been passed, mark auth as OK and continue loading
            Session.RoomAuthed = true;
            EnterRoom(Session, Instance);
        }

        public static void EnterRoom(Session Session, RoomInstance Instance)
        {
            if (!Session.RoomAuthed || Session.RoomJoined || Session.AbsoluteRoomId != Instance.RoomId)
            {
                return;
            }

            //Session.SendData(RoomUrlComposer.Compose("/client/public/" + Instance.Info.ModelName + "/0"));
            Session.SendData(RoomEntryModelComposer.Compose(Instance.Model.Name, Instance.Info.Id));

            if (Instance.Info.Type == RoomType.Flat)
            {
                Dictionary<string, string> Decorations = Instance.Info.Decorations;

                foreach (KeyValuePair<string, string> Decoration in Decorations)
                {
                    Session.SendData(RoomDecorationComposer.Compose(Decoration.Key, Decoration.Value));
                }

                Session.SendData(RoomRatingInfoComposer.Compose((Session.RatedRoomsCache.HasRatedRoom(Instance.RoomId) || Instance.CheckUserRights(Session, true)) ? Instance.Info.Score : -1));
                Session.SendData(RoomEventInfoComposer.Compose(Instance.Event));
            }
        }

        #region Groups
        private static void GetGroupBadges(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.AbsoluteRoomId);

            if (Instance == null)
            {
                return;
            }

            Group UserGroup = GroupManager.GetGroup(Session.CharacterInfo.FavoriteGroupId);

            Session.SendData(RoomGroupBadgeComposer.Compose(Session.CharacterId, UserGroup));
        }
        private static void JoinGroup(Session Session, ClientMessage Message)
        {
            Group GroupData = GroupManager.GetGroup(Message.PopWiredInt32());
            if(GroupData != null && !GroupData.HasPendingRequestByUserId(Session.CharacterId) 
                && !GroupData.MembershipList.Contains(Session.CharacterId))
            {
                GroupData.AddMemberInDatabase(Session.CharacterId);
                bool IsFavorite = Session.CharacterInfo.FavoriteGroupId == GroupData.Id;

                Session.SendData(GroupInfoComposer.Compose(GroupData, Session.CharacterId, IsFavorite));
            }
        }
        private static void GetGroupInfo(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.AbsoluteRoomId);

            if (Instance == null)
            {
                return;
            }

            Group UserGroup = GroupManager.GetGroup(Message.PopWiredInt32());
            bool IsFavorite = Session.CharacterInfo.FavoriteGroupId == UserGroup.Id;

            Session.SendData(GroupInfoComposer.Compose(UserGroup, Session.CharacterId, IsFavorite));
        }
        private static void LoadUserGroups(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.AbsoluteRoomId);

            if (Instance == null)
            {
                return;
            }

            Session.SendData(UserGroupsComposer.Compose(Session.CharacterInfo));
        }
        public static void AddFavoriteGroup(Session Session, ClientMessage Message)
        {
            int GroupId = Message.PopWiredInt32();
            if (GroupId > 0)
            {
                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    Session.CharacterInfo.UpdateFavoriteGroup(MySqlClient, GroupId);

                    RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);
                    if (Instance != null)
                    {
                        RoomActor Actor = Instance.GetActorByReferenceId(Session.CharacterId);
                        Instance.BroadcastMessage(RoomUserObjectListComposer.Compose(new List<RoomActor>() { Actor }));
                    }

                    Group UserGroup = GroupManager.GetGroup(Session.CharacterInfo.FavoriteGroupId);

                    Session.SendData(RoomGroupBadgeComposer.Compose(Session.CharacterId, UserGroup));
                    Session.SendData(UserGroupsComposer.Compose(Session.CharacterInfo));
                }
            }
        }
        public static void RemoveFavoriteGroup(Session Session, ClientMessage Message)
        {
            int GroupId = Message.PopWiredInt32();
            if (GroupId > 0)
            {
                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    Session.CharacterInfo.UpdateFavoriteGroup(MySqlClient, 0);
                    
                    RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);
                    if (Instance != null)
                    {
                        RoomActor Actor = Instance.GetActorByReferenceId(Session.CharacterId);
                        Instance.BroadcastMessage(RoomUserObjectListComposer.Compose(new List<RoomActor>() { Actor }));
                    }

                    Session.SendData(UserGroupsComposer.Compose(Session.CharacterInfo));
                }
            }
        }
        #endregion

        private static void GetFurniCampaigns(Session Session, ClientMessage Message)
        {
            ServerMessage FurniCampaigns = new ServerMessage(OpcodesOut.ROOM_FURNI_CAMPAIGNS);
            FurniCampaigns.AppendInt32(0);
            Session.SendData(FurniCampaigns);
        }

        private static void GetRoomObjects(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.AbsoluteRoomId);

            if (Instance == null || Session.RoomJoined || !Session.RoomAuthed) // if instance not found, or user already joined us, OR if the user isn't authed in the first place, let's gtfo
            {
                return;
            }
            
            Instance.SendObjects(Session);

            if (!Instance.AddUserToRoom(Session))
            {
                RoomManager.RemoveUserFromRoom(Session);
                return;
            }

            Session.RoomAuthed = true;
            Session.RoomJoined = true;
            Session.RoomJoinedTimestamp = UnixTimestamp.GetCurrent();

            ModerationLogs.LogRoomEntry(Session.CharacterId, Instance.RoomId);
            MessengerHandler.MarkUpdateNeeded(Session, 0, false);

            Session.SendData(RoomWallsStatusComposer.Compose(Instance.Info.HideWalls, Instance.Info.WallThickness, Instance.Info.FloorThickness));
            Session.SendData(RoomInfoRightsComposer.Compose(Instance.Info.Type == RoomType.Flat, Instance.RoomId,
                (Instance.Info.Type == RoomType.Flat && Instance.CheckUserRights(Session, true)), Instance.Info.PubInternalName,
                Instance.Info.PubInternalId));

            /*if (Instance.Info.Type == RoomType.Flat)
            {
                Session.SendData(RoomInfoComposer.Compose(Instance.Info, true));
            }*/

            if (Session.CharacterInfo.IsMuted)
            {
                Session.SendData(RoomMutedComposer.Compose((int)Session.CharacterInfo.MutedSecondsLeft));
            }

            if (Instance.Info.OwnerId != Session.CharacterId)
            {
                QuestManager.ProgressUserQuest(Session, QuestType.SOCIAL_VISIT);
            }

            if (Session.QuestCache.CurrentQuestId > 0)
            {
                Quest Quest = QuestManager.GetQuest(Session.QuestCache.CurrentQuestId);

                if (Quest != null)
                {
                    Session.SendData(QuestStartedComposer.Compose(Session, Quest));
                }
            }

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                if (Instance.Info.BadgeCode != string.Empty)
                {
                    RoomActor Actor = Instance.GetActorByReferenceId(Session.CharacterId);
                    if (Actor == null)
                    {
                        return;
                    }

                    BadgeDefinition BadgeToGive = RightsManager.GetBadgeDefinitionByCode(Instance.Info.BadgeCode);
                    if (BadgeToGive == null)
                    {
                        return;
                    }

                    if (!Session.BadgeCache.ContainsCode(Instance.Info.BadgeCode))
                    {
                        Session.BadgeCache.UpdateAchievementBadge(MySqlClient, BadgeToGive.Code, BadgeToGive, Session.AchievementCache, "static");

                        InventoryBadge UserBadge = Session.BadgeCache.GetBadge(Instance.Info.BadgeCode);
                        Session.NewItemsCache.MarkNewItem(MySqlClient, NewItemsCategory.Badges, UserBadge.Id);
                        Session.NewItemsCache.SendNewItems(Session);

                        Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("onenter_room_win_badge_success"), 1, ChatType.Whisper));
                    }
                    else if (Session.BadgeCache.ContainsCode(Instance.Info.BadgeCode))
                    {
                        Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("onenter_room_win_badge_error"), 4, ChatType.Whisper));
                    }
                }

                if (Instance.Info.OwnerId != Session.CharacterId && Instance.Info.Type != RoomType.Public)
                {
                    AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_RoomEntry", 1);
                }

                RoomPoll Poll = RoomPollManager.GetRoomPoll(Instance.Info.Id);
                if (Poll != null && !Session.CharacterInfo.FilledPolls.Contains(Poll.Id))
                {
                    Session.SendData(RoomPollRequestComposer.Compose(Poll));
                }
            }
        }

        private static void GetInterstitial(Session Session, ClientMessage Message)
        {
            Session.SendData(RoomInterstitialComposer.Compose(InterstitialManager.GetRandomInterstitial(true)));
        }

        private static void GetAdvertisement(Session Session, ClientMessage Message)
        {
            RoomInfo Info = RoomInfoLoader.GetRoomInfo(Session.CurrentRoomId);

            if (Info == null) 
            {
                return; 
            }

            Interstitial RoomAds = InterstitialManager.GetRoomAdsForRoomId(Info.Id);
            if (RoomAds != null && Info.Type == RoomType.Public)
            {
                Session.SendData(PublicRoomAdvertisement.Compose(RoomAds));
            }
        }

        private static void GetRoomInfo(Session Session, ClientMessage Message)
        {
            RoomInfo Info = RoomInfoLoader.GetRoomInfo(Message.PopWiredUInt32());

            if (Info == null)
            {
                return;
            }

            bool Bool1 = Message.PopWiredBoolean(); // true when entering a room, otherwise always false
            bool Bool2 = Message.PopWiredBoolean(); // true when requesting info before entering (stalking etc), otherwise always false??

            Session.SendData(RoomInfoComposer.Compose(Info, Bool1, Bool2));
        }

        private static void UserExit(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null)
            {
                return;
            }

            RoomActor Actor = Instance.GetActorByReferenceId(Session.CharacterId);

            if (Actor == null)
            {
                return;
            }

            Actor.LeaveRoom();
        }
        private static void UserMoveTo(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null)
            {
                return;
            }

            RoomActor Actor = Instance.GetActorByReferenceId(Session.CharacterId);

            if (Actor == null)
            {
                return;
            }

            int RequestX = Message.PopWiredInt32();
            int RequestY = Message.PopWiredInt32();

            if (RequestX < 0 || RequestY < 0)
            {
                return;
            }

            Actor.MoveTo(new Vector2(RequestX, RequestY));
        }

        private static void UserWave(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null)
            {
                return;
            }

            RoomActor Actor = Instance.GetActorByReferenceId(Session.CharacterId);

            if (Actor == null || Actor.AvatarEffectId > 0)
            {
                return;
            }

            Actor.Wave();
            QuestManager.ProgressUserQuest(Session, QuestType.SOCIAL_WAVE);
        }

        private static void UserDance(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null)
            {
                return;
            }

            RoomActor Actor = Instance.GetActorByReferenceId(Session.CharacterId);

            if (Actor == null || Actor.AvatarEffectId > 0)
            {
                return;
            }

            int DanceId = Message.PopWiredInt32();

            if (DanceId < 0 || DanceId > 4)
            {
                return;
            }

            if (!Session.HasRight("club_regular") && !Session.HasRight("club_vip") && DanceId != 0)
            {
                DanceId = 1;
            }

            Actor.Dance(DanceId);

            QuestManager.ProgressUserQuest(Session, QuestType.SOCIAL_DANCE);
        }

        private static void GetUserBadges(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null)
            {
                return;
            }

            Session TargetSession = SessionManager.GetSessionByCharacterId(Message.PopWiredUInt32());

            if (TargetSession == null)
            {
                return;
            }

            Session.SendData(RoomUserBadgesComposer.Compose(TargetSession.CharacterId,
                TargetSession.BadgeCache.EquippedBadges));
        }

        private static void UserChat(Session Session, ClientMessage Message)
        {
            if (Session.CharacterInfo.IsMuted)
            {
                return;
            }

            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null)
            {
                return;
            }

            RoomActor Actor = Instance.GetActorByReferenceId(Session.CharacterId);

            if (Actor == null)
            {
                return;
            }

            bool Shout = (Message.Id == OpcodesIn.ROOM_CHAT_SHOUT);
            string MessageText = ChatWordFilter.CheckWords(UserInputFilter.FilterString(Message.PopString()), Session);

            bool done = Instance.WiredManager.HandleChat(MessageText, Actor);

            if (MessageText.Length == 0)
            {
                return;
            }

            if (MessageText.Length > 100)
            {
                MessageText = MessageText.Substring(0, 100);
            }

            /*if (MessageText.StartsWith(":") && (ChatCommands.HandleCommand(Session, MessageText.Substring(1)) ||
                Session.HasRight("moderation_tool")))*/

            if (MessageText.StartsWith(":") && CommandManager.HandleCommand(Session, MessageText))
            {
                return;
            }

            if (!done)
            {
                Actor.Chat(MessageText, Shout, Session.HasRight("mute"));
            }
            else
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, MessageText, 0, ChatType.Whisper));
            }

            if (Instance.HumanActorCount > 1)
            {
                QuestManager.ProgressUserQuest(Session, QuestType.SOCIAL_CHAT);
            }
        }

        private static void UserWhisper(Session Session, ClientMessage Message)
        {
            if (Session.CharacterInfo.IsMuted)
            {
                return;
            }

            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null)
            {
                return;
            }

            RoomActor Actor = Instance.GetActorByReferenceId(Session.CharacterId);

            if (Actor == null)
            {
                return;
            }

            string MessageText = ChatWordFilter.CheckWords(UserInputFilter.FilterString(Message.PopString().Trim()), Session);

            if (MessageText.Length == 0)
            {
                return;
            }            

            if (MessageText.Length > 100)
            {
                MessageText = MessageText.Substring(0, 100);
            }

            string[] Bits = MessageText.Split(' ');

            if (Bits.Length < 2)
            {
                return;
            }

            string UserBit = Bits[0];
            MessageText = MessageText.Substring(UserBit.Length + 1);

            uint UserId = CharacterResolverCache.GetUidFromName(UserBit);

            if (UserId > 0)
            {
                Actor.Whisper(MessageText, UserId);
            }
        }

        private static void TypingStateChanged(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null)
            {
                return;
            }

            RoomActor Actor = Instance.GetActorByReferenceId(Session.CharacterId);

            if (Actor == null)
            {
                return;
            }

            Actor.SetTypingState((Message.Id == OpcodesIn.ROOM_CHAT_START_TYPING));
        }

        private static void UserChangeRotation(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null)
            {
                return;
            }

            RoomActor Actor = Instance.GetActorByReferenceId(Session.CharacterId);

            if (Actor == null || Actor.IsMoving)
            {
                return;
            }

            Vector2 PositionToFace = new Vector2(Message.PopWiredInt32(), Message.PopWiredInt32());

            if ((Actor.Position.X == PositionToFace.X && Actor.Position.Y == PositionToFace.Y) ||
                Actor.UserStatusses.ContainsKey("lay") || Actor.UserStatusses.ContainsKey("sit"))
            {
                return;
            }

            int NewRotation = Rotation.Calculate(Actor.Position.GetVector2(), PositionToFace, Actor.MoonWalkEnabled);

            if (Actor.BodyRotation != NewRotation)
            {
                Actor.BodyRotation = NewRotation;
                Actor.UpdateNeeded = true;
            }

            if (Actor.HeadRotation != NewRotation)
            {
                Actor.HeadRotation = NewRotation;
                Actor.UpdateNeeded = true;
            }
        }

        private static void SetHome(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session, true))
            {
                return;
            }

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                Session.CharacterInfo.SetHomeRoom(MySqlClient, Instance.RoomId);
            }

            Session.SendData(UserHomeRoomComposer.Compose(Session.CharacterInfo.HomeRoom));
        }

        private static void GetEditInfo(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session, true))
            {
                return;
            }

            Session.SendData(RoomEditInfoComposer.Compose(Instance));
        }

        private static void EditRoom(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session, true))
            {
                return;
            }

            // FQJ@LRoy's Office@fThis is where I handle business. Yeah.J@@RBIJ@Foffice@IbuttsechsAAAA

            uint Id = Message.PopWiredUInt32();

            if (Id != Instance.RoomId)
            {
                return;
            }

            string Name = UserInputFilter.FilterString(Message.PopString()).Trim();
            string Description = UserInputFilter.FilterString(Message.PopString()).Trim();
            RoomAccessType AccessType = (RoomAccessType)Message.PopWiredInt32();
            string Password = UserInputFilter.FilterString(Message.PopString()).Trim();
            int UserLimit = Message.PopWiredInt32();
            int CategoryId = Message.PopWiredInt32();
            int TagCount = Message.PopWiredInt32();

            List<string> Tags = new List<string>();

            for (int i = 0; (i < TagCount && i < 2); i++)
            {
                string Tag = UserInputFilter.FilterString(Message.PopString()).Trim().ToLower();

                if (Tag.Length > 32)
                {
                    Tag = Tag.Substring(0, 32);
                }

                if (Tag.Length > 0 && !Tags.Contains(Tag))
                {
                    Tags.Add(Tag);
                }
            }

            bool AllowPets = (Message.ReadBytes(1)[0] == 65);
            bool AllowPetEating = (Message.ReadBytes(1)[0] == 65);
            bool AllowBlocking = (Message.ReadBytes(1)[0] == 65);
            bool HideWalls = (Message.ReadBytes(1)[0] == 65);
            int WallThickness = Message.PopWiredInt32();
            int FloorThickness = Message.PopWiredInt32();

            if (WallThickness < -2 || WallThickness > 1)
            {
                WallThickness = 0;
            }

            if (FloorThickness < -2 || FloorThickness > 1)
            {
                FloorThickness = 0;
            }

            if (HideWalls && !Session.HasRight("club_vip"))
            {
                HideWalls = false;
            }

            if (Name.Length > 60) // was 25
            {
                Name = Name.Substring(0, 60);
            }

            if (Description.Length > 128)
            {
                Description = Description.Substring(0, 128);
            }

            if (Password.Length > 64)
            {
                Password = Password.Substring(0, 64);
            }

            if (UserLimit > Instance.Model.MaxUsers)
            {
                UserLimit = Instance.Model.MaxUsers;
            }

            if (Name.Length == 0)
            {
                Name = "Room";
            }

            if (AccessType == RoomAccessType.PasswordProtected && Password.Length == 0)
            {
                AccessType = RoomAccessType.Open;
            }

            FlatCategory SelectedCategory = Navigator.GetCategory((uint)CategoryId);
            if(SelectedCategory.RequiredRight.Length > 0 && !Session.HasRight(SelectedCategory.RequiredRight))
            {
                FlatCategory DefaultCategory = Navigator.GetDefaultCategory();

                CategoryId = DefaultCategory.Id;
                
                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("selected_category_min_rank", new string[] { SelectedCategory.Title,
                    DefaultCategory.Title })));
            }

            Instance.Info.EditRoom(Name, Description, AccessType, Password, UserLimit, CategoryId, Tags, AllowPets,
                AllowPetEating, AllowBlocking, HideWalls, WallThickness, FloorThickness);

            Session.SendData(RoomUpdatedNotification1Composer.Compose(Instance.RoomId));
            Instance.BroadcastMessage(RoomUpdatedNotification2Composer.Compose(Instance.RoomId));
            Instance.BroadcastMessage(RoomWallsStatusComposer.Compose(Instance.Info.HideWalls, Instance.Info.WallThickness,
                Instance.Info.FloorThickness));
            //Instance.BroadcastMessage(RoomInfoComposer.Compose(Instance.Info, false));

            if (Instance.Info.AccessType != RoomAccessType.Open && Instance.HasOngoingEvent)
            {
                Instance.StopEvent();
            }
        }

        private static void KickUser(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session))
            {
                return;
            }

            Instance.SoftKickUser(Message.PopWiredUInt32(), true, true);
        }

        private static void BanUser(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session, true))
            {
                return;
            }

            Instance.BanUser(Message.PopWiredUInt32());
        }

        private static void KickBot(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session, true))
            {
                return;
            }

            Instance.KickBot(Message.PopWiredUInt32());
        }

        private static void GiveRights(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session, true))
            {
                return;
            }

            uint UserId = Message.PopWiredUInt32();

            if (Instance.GiveUserRights(UserId))
            {
                Session.SendData(RoomRightsGivenConfirmationComposer.Compose(Instance.RoomId, UserId, CharacterResolverCache.GetNameFromUid(UserId)));
            }
        }

        private static void TakeRights(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session, true))
            {
                return;
            }

            int Amount = Message.PopWiredInt32();

            for (int i = 0; (i < Amount && i <= 100); i++)
            {
                uint UserId = Message.PopWiredUInt32();

                if (UserId > 0 && Instance.TakeUserRights(UserId))
                {
                    Session.SendData(RoomRightsRemovedConfirmationComposer.Compose(Instance.RoomId, UserId));
                }
            }
        }
        
        private static void TakeAllRights(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session, true))
            {
                return;
            }

            foreach (uint UserId in Instance.UsersWithRights)
            {
                if (Instance.TakeUserRights(UserId))
                {
                    Session.SendData(RoomRightsRemovedConfirmationComposer.Compose(Instance.RoomId, UserId));
                }                
            }
        }

        private static void CanCreateEvent(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null)
            {
                return;
            }

            RoomCanCreateEventError ErrorCode = RoomCanCreateEventError.CanCreate;

            if(!Session.InRoom)
            {
                ErrorCode = RoomCanCreateEventError.NeedToBeInaRoom;
            }

            if(!Instance.CheckUserRights(Session, true))
            {
                ErrorCode = RoomCanCreateEventError.OnlyRoomOwner;
            }

            if (Instance.Info.AccessType != RoomAccessType.Open)
            {
                ErrorCode = RoomCanCreateEventError.DoorLocked;
            }

            if(!ServerSettings.RoomEventsEnabled)
            {
                ErrorCode = RoomCanCreateEventError.EventsAreDisabled;
            }

            if (Instance.HasOngoingEvent)
            {
                ErrorCode = RoomCanCreateEventError.AlreadyCreated;
            }

            if(Navigator.UserHasRoomWithEvent(Instance.Info.OwnerId) > 0)
            {
                ErrorCode = RoomCanCreateEventError.AlreadyCreatedInAnotherRoom;
            }

            Session.SendData(RoomCanCreateEventComposer.Compose(ErrorCode));
        }

        private static void CreateOrEditEvent(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session, true) || Instance.Info.AccessType != RoomAccessType.Open)
            {
                return;
            }

            int CategoryId = Message.PopWiredInt32();
            string Name = UserInputFilter.FilterString(Message.PopString()).Trim();
            string Description = UserInputFilter.FilterString(Message.PopString()).Trim();
            int TagCount = Message.PopWiredInt32();
            List<string> Tags = new List<string>();

            for (int i = 0; (i < TagCount && i < 2); i++)
            {
                string Tag = UserInputFilter.FilterString(Message.PopString()).Trim().ToLower();

                if (Tag.Length > 25)
                {
                    Tag = Tag.Substring(0, 25);
                }

                if (Tag.Length > 0 && !Tags.Contains(Tag))
                {
                    Tags.Add(Tag);
                }
            }

            if (!Instance.HasOngoingEvent)
            {
                Session.MessengerFriendCache.BroadcastToFriends(MessengerFriendEventComposer.Compose(Session.CharacterId,
                    MessengerFriendEventType.EventStarted, Name));
            }

            Instance.StartOrUpdateEvent(Name, Description, CategoryId, Tags);
        }

        private static void StopEvent(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session, true))
            {
                return;
            }

            Instance.StopEvent();
        }

        private static void SetIcon(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session, true))
            {
                return;
            }

            int Junk = Message.PopWiredInt32();
            int Background = Message.PopWiredInt32();

            if (Background < 1 || Background > 24)
            {
                Background = 1;
            }

            int Foreground = Message.PopWiredInt32();

            if (Foreground < 0 || Foreground > 11)
            {
                Foreground = 0;
            }

            int ObjectCount = Message.PopWiredInt32();

            Dictionary<int, int> Objects = new Dictionary<int, int>();

            for (int i = 0; (i < ObjectCount && i < 10); i++)
            {
                int Position = Message.PopWiredInt32();
                int Item = Message.PopWiredInt32();

                if (Position < 0 || Position > 10 || Item < 1 || Item > 27 || Objects.ContainsKey(Position))
                {
                    continue;
                }

                Objects.Add(Position, Item);
            }

            Instance.Info.UpdateIcon(Background, Foreground, Objects);
            Session.SendData(RoomUpdatedNotification3Composer.Compose(Instance.RoomId));
            Instance.BroadcastMessage(RoomUpdatedNotification2Composer.Compose(Instance.RoomId));
        }

        private static void RateRoom(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || Instance.CheckUserRights(Session, true) || Session.RatedRoomsCache.HasRatedRoom(Instance.RoomId))
            {
                return;
            }

            Instance.Info.IncreaseScore();
            Session.RatedRoomsCache.MarkRoomRated(Instance.Info.Id);
            Session.SendData(RoomRatingInfoComposer.Compose(Instance.Info.Score));

            if(Session.CharacterInfo.AllowFriendStream)
            {
                FriendStreamHandler.InsertNewEvent(Session.CharacterId, EventStreamType.RoomLiked, Instance.Info.Id.ToString());
            }
        }

        private static void GetUserTags(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null)
            {
                return;
            }

            RoomActor Actor = Instance.GetActorByReferenceId(Message.PopWiredUInt32());

            if (Actor == null)
            {
                return;
            }

            CharacterInfo Info = (CharacterInfo)Actor.ReferenceObject;
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                Info.UpdateTags(MySqlClient);
            }

            Session.SendData(RoomUserTagsComposer.Compose(Info.Id, Info.Tags));
        }

        private static void DeleteRoom(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session, true))
            {
                return;
            }

            Instance.DeleteRoom(Session);

            Navigator.GetUserRooms(Session, true);
        }

        private static void IgnoreUser(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null)
            {
                return;
            }

            uint UserId = CharacterResolverCache.GetUidFromName(Message.PopString());

            if (UserId == 0)
            {
                return;
            }

            Session TargetSession = SessionManager.GetSessionByCharacterId(UserId);

            if (TargetSession != null)
            {
                if (TargetSession.HasRight("cannot_ignore"))
                {
                    Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("cannot_ignore_user")));
                    return;
                }

                Session.IgnoreCache.MarkUserIgnored(TargetSession.CharacterId);
                Session.SendData(RoomIgnoreResultComposer.Compose(1));
            }
        }

        private static void UnignoreUser(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null)
            {
                return;
            }

            uint UserId = CharacterResolverCache.GetUidFromName(Message.PopString());

            if (UserId > 0)
            {
                Session.IgnoreCache.MarkUserUnignored(UserId);
                Session.SendData(RoomIgnoreResultComposer.Compose(3));
            }
        }

        private static void AnswerDoorbell(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session))
            {
                return;
            }

            string Name = Message.PopString();
            bool Accept = (Message.ReadBytes(1)[0] == 65);

            Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Name));

            if (TargetSession == null || TargetSession.AbsoluteRoomId != Instance.RoomId || TargetSession.RoomAuthed)
            {
                return;
            }

            if (Accept)
            {
                TargetSession.RoomAuthed = true;
                TargetSession.SendData(RoomDoorbellAcceptedComposer.Compose());
                return;
            }

            TargetSession.SendData(RoomDoorbellNoResponseComposer.Compose());
            RoomManager.RemoveUserFromRoom(TargetSession, false);
        }

        private static void ContinueLoadingAfterDoorbell(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Message.PopWiredUInt32());

            if (Instance == null)
            {
                return;
            }

            EnterRoom(Session, Instance);
        }

        private static void ApplyDecoration(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session, true))
            { 
                return;
            }

            Item Item = Session.InventoryCache.GetItem(Message.PopWiredUInt32());

            if (Item == null)
            {
                return;
            }

            string DecorationKey = string.Empty;

            switch (Item.Definition.Behavior)
            {
                case ItemBehavior.Floor:

                    QuestManager.ProgressUserQuest(Session, QuestType.FURNI_DECORATION_FLOOR);
                    DecorationKey = "floor";
                    break;

                case ItemBehavior.Wallpaper:

                    QuestManager.ProgressUserQuest(Session, QuestType.FURNI_DECORATION_WALL);
                    DecorationKey = "wallpaper";
                    break;

                case ItemBehavior.Landscape:

                    DecorationKey = "landscape";
                    break;
            }

            if (DecorationKey.Length == 0)
            {
                return;
            }

            Session.InventoryCache.RemoveItem(Item.Id);

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                Item.RemovePermanently(MySqlClient);
                Instance.Info.ApplyDecoration(MySqlClient, DecorationKey, Item.Flags);

                string Achievement = "ACH_RoomDeco" + char.ToUpper(DecorationKey[0]) + DecorationKey.Substring(1);
                AchievementManager.ProgressUserAchievement(MySqlClient, Session, Achievement, 1);
            }

            Instance.BroadcastMessage(RoomDecorationComposer.Compose(DecorationKey, Item.Flags));
            Session.SendData(InventoryItemRemovedComposer.Compose(Item.Id));
        }

        private static void ActivateGeneric(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null) // warning: rights need to be checked in the event handler; we do not check here
            {
                return;
            }

            Item Item = Instance.GetItem(Message.PopWiredUInt32());

            if (Item == null || Item.Definition.Behavior == ItemBehavior.StaticItem)
            {
                return;
            }

            int RequestData = Message.PopWiredInt32();

            RoomActor Actor = Instance.GetActorByReferenceId(Session.CharacterId);

            ItemEventDispatcher.InvokeItemEventHandler(Actor, Item, Instance, ItemEventType.Interact, RequestData);
            
            QuestManager.ProgressUserQuest(Session, QuestType.EXPLORE_FIND_SPECIFIC_ITEM, Item.DefinitionId);

            QuestType Type = Item.Definition.Behavior == ItemBehavior.Switchable ?
                QuestType.FURNI_SWITCH : QuestType.EXPLORE_FIND_ITEM_BEHAVIOR;

            uint Data = Item.Definition.Behavior == ItemBehavior.Switchable ?
                0 : (uint)Item.Definition.Behavior;

            QuestManager.ProgressUserQuest(Session, Type, Data);
        }

        private static void AddToStaffPicked(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Session.HasRight("hotel_admin"))
            {
                return;
            }

            if (!Navigator.StaffPickedContainsRoom(Instance.RoomId))
            {
                Navigator.AddRoomToStaffPicked(Instance.RoomId);
                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("staffpick_add_success")));

                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    CharacterInfo RoomOwnerInfo = CharacterInfoLoader.GetCharacterInfo(MySqlClient, Instance.Info.OwnerId);

                    if (RoomOwnerInfo.HasLinkedSession)
                    {
                        Session TargetSession = SessionManager.GetSessionByCharacterId(RoomOwnerInfo.Id);
                        AchievementManager.ProgressUserAchievement(MySqlClient, TargetSession, "ACH_Spr", 1);
                    }
                    else
                    {
                        AchievementManager.OfflineProgressUserAchievement(MySqlClient, Instance.Info.OwnerId, "ACH_Spr", 1);
                    }
                }
            }
            else
            {
                Navigator.RemoveRoomFromStaffPicked(Instance.RoomId);
                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("staffpick_remove_success")));
            }
        }

        private static void RespectUser(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || Session.CharacterInfo.RespectCreditHuman <= 0)
            {
                return;
            }

            RoomActor TargetActor = Instance.GetActorByReferenceId(Message.PopWiredUInt32());

            if (TargetActor == null)
            {
                return;
            }

            Session TargetSession = SessionManager.GetSessionByCharacterId(TargetActor.ReferenceId);

            if (TargetSession == null)
            {
                return;
            }

            TargetSession.CharacterInfo.RespectPoints++;
            Session.CharacterInfo.RespectCreditHuman--;

            Instance.BroadcastMessage(RoomUserRespectedComposer.Compose(TargetSession.CharacterId,
                TargetSession.CharacterInfo.RespectPoints));

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                TargetSession.CharacterInfo.SynchronizeRespectData(MySqlClient);
                Session.CharacterInfo.SynchronizeRespectData(MySqlClient);

                AchievementManager.ProgressUserAchievement(MySqlClient, TargetSession, "ACH_RespectEarned", 1);
                AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_RespectGiven", 1);
            }

            QuestManager.ProgressUserQuest(Session, QuestType.SOCIAL_RESPECT);
        }

        private static void SyncSoundManager(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null)
            {
                return;
            }

            if (Instance != null && Instance.MusicController.IsPlaying)
            {
                Session.SendData(MusicPlayingComposer.Compose(Instance.MusicController.CurrentSong.SongData.Id,
                    Instance.MusicController.SongQueuePosition, Instance.MusicController.SongSyncTimestamp));
            }
        }
    }
}
