using System;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Misc;
using Snowlight.Game.Pets;
using Snowlight.Game.Bots;
using Snowlight.Game.Items;
using Snowlight.Specialized;
using Snowlight.Game.Rights;
using Snowlight.Game.Catalog;
using Snowlight.Communication;
using Snowlight.Game.Sessions;
using Snowlight.Game.Achievements;
using Snowlight.Game.Quests;
using Snowlight.Communication.Outgoing;
using Snowlight.Communication.Incoming;
using Snowlight.Game.Characters;

namespace Snowlight.Game.Rooms
{
    public static class RoomItemHandler
    {
        public static void Initialize()
        {
            DataRouter.RegisterHandler(OpcodesIn.ROOM_ITEM_PLACE, new ProcessRequestCallback(PlaceItem));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_ITEM_PLACE_STICKY, new ProcessRequestCallback(PlaceItem));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_ITEM_TAKE, new ProcessRequestCallback(TakeItem));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_MOVE_FLOOR_ITEM, new ProcessRequestCallback(MoveFloorItem));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_MOVE_WALL_ITEM, new ProcessRequestCallback(MoveWallItem));

            DataRouter.RegisterHandler(OpcodesIn.OPEN_STICKY, new ProcessRequestCallback(OpenSticky));
            DataRouter.RegisterHandler(OpcodesIn.SAVE_STICKY, new ProcessRequestCallback(SaveSticky));
            DataRouter.RegisterHandler(OpcodesIn.DELETE_STICKY, new ProcessRequestCallback(DeleteSticky));

            DataRouter.RegisterHandler(OpcodesIn.MOODLIGHT_INFO, new ProcessRequestCallback(GetMoodlightInfo));
            DataRouter.RegisterHandler(OpcodesIn.MOODLIGHT_SWITCH, new ProcessRequestCallback(SwitchMoodlight));
            DataRouter.RegisterHandler(OpcodesIn.MOODLIGHT_UPDATE, new ProcessRequestCallback(UpdateMoodlight));

            DataRouter.RegisterHandler(OpcodesIn.PLACE_PET, new ProcessRequestCallback(PlacePet));
            DataRouter.RegisterHandler(OpcodesIn.PETS_GET_INFO, new ProcessRequestCallback(GetPetInfo));
            DataRouter.RegisterHandler(OpcodesIn.PET_TAKE, new ProcessRequestCallback(TakePet));
            DataRouter.RegisterHandler(OpcodesIn.PET_GET_TRAINING_INFO, new ProcessRequestCallback(PetTrainerPanel));
            DataRouter.RegisterHandler(OpcodesIn.PET_RESPECT, new ProcessRequestCallback(RespectPet));

            DataRouter.RegisterHandler(OpcodesIn.OPEN_GIFT, new ProcessRequestCallback(OpenPresent));

            DataRouter.RegisterHandler(OpcodesIn.ROOM_SAVE_BRANDING, new ProcessRequestCallback(SaveBranding));

            DataRouter.RegisterHandler(OpcodesIn.SET_FOOTBALL_GATE_DATA, new ProcessRequestCallback(SetFootballGateData));

            DataRouter.RegisterHandler(OpcodesIn.UPDATE_WIRED_TRIGGER, new ProcessRequestCallback(SaveWired));
            DataRouter.RegisterHandler(OpcodesIn.UPDATE_WIRED_EFFECT, new ProcessRequestCallback(SaveWired));
            DataRouter.RegisterHandler(OpcodesIn.UPDATE_WIRED_CONDITION, new ProcessRequestCallback(SaveWired));
        }

        private static void PlaceItem(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null)
            {
                return;
            }

            uint ItemId = 0;
            string[] Data = null;

            if (Message.Id == OpcodesIn.ROOM_ITEM_PLACE_STICKY)
            {
                ItemId = Message.PopWiredUInt32();

                string RawData = Message.PopString();
                string[] TmpData = RawData.Split(' ');

                Data = new string[TmpData.Length + 1];
                Data[0] = string.Empty;
                Data[1] = TmpData[0];
                Data[2] = TmpData[1];
                Data[3] = TmpData[2];
            }
            else
            {
                string RawData = Message.PopString();
                Data = RawData.Split(' ');

                uint.TryParse(Data[0], out ItemId);
            }

            Item Item = Session.InventoryCache.GetItem(ItemId);

            if (Item == null)
            {
                return;
            }

            bool HasPlacementRights = Instance.CheckUserRights(Session);
            bool IsPlacingGuestStickie = false;

            if (Item.Definition.Behavior == ItemBehavior.StickyNote && !HasPlacementRights &&
                Instance.GuestsCanPlaceStickies)
            {
                IsPlacingGuestStickie = true;
            }
            else if (!HasPlacementRights)
            {
                Session.SendData(RoomItemPlacementErrorComposer.Compose(RoomItemPlacementErrorCode.InsufficientRights));
                return;
            }

            if (Item.PendingExpiration && Item.ExpireTimeLeft <= 0)
            {
                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    Item.RemovePermanently(MySqlClient);
                }

                Session.InventoryCache.RemoveItem(Item.Id);
                Session.SendData(InventoryItemRemovedComposer.Compose(Item.Id));
                return;
            }

            switch (Item.Definition.Type)
            {
                default:
                case ItemType.FloorItem:

                    if (Data.Length != 4)
                    {
                        return;
                    }

                    int X = 0;
                    int Y = 0;
                    int Rotation = 0;

                    int.TryParse(Data[1], out X);
                    int.TryParse(Data[2], out Y);
                    int.TryParse(Data[3], out Rotation);

                    Vector3 FinalizedPosition = Instance.SetFloorItem(Session, Item, new Vector2(X, Y), Rotation);

                    if (FinalizedPosition != null)
                    {
                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            Item.MoveToRoom(MySqlClient, Instance.RoomId, FinalizedPosition, Rotation);


                            Instance.RegenerateRelativeHeightmap();

                            Session.InventoryCache.RemoveItem(Item.Id);
                            Session.SendData(InventoryItemRemovedComposer.Compose(Item.Id));

                            ItemEventDispatcher.InvokeItemEventHandler(Session, Item, Instance, ItemEventType.Placed);

                            Instance.BroadcastMessage(RoomFloorItemPlacedComposer.Compose(Item));

                            QuestManager.ProgressUserQuest(Session, QuestType.FURNI_PLACE);

                            if (FinalizedPosition.Z > Instance.Model.Heightmap.FloorHeight[FinalizedPosition.X, FinalizedPosition.Y])
                            {
                                QuestManager.ProgressUserQuest(Session, QuestType.FURNI_STACK);
                            }

                            if (Item.Definition.Behavior.Equals(ItemBehavior.BlackHole))
                            {
                                string HoleACH = "ACH_RoomDecoHoleFurniCount";

                                UserAchievement HoleCountAchData = Session.AchievementCache.GetAchievementData(HoleACH);

                                int HoleCountAchData_Progress = HoleCountAchData != null ? HoleCountAchData.Progress : 0;

                                int ActualRoomHoleCount = Instance.GetFloorItems().Where(I => I.Definition.Behavior.Equals(ItemBehavior.BlackHole)).Count();

                                int Difference = ActualRoomHoleCount - HoleCountAchData_Progress;

                                if (Difference > 0)
                                {
                                    AchievementManager.ProgressUserAchievement(MySqlClient, Session, HoleACH, Difference);
                                }
                            }
                        }
                    }
                    
                    break;

                case ItemType.WallItem:

                    string[] CorrectedData = new string[Data.Length - 1];

                    for (int i = 1; i < Data.Length; i++)
                    {
                        CorrectedData[i - 1] = Data[i];
                    }

                    string WallPos = Instance.SetWallItem(Session, CorrectedData, Item);

                    if (WallPos.Length > 0)
                    {
                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            Item.MoveToRoom(MySqlClient, Instance.RoomId, new Vector3(0, 0, 0), 0, WallPos);


                            Session.InventoryCache.RemoveItem(Item.Id);
                            Session.SendData(InventoryItemRemovedComposer.Compose(Item.Id));

                            ItemEventDispatcher.InvokeItemEventHandler(Session, Item, Instance, ItemEventType.Placed);

                            Instance.BroadcastMessage(RoomWallItemPlacedComposer.Compose(Item));

                            if (IsPlacingGuestStickie)
                            {
                                Instance.GiveTemporaryStickieRights(Item.Id, Session.CharacterId);
                                Session.SendData(StickyDataComposer.Compose(Item));

                                CharacterInfo OwnerInfo = CharacterInfoLoader.GetCharacterInfo(MySqlClient, Instance.Info.OwnerId);

                                if (OwnerInfo.HasLinkedSession)
                                {
                                    Session OwnerSession = SessionManager.GetSessionByCharacterId(Instance.Info.OwnerId);

                                    AchievementManager.ProgressUserAchievement(MySqlClient, OwnerSession, "ACH_NotesReceived", 1);
                                }
                                else
                                {
                                    AchievementManager.OfflineProgressUserAchievement(MySqlClient, Instance.Info.OwnerId, "ACH_NotesReceived", 1);
                                }

                                if (Session.CharacterId != Instance.Info.OwnerId)
                                {
                                    AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_NotesLeft", 1);
                                }
                            }
                        }
                    }

                    break;
            }
        }

        private static void TakeItem(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session, true))
            {
                return;
            }

            int Unknown1 = Message.PopWiredInt32();
            Item Item = Instance.GetItem(Message.PopWiredUInt32());

            if (Item == null || Item.Definition.Behavior == ItemBehavior.StickyNote)
            {
                return;
            }
            
            if (Instance.TakeItem(Item.Id))
            {
                ItemEventDispatcher.InvokeItemEventHandler(Session, Item, Instance, ItemEventType.Removing);

                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    Item.MoveToUserInventory(MySqlClient, Session.CharacterId);
                }

                Instance.RegenerateRelativeHeightmap();

                Session.InventoryCache.Add(Item);
                Session.SendData(InventoryItemAddedComposer.Compose(Item));

                QuestManager.ProgressUserQuest(Session, QuestType.FURNI_PICK);
            }
        }

        private static void MoveFloorItem(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session))
            {
                return;
            }

            Item Item = Instance.GetItem(Message.PopWiredUInt32());

            if (Item == null || Item.Definition.Type != ItemType.FloorItem)
            {
                return;
            }

            Vector2 NewPosition = new Vector2(Message.PopWiredInt32(), Message.PopWiredInt32());
            int NewRotation = Message.PopWiredInt32();

            bool IsRotationOnly = (Item.RoomId == Instance.RoomId && Item.RoomPosition.X == NewPosition.X &&
                    Item.RoomPosition.Y == NewPosition.Y && NewRotation != Item.RoomRotation);

            Vector3 FinalizedPosition = Instance.SetFloorItem(Session, Item, NewPosition, NewRotation);

            if (FinalizedPosition != null)
            {
                Item.MoveToRoom(null, Instance.RoomId, FinalizedPosition, NewRotation, string.Empty);
                RoomManager.MarkWriteback(Item, false);         

                Instance.RegenerateRelativeHeightmap();
                Instance.BroadcastMessage(RoomItemUpdatedComposer.Compose(Item));

                ItemEventDispatcher.InvokeItemEventHandler(Session, Item, Instance, ItemEventType.Moved,
                    IsRotationOnly ? 1 : 0);

                QuestManager.ProgressUserQuest(Session, IsRotationOnly ? QuestType.FURNI_ROTATE : QuestType.FURNI_MOVE);

                if (FinalizedPosition.Z > Instance.Model.Heightmap.FloorHeight[FinalizedPosition.X, FinalizedPosition.Y])
                {
                    QuestManager.ProgressUserQuest(Session, QuestType.FURNI_STACK);
                }
            }
        }

        private static void MoveWallItem(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session))
            {
                return;
            }

            Item Item = Instance.GetItem(Message.PopWiredUInt32());

            if (Item == null || Item.Definition.Type != ItemType.WallItem)
            {
                return;
            }

            string RawPlacementData = Message.PopString();
            string WallPos = Instance.SetWallItem(Session, RawPlacementData, Item);

            if (WallPos.Length > 0)
            {
                Item.MoveToRoom(null, Instance.RoomId, new Vector3(0, 0, 0), 0, WallPos);
                RoomManager.MarkWriteback(Item, false);    

                Instance.BroadcastMessage(RoomWallItemMovedComposer.Compose(Item));

                ItemEventDispatcher.InvokeItemEventHandler(Session, Item, Instance, ItemEventType.Moved);
            }
        }

        private static void OpenSticky(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null)
            {
                return;
            }

            Item Item = Instance.GetItem(Message.PopWiredUInt32());

            if (Item == null || Item.Definition.Behavior != ItemBehavior.StickyNote)
            {
                return;
            }

            Session.SendData(StickyDataComposer.Compose(Item));
        }

        private static void SaveSticky(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null)
            {
                return;
            }

            Item Item = Instance.GetItem(Message.PopWiredUInt32());

            if (Item == null || Item.Definition.Behavior != ItemBehavior.StickyNote)
            {
                return;
            }

            StickieEditingRights Rights = Instance.GetStickieEditingRights(Session, Item);

            if (Rights == StickieEditingRights.ReadOnly)
            {
                return;
            }

            string RawData = Message.PopString();
            string[] Bits = RawData.Split(' ');

            if (Bits.Length < 2)
            {
                return;
            }

            string Color = Bits[0].ToUpper().Trim();
            string Text = UserInputFilter.FilterString(RawData.Substring(Color.Length + 1, RawData.Length - (Color.Length + 1))).Trim();

            if (Color != "FFFF33" && Color != "FF9CFF" && Color != "9CCEFF" && Color != "9CFF9C" || Text.Length > 391)
            {
                return;
            }

            Item.Flags = RawData;
            Item.DisplayFlags = Color;

            if (Rights == StickieEditingRights.GuestEdit)
            {
                Item.Flags += "\n-----\n" + Session.CharacterInfo.Username + "\n" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            }

            Instance.RevokeTemporaryStickieRights(Item.Id);

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                Item.SynchronizeDatabase(MySqlClient, true);
            }

            Instance.BroadcastMessage(RoomWallItemMovedComposer.Compose(Item));
        }

        private static void DeleteSticky(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session, true))
            {
                return;
            }

            Item Item = Instance.GetItem(Message.PopWiredUInt32());

            if (Item == null || Item.Definition.Behavior != ItemBehavior.StickyNote)
            {
                return;
            }

            if (Instance.TakeItem(Item.Id))
            {
                Instance.RegenerateRelativeHeightmap();

                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    Item.RemovePermanently(MySqlClient);
                }
            }
        }

        private static void GetMoodlightInfo(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session, true))
            {
                return;
            }

            Item Item = Instance.MoodlightItem;

            if (Item == null)
            {
                return;
            }

            Session.SendData(MoodlightDataComposer.Compose(MoodlightData.GenerateFromFlags(Item.Flags)));
        }

        private static void SwitchMoodlight(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session, true))
            {
                return;
            }

            Item Item = Instance.MoodlightItem;

            if (Item == null)
            {
                return;
            }

            MoodlightData Data = MoodlightData.GenerateFromFlags(Item.Flags);
            Data.Enabled = !Data.Enabled;

            Item.Flags = Data.ToItemFlagData();
            Item.DisplayFlags = Data.ToDisplayData();

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                Item.SynchronizeDatabase(MySqlClient, true);
            }

            Item.BroadcastStateUpdate(Instance);
        }

        private static void UpdateMoodlight(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null || !Instance.CheckUserRights(Session, true))
            {
                return;
            }

            Item Item = Instance.MoodlightItem;

            if (Item == null)
            {
                return;
            }

            MoodlightData Data = MoodlightData.GenerateFromFlags(Item.Flags);
            int PresetId = Message.PopWiredInt32();
            MoodlightPreset Preset = null;

            if (Data.Presets.ContainsKey(PresetId))
            {
                Preset = Data.Presets[PresetId];
            }

            if (Preset == null)
            {
                return;
            }

            Preset.BackgroundOnly = !Message.PopWiredBoolean();
            Preset.ColorCode = UserInputFilter.FilterString(Message.PopString().Trim());
            Preset.ColorIntensity = Message.PopWiredInt32();

            if (!MoodlightData.IsValidColor(Preset.ColorCode))
            {
                return;
            }

            Item.Flags = Data.ToItemFlagData();
            Item.DisplayFlags = Data.ToDisplayData();

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                Item.SynchronizeDatabase(MySqlClient, true);
            }

            Item.BroadcastStateUpdate(Instance);
        }

        private static void PlacePet(Session Session, ClientMessage Message)
        {
            /* 
             * needs to investigate a visual bug when pet is placed.
             * This bug ocour to session that are placing a pet if ActorId and PetId are equals
             */

            if (!Session.HasRight("hotel_admin") && !ServerSettings.PetsEnabled)
            {
                Session.SendData(PetPlacementErrorComposer.Compose(PetPlacingError.PetsDisabledInThisHotel));
                return;
            }

            uint PetId = Message.PopWiredUInt32();
            int X = Message.PopWiredInt32();
            int Y = Message.PopWiredInt32();

            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);
            if (Instance == null || (!Instance.CheckUserRights(Session, true) && !Instance.Info.AllowPets))
            {
                Session.SendData(PetPlacementErrorComposer.Compose(PetPlacingError.PetsDisabledInThisRoom));
                return;
            }

            bool IsOwner = Instance.CheckUserRights(Session, true);

            Pet Pet = Session.InventoryCache.GetPet(PetId);
            if (Pet == null || Pet.IsInRoom)
            {
                return;
            }

            RoomActor Actor = Instance.GetActorByReferenceId(Session.CharacterId);
            if (Actor == null)
            {
                return;
            }

            Vector2 DesiredPosition = IsOwner ? new Vector2(X, Y) : Actor.SquareInFront;
            if (!Instance.CanInitiateMoveToPosition(DesiredPosition)) 
            {
                PetPlacingError Code = IsOwner ? PetPlacingError.RoomOwnerPlacingError :
                    PetPlacingError.GuestPetPlacingError;

                Session.SendData(PetPlacementErrorComposer.Compose(Code));
                return;
            }

            Bot BotDefinition = BotManager.GetHandlerDefinitionForPetType(Pet.Type);
            if (BotDefinition == null)
            {
                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("cannot_place_pet")));
                return;
            }

            if (!Instance.CanPlacePet(IsOwner))
            {
                Session.SendData(PetPlacementErrorComposer.Compose(PetPlacingError.ReachPetLimitForThisRoom));
                return;
            }

            Vector3 Position = new Vector3(DesiredPosition.X, DesiredPosition.Y, Instance.GetUserStepHeight(DesiredPosition));

            Pet.MoveToRoom(Instance.RoomId, Position);
            Instance.AddBotToRoom(BotManager.CreateNewInstance(BotDefinition, Instance.RoomId, Position, Pet));

            RoomManager.MarkWriteback(Pet);

            Session.InventoryCache.RemovePet(Pet.Id);
            Session.SendData(InventoryPetRemovedComposer.Compose(Pet.Id));

            if (Instance.Info.OwnerId != Session.CharacterId)
            {
                QuestManager.ProgressUserQuest(Session, QuestType.PET_TO_OTHER_ROOM, 1);
                if (Instance.PetActorCount > 1)
                {
                    QuestManager.ProgressUserQuest(Session, QuestType.PETS_IN_ROOM, (uint)Instance.PetActorCount);
                }
            }
        }

        private static void GetPetInfo(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);
            if (Instance == null)
            {
                return;
            }

            uint PetId = Message.PopWiredUInt32();
            RoomActor Actor = Instance.GetActorByReferenceId(PetId, RoomActorType.AiBot);
            if (Actor == null)
            {
                return;
            }

            Pet PetData = ((Bot)Actor.ReferenceObject).PetData;
            if (PetData == null) 
            {
                return;
            }

            Session.SendData(PetInfoComposer.Compose(PetData));
            Session.SendData(PetTrainingPanelComposer.Compose(PetData));

            QuestManager.ProgressUserQuest(Session, QuestType.FIND_A_PET_TYPE, (uint)PetData.Type);
        }

        private static void TakePet(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);
            if (Instance == null)
            {
                return;
            }

            uint PetId = Message.PopWiredUInt32();
            RoomActor Actor = Instance.GetActorByReferenceId(PetId, RoomActorType.AiBot);
            if (Actor == null) 
            {
                return;
            }

            Pet PetData = ((Bot)Actor.ReferenceObject).PetData;
            if (PetData == null || (PetData.OwnerId != Session.CharacterId && !Session.HasRight("hotel_admin")))
            {
                return;
            }

            PetData.MoveToUserInventory();
            RoomManager.MarkWriteback(PetData);

            Instance.RemoveActorFromRoom(Actor.Id);

            Session.InventoryCache.Add(PetData);
            Session.SendData(InventoryPetAddedComposer.Compose(PetData));
        }
        private static void PetTrainerPanel(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);
            if (Instance == null)
            {
                return;
            }

            uint PetId = Message.PopWiredUInt32();
            RoomActor Actor = Instance.GetActorByReferenceId(PetId, RoomActorType.AiBot);
            if (Actor == null)
            {
                return;
            }

            Pet PetData = ((Bot)Actor.ReferenceObject).PetData;
            if (PetData == null)
            {
                return;
            }

            Session.SendData(PetTrainingPanelComposer.Compose(PetData));
        }

        private static void RespectPet(Session Session, ClientMessage Message)
        {
            if (Session.CharacterInfo.RespectCreditPets <= 0)
            {
                return;
            }

            TimeSpan TotalDaysRegistered = DateTime.Now - UnixTimestamp.GetDateTimeFromUnixTimestamp(Session.CharacterInfo.TimestampRegistered);
            if (ServerSettings.PetScratchingAccountDaysOldEnabled &&
                TotalDaysRegistered.Days < ServerSettings.PetScratchingAccountDaysOld)
            {
                Session.SendData(PetRespectErrorComposer.Compose(TotalDaysRegistered.Days));
                return;
            }

            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);
            if (Instance == null)
            {
                return;
            }

            uint PetId = Message.PopWiredUInt32();
            RoomActor Actor = Instance.GetActorByReferenceId(PetId, RoomActorType.AiBot);
            if (Actor == null)
            {
                return;
            }

            Pet PetData = ((Bot)Actor.ReferenceObject).PetData;
            if (PetData == null)
            {
                return;
            }

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                Session.CharacterInfo.RespectCreditPets--;
                Session.CharacterInfo.SynchronizeRespectData(MySqlClient);
                PetData.OnRespect(MySqlClient, Instance);

                CharacterInfo OwnerInfo = CharacterInfoLoader.GetCharacterInfo(MySqlClient, PetData.OwnerId);

                if (OwnerInfo.HasLinkedSession)
                {
                    Session TargetSession = SessionManager.GetSessionByCharacterId(OwnerInfo.Id);

                    if (TargetSession.CharacterId != Session.CharacterId)
                    {
                        AchievementManager.ProgressUserAchievement(MySqlClient, TargetSession, "ACH_PetRespectReceiver", 1);
                    }
                }
                else
                {
                    AchievementManager.OfflineProgressUserAchievement(MySqlClient, OwnerInfo.Id, "ACH_PetRespectReceiver", 1);
                }

                if (Session.CharacterId != PetData.OwnerId)
                {
                    AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_PetRespectGiver", 1);
                }

                QuestManager.ProgressUserQuest(Session, QuestType.SCRATCH_A_PET, 1);
            }

            Actor.SetStatus("std", "");
            Actor.SetStatus("gst sml", "");
            Actor.UpdateNeeded = true;
        }

        private static void OpenPresent(Session Session, ClientMessage Message)
        {
            uint PresentID = Message.PopWiredUInt32();
            double ExpireTimestamp = 0;

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);
                if (Instance == null || !Instance.CheckUserRights(Session, true))
                {
                    return;
                }

                Item GiftItem = Instance.GetItem(PresentID);
                if (GiftItem == null)
                {
                    return;
                }

                MySqlClient.SetParameter("itemid", GiftItem.Id);
                DataRow Row = MySqlClient.ExecuteQueryRow("SELECT * FROM user_gifts WHERE item_id = @itemid LIMIT 1");
                if (Row == null)
                {
                    return;
                }

                GiftItem.RemovePermanently(MySqlClient);
                Instance.TakeItem(GiftItem.Id);
                Instance.RegenerateRelativeHeightmap();

                string[] RowItemIds = ((string)Row["base_ids"]).Split('|');
                string[] RowAmounts = ((string)Row["amounts"]).Split('|');

                for(int i = 0; i < RowItemIds.Length; i++)
                {
                    int Amount = int.Parse(RowAmounts[i]);

                    ItemDefinition BaseItem = ItemDefinitionManager.GetDefinition(uint.Parse(RowItemIds[i]));

                    Session.SendData(RoomGiftOpenedComposer.Compose(BaseItem.TypeLetter, BaseItem.SpriteId, BaseItem.Name));

                    switch (BaseItem.Behavior)
                    {
                        case ItemBehavior.Rental:

                            ExpireTimestamp = UnixTimestamp.GetCurrent() + 3600;
                            break;

                        case ItemBehavior.DuckHC:
                        case ItemBehavior.DuckVIP:

                            string BasicAchievement = "ACH_BasicClub";
                            string VipAchievement = "ACH_VipClub";

                            double Length = 86400 * BaseItem.BehaviorData;

                            ClubSubscriptionLevel Level = BaseItem.Behavior == ItemBehavior.DuckVIP ?
                                ClubSubscriptionLevel.VipClub : BaseItem.Behavior == ItemBehavior.DuckHC ?
                                ClubSubscriptionLevel.BasicClub : ClubSubscriptionLevel.None;

                            if (Session.SubscriptionManager.IsActive &&
                                Session.SubscriptionManager.SubscriptionLevel == ClubSubscriptionLevel.VipClub)
                            {
                                Level = ClubSubscriptionLevel.VipClub;
                            }

                            // Extend membership
                            Session.SubscriptionManager.AddOrExtend((int)Level, Length);

                            // Check if we need to manually award basic/vip badges
                            bool NeedsBasicUnlock = !Session.BadgeCache.ContainsCodeWith(BasicAchievement)
                                && Level >= ClubSubscriptionLevel.BasicClub;

                            bool NeedsVipUnlock = !Session.BadgeCache.ContainsCodeWith(VipAchievement)
                                && Level == ClubSubscriptionLevel.VipClub;

                            // Reload the badge cache (reactivating any disabled subscription badges)
                            Session.BadgeCache.ReloadCache(MySqlClient, Session.AchievementCache);

                            // Virtually unlock the basic achievement without reward if needed
                            if (NeedsBasicUnlock)
                            {
                                Achievement Achievement = AchievementManager.GetAchievement(BasicAchievement);

                                if (Achievement != null)
                                {
                                    UserAchievement UserAchievement = Session.AchievementCache.GetAchievementData(
                                        BasicAchievement);

                                    if (UserAchievement != null)
                                    {
                                        Session.SendData(AchievementUnlockedComposer.Compose(Achievement, UserAchievement.Level,
                                            0, 0, 0));
                                    }
                                }
                            }

                            // Virtually unlock the VIP achievement without reward if needed
                            if (NeedsVipUnlock)
                            {
                                Achievement Achievement = AchievementManager.GetAchievement(VipAchievement);

                                if (Achievement != null)
                                {
                                    UserAchievement UserAchievement = Session.AchievementCache.GetAchievementData(
                                        VipAchievement);

                                    if (UserAchievement != null)
                                    {
                                        Session.SendData(AchievementUnlockedComposer.Compose(Achievement, UserAchievement.Level,
                                           0, 0, 0));
                                    }
                                }
                            }

                            // Disable any VIP badges if they still aren't valid
                            if (Session.SubscriptionManager.SubscriptionLevel < ClubSubscriptionLevel.VipClub)
                            {
                                Session.BadgeCache.DisableSubscriptionBadge(VipAchievement);
                            }

                            // Synchronize equipped badges if the user has unlocked anything
                            if (NeedsVipUnlock || NeedsBasicUnlock)
                            {
                                if (Instance != null)
                                {
                                    Instance.BroadcastMessage(RoomUserBadgesComposer.Compose(Session.CharacterId,
                                        Session.BadgeCache.EquippedBadges));
                                }
                            }

                            Session.SubscriptionManager.UpdateUserBadge();

                            // Clear catalog cache for user (in case of changes)
                            CatalogManager.ClearCacheGroup(Session.CharacterId);

                            // Send new data to client
                            Session.SendData(FuseRightsListComposer.Compose(Session));
                            Session.SendData(SubscriptionStatusComposer.Compose(Session.SubscriptionManager, true));

                            if (Session.SubscriptionManager.GiftPoints > 0)
                            {
                                Session.SendData(ClubGiftReadyComposer.Compose(Session.SubscriptionManager.GiftPoints));
                            }

                            break;
                    }

                    Dictionary<int, List<uint>> NewItems = new Dictionary<int, List<uint>>();
                    List<Item> GeneratedGenericItems = new List<Item>();
                    for (int a = 0; a < Amount; a++)
                    {
                        GeneratedGenericItems.Add(ItemFactory.CreateItem(MySqlClient, BaseItem.Id,
                            Session.CharacterInfo.Id, (string)Row["extra_data"], (string)Row["extra_data"], ExpireTimestamp));
                    }

                    switch (BaseItem.Behavior)
                    {
                        case ItemBehavior.Teleporter:

                            Item LinkedItem = ItemFactory.CreateItem(MySqlClient, BaseItem.Id,
                                Session.CharacterInfo.Id, GeneratedGenericItems[0].Id.ToString(), string.Empty,
                                ExpireTimestamp);

                            GeneratedGenericItems[0].Flags = LinkedItem.Id.ToString();
                            GeneratedGenericItems[0].SynchronizeDatabase(MySqlClient, true);

                            GeneratedGenericItems.Add(LinkedItem);
                            break;
                    }

                    foreach (Item GeneratedItem in GeneratedGenericItems)
                    {
                        Session.InventoryCache.Add(GeneratedItem);

                        int TabId = GeneratedItem.Definition.Type == ItemType.FloorItem ? 1 : 2;

                        if (!NewItems.ContainsKey(TabId))
                        {
                            NewItems.Add(TabId, new List<uint>());
                        }

                        NewItems[TabId].Add(GeneratedItem.Id);
                    }

                    foreach (KeyValuePair<int, List<uint>> NewItemData in NewItems)
                    {
                        foreach (uint NewItem in NewItemData.Value)
                        {
                            Session.NewItemsCache.MarkNewItem(MySqlClient, NewItemData.Key, NewItem);
                        }
                    }

                    if (NewItems.Count > 0)
                    {
                        Session.SendData(InventoryNewItemsComposer.Compose(new Dictionary<int, List<uint>>(NewItems)));
                    }
                }
                
                Session.SendData(InventoryRefreshComposer.Compose());

                MySqlClient.SetParameter("itemid", GiftItem.Id);
                MySqlClient.ExecuteNonQuery("DELETE FROM user_gifts WHERE item_id = @itemid LIMIT 1");
            }
        }

        private static void SaveBranding(Session Session, ClientMessage Message)
        {
            uint ItemId = Message.PopWiredUInt32();
            uint Data = Message.PopWiredUInt32();
            
            string Brand = Message.PopString();
            string Brand2 = Message.PopString();
            string Brand3 = Message.PopString();
            string Brand4 = Message.PopString();
            string Brand5 = Message.PopString();
            string Brand6 = Message.PopString();
            string Brand7 = Message.PopString();
            string Brand8 = Message.PopString();
            string Brand9 = Message.PopString();
            string Brand10 = Message.PopString();
            string BrandData = Brand + "=" + Brand2 + Convert.ToChar(9) + Brand3 + "=" + Brand4 + Convert.ToChar(9) + Brand5 + "=" + Brand6 + Convert.ToChar(9) + Brand7 + "=" + Brand8 + Convert.ToChar(9) + Brand9 + "=" + Brand10 + Convert.ToChar(9) + "state=0";

            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);
            Item Item = Instance.GetItem(ItemId);

            Item.Flags = BrandData;
            Item.DisplayFlags = BrandData;
            RoomManager.MarkWriteback(Item, true);

            Item.BroadcastStateUpdate(Instance);

            Instance.RegenerateRelativeHeightmap();
        }
        private static void SetFootballGateData(Session Session, ClientMessage Message)
        {
            uint ItemId = Message.PopWiredUInt32();
            string CharGender = Message.PopString().ToUpper();
            string Figure = UserInputFilter.FilterString(Message.PopString());
            Figure = Figure.Replace("hd-99999-99999.", "");

            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance != null)
            {
                Item RoomItem = Instance.GetItem(ItemId);
                if (RoomItem != null)
                {
                    string[] ItemExtraData = RoomItem.Flags.Split(',');
                    switch (CharGender.ToUpper())
                    {
                        case "M":

                            ItemExtraData[0] = "hd-99999-99999." + Figure;
                            break;

                        case "F":

                            ItemExtraData[1] = "hd-99999-99999." + Figure;
                            break;

                        default:

                            return;
                    }

                    RoomItem.Flags = string.Join(",", ItemExtraData);
                    RoomItem.DisplayFlags = RoomItem.Flags;
                    RoomManager.MarkWriteback(RoomItem, true);

                    RoomItem.BroadcastStateUpdate(Instance);

                    Instance.RegenerateRelativeHeightmap();
                }
            }
        }
        private static void SaveWired(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);
            Instance.WiredManager.HandleSave(Session, Message);
        }
    }
}
