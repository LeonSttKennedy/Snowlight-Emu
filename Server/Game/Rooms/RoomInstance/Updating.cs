using System;
using System.Collections.Generic;
using System.Linq;

using Snowlight.Specialized;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Items;
using Snowlight.Game.Pathfinding;
using Snowlight.Game.Bots;
using Snowlight.Game.Achievements;
using Snowlight.Game.Quests;
using Snowlight.Util;
using Snowlight.Game.Items.Wired;

namespace Snowlight.Game.Rooms
{
    public partial class RoomInstance : IDisposable
    {
        public void PerformUpdate()
        {
            List<RoomActor> ActorsToUpdate = new List<RoomActor>();
            List<RoomActor> ActorsToRemove = new List<RoomActor>();

            // Copy collection
            Dictionary<uint, RoomActor> ActorsCopy = new Dictionary<uint, RoomActor>();
            List<RoomActor>[,] NewUserGrid = new List<RoomActor>[mCachedModel.Heightmap.SizeX, mCachedModel.Heightmap.SizeY];

            lock (mActorSyncRoot)
            {
                foreach (KeyValuePair<uint, RoomActor> CopyItem in mActors)
                {
                    ActorsCopy.Add(CopyItem.Key, CopyItem.Value);
                }
            }

            foreach (RoomActor Actor in ActorsCopy.Values)
            {
                // If the room is unloaded, allow no actors
                if (mUnloaded)
                {
                    ActorsToRemove.Add(Actor);
                    continue;
                }

                // The idle time is increased in every step.
                if (Actor.Type == RoomActorType.UserCharacter)
                {
                    Actor.IncreaseIdleTime(Actor.ReferenceId == Info.OwnerId);
                }

                // If this is a bot, allow the brain to process this update tick.
                if (Actor.Type == RoomActorType.AiBot)
                {
                    ((Bot)Actor.ReferenceObject).Brain.PerformUpdate(this);
                }

                // Remove any walking statusses (they will be re-applied below if neccessary)
                if (Actor.UserStatusses.ContainsKey("mv"))
                {
                    Actor.RemoveStatus("mv");
                    Actor.UpdateNeeded = true;
                }

                // Update actor position if neccessary
                if (Actor.PositionToSet != null)
                {
                    if (Actor.IsSitting)
                    {
                        Actor.RemoveStatus("sit");
                        Actor.IsSitting = false;
                    }

                    // Check if the new position is in the door
                    if (Actor.Type == RoomActorType.UserCharacter && Actor.PositionToSet.X == mCachedModel.DoorPosition.X && Actor.PositionToSet.Y == mCachedModel.DoorPosition.Y)
                    {
                        ActorsToRemove.Add(Actor);
                        continue;
                    }

                    // Update the actual position
                    Actor.Position = new Vector3(Actor.PositionToSet.X, Actor.PositionToSet.Y, (Math.Round(GetUserStepHeight(Actor.PositionToSet), 1)));
                    Actor.PositionToSet = null;

                    // Handle any "MoveToAndInteract" events if this was the last step
                    if (!Actor.IsMoving && Actor.MoveToAndInteract > 0 && Actor.Type == RoomActorType.UserCharacter)
                    {
                        Item Item = GetItem(Actor.MoveToAndInteract);

                        if (Item != null)
                        {
                            ItemEventDispatcher.InvokeItemEventHandler(SessionManager.GetSessionByCharacterId(
                                Actor.ReferenceId), Item, this, ItemEventType.Interact, Actor.MoveToAndInteractData);
                        }

                        Actor.MoveToAndInteract = 0;
                        Actor.MoveToAndInteractData = 0;
                    }
                }

                // If there are more steps to be made, handle it.
                if (Actor.IsMoving)
                {
                    // Check if moving to door
                    if (!Actor.IsLeavingRoom && Actor.Pathfinder.Target.X == mCachedModel.DoorPosition.X &&
                        Actor.Pathfinder.Target.Y == mCachedModel.DoorPosition.Y)
                    {
                        Actor.IsLeavingRoom = true;
                    }

                    // Get the next step from the pathfinder
                    Vector2 NextStep = Actor.GetNextStep();

                    // If the user is leaving and has exceeded 11 steps, help him out by instantly
                    // removing them.
                    if (Actor.LeaveStepsTaken >= 11)
                    {
                        ActorsToRemove.Add(Actor);
                        continue;
                    }

                    // If the pathfinder reports no more steps to be made, this is the last step.
                    bool LastStep = !Actor.IsMoving;

                    // Check that the next step is valid and allowed
                    if (NextStep != null && ((!Actor.ClippingEnabled && IsValidPosition(NextStep)) || 
                        (!Actor.TeleportEnabled && IsValidPosition(NextStep)) ||
                        IsValidStep(Actor.Position.GetVector2(), NextStep, LastStep, Actor.OverrideClipping, NewUserGrid)))
                    {
                        // Update "mv" status
                        Actor.SetStatus("mv", NextStep.X + "," + NextStep.Y + "," + (Math.Round(GetUserStepHeight(NextStep), 1)).ToString().Replace(',', '.'));
                        Actor.PositionToSet = NextStep;

                        // Update new/temporary grid with our new move to position
                        if (NewUserGrid[NextStep.X, NextStep.Y] == null)
                        {
                            NewUserGrid[NextStep.X, NextStep.Y] = new List<RoomActor>();
                        }

                        NewUserGrid[NextStep.X, NextStep.Y].Add(Actor);

                        // Remove any "sit" statusses
                        if (Actor.UserStatusses.ContainsKey("sit"))
                        {
                            Actor.RemoveStatus("sit");
                        }

                        // Remove any "lay" statusses
                        if (Actor.UserStatusses.ContainsKey("lay"))
                        {
                            Actor.RemoveStatus("lay");
                        }

                        // Update rotation
                        Actor.BodyRotation = Rotation.Calculate(Actor.Position.GetVector2(), NextStep, Actor.MoonWalkEnabled);
                        Actor.HeadRotation = Actor.BodyRotation;

                        uint WiredId = mWiredManager.GetRegisteredWalkItem(mFurniMap[NextStep.X, NextStep.Y]);

                        Item Wired = GetItem(WiredId);
                        
                        if (Wired != null && Wired.Definition.Behavior == ItemBehavior.WiredTrigger)
                        {
                            if (WiredTypesUtil.TriggerFromInt(Wired.Definition.BehaviorData) == WiredTriggerTypes.walks_on_furni && Actor.FurniOnId != mFurniMap[NextStep.X, NextStep.Y])
                            {
                                mWiredManager.ExecuteActions(Wired, Actor);
                            }
                        }

                        WiredId = mWiredManager.GetRegisteredWalkItem(mFurniMap[Actor.Position.X, Actor.Position.Y]);

                        Wired = GetItem(WiredId);

                        if (Wired != null && Wired.Definition.Behavior == ItemBehavior.WiredTrigger)
                        {
                            if (WiredTypesUtil.TriggerFromInt(Wired.Definition.BehaviorData) == WiredTriggerTypes.walks_off_furni && Actor.FurniOnId != mFurniMap[NextStep.X, NextStep.Y])
                            {
                                mWiredManager.ExecuteActions(Wired, Actor);
                            }
                        }

                        Actor.FurniOnId = mFurniMap[NextStep.X, NextStep.Y];

                        // Request update for next @B cycle
                        Actor.UpdateNeeded = true;
                    }
                    else
                    {
                        // Invalid step: tell pathfinder to stop and mark current position on temporary grid
                        Actor.StopMoving();

                        if (NewUserGrid[NextStep.X, NextStep.Y] == null)
                        {
                            NewUserGrid[NextStep.X, NextStep.Y] = new List<RoomActor>();
                        }

                        NewUserGrid[NextStep.X, NextStep.Y].Add(Actor);
                    }
                }
                else
                {
                    if (NewUserGrid[Actor.Position.X, Actor.Position.Y] == null)
                    {
                        NewUserGrid[Actor.Position.X, Actor.Position.Y] = new List<RoomActor>();
                    }

                    NewUserGrid[Actor.Position.X, Actor.Position.Y].Add(Actor);
                }

                // Platform user updating
                double FloorHeight = GetUserStepHeight(Actor.Position.GetVector2());
                if(!Actor.IsMoving && Actor.Position.Z != FloorHeight)
                {
                    Actor.Position.Z = FloorHeight;
                    Actor.UpdateNeeded = true;
                }

                // If the actor is leaving and has stopped walking, help them out by removing them.
                if (!Actor.IsMoving && Actor.IsLeavingRoom && Actor.Type == RoomActorType.UserCharacter)
                {
                    ActorsToRemove.Add(Actor);
                    continue;
                }

                // Update status (apply any sit/lay/effect)
                UpdateActorStatus(Actor);

                // Execute the triggers
                ExecuteTriggers(Actor);

                // Add this actor to the update list if this has been requested
                if (Actor.UpdateNeeded)
                {
                    ActorsToUpdate.Add(Actor);
                    Actor.UpdateNeeded = false;
                }
            }

            // Remove all actors that need to be removed
            foreach (RoomActor Actor in ActorsToRemove)
            {
                lock (mActorSyncRoot)
                {
                    if (ActorsToUpdate.Contains(Actor))
                    {
                        ActorsToUpdate.Remove(Actor);
                    }
                }

                switch (Actor.Type)
                {
                    default:

                        RemoveActorFromRoom(Actor.Id);
                        break;

                    case RoomActorType.UserCharacter:

                        HardKickUser(Actor.ReferenceId);
                        break;
                }
            }

            // Send update list (if there are any updates) to room
            if (ActorsToUpdate.Count > 0)
            {
                BroadcastMessage(RoomUserStatusListComposer.Compose(ActorsToUpdate));
            }

            // Update tick on all items --
            List<Item> ItemCopy = null;

            lock (mItemSyncRoot)
            {
                ItemCopy = mItems.Values.ToList();
            }

            foreach (Item Item in ItemCopy)
            {
                Item.Update(this);
            }

            // Invalidate door position
            NewUserGrid[mCachedModel.DoorPosition.X, mCachedModel.DoorPosition.Y] = null;

            // Update user grid to our new version
            lock (mActorSyncRoot)
            {
                mUserGrid = NewUserGrid;
            }

            if (mMusicController != null && mMusicController.IsPlaying)
            {
                mMusicController.Update(this);
            }
        }
        
        public void ExecuteTriggers(RoomActor Actor)
        {
            Vector2 Redirect = mRedirectGrid[Actor.Position.X, Actor.Position.Y];
            if (Redirect != null)
            {
                Actor.Position = new Vector3(Redirect.X, Redirect.Y, GetUserStepHeight(Redirect));
            }

            RoomTriggers Action = mTileTriggers[Actor.Position.X, Actor.Position.Y];
            if (Action == null || Action.RoomPosition == null)
            {
                return;
            }

            Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);
            if (Session == null)
            {
                return;
            }

            RoomInstance Instance = null;
            if (!Session.IsTeleporting)
            {
                Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);
                if (Instance == null)
                {
                    return;
                }
            }

            if (Actor.Position.X == Action.RoomPosition.X && Actor.Position.Y == Action.RoomPosition.Y)
            {
                switch (Action.Action)
                {
                    case RoomTriggerList.ROLLER:

                        if (Actor.IsMoving)
                        {
                            Actor.StopMoving();
                            Actor.RemoveStatus("mv");
                            Actor.UpdateNeeded = true;
                        }

                        Actor.MoveTo(Action.ToRoomPosition.GetVector2());

                        Vector2 NextStep = Actor.GetNextStep();
                        if (NextStep == null)
                        {
                            break;
                        }

                        Actor.SetStatus("mv", NextStep.X + "," + NextStep.Y + "," + (Math.Round(GetUserStepHeight(NextStep), 1)).ToString().Replace(',', '.'));
                        Actor.PositionToSet = NextStep;

                        if (Actor.UserStatusses.ContainsKey("sit"))
                        {
                            Actor.RemoveStatus("sit");
                        }
                        
                        if (Actor.UserStatusses.ContainsKey("lay"))
                        {
                            Actor.RemoveStatus("lay");
                        }

                        Actor.BodyRotation = Rotation.Calculate(Actor.Position.GetVector2(), NextStep, Actor.MoonWalkEnabled);
                        Actor.HeadRotation = Actor.BodyRotation;
                        Actor.UpdateNeeded = true;
                        break;

                    case RoomTriggerList.TELEPORT:

                        if (Actor.Type != RoomActorType.UserCharacter)
                        {
                            break;
                        }

                        if (Action.ToRoomId == Session.CurrentRoomId || Action.ToRoomId == 0)
                        {
                            Actor.PositionToSet = Action.ToRoomPosition.GetVector2();
                            Actor.MoveTo(Actor.PositionToSet);
                            Actor.UpdateNeeded = true;
                        }
                        else if (Action.ToRoomId != Session.CurrentRoomId)
                        {
                            if (!Actor.IsMoving && Actor.Pathfinder.IsCompleted)
                            {
                                if (Instance == null)
                                {
                                    break;
                                }

                                Actor.BlockWalking();
                                
                                RoomInfo Info = RoomInfoLoader.GetRoomInfo(Action.ToRoomId);

                                if (Info.CurrentUsers >= Info.MaxUsers && !Session.HasRight("enter_full_rooms"))
                                {
                                    break;
                                }

                                Session.IsTeleporting = true;
                                Session.TriggerTeleporterId = Action.Id;

                                Session.SendData(MessengerFollowResultComposer.Compose(Info));
                            }
                        }
                        break;

                    case RoomTriggerList.INFOBUSDOOR:

                        if (ServerSettings.InfobusStatus == InfobusStatus.Open)
                        {
                            goto case RoomTriggerList.TELEPORT;
                        }
                        break;
                }
            }
        }

        public void UpdateActorStatus(RoomActor Actor)
        {
            Vector2 Redirection = mRedirectGrid[Actor.Position.X, Actor.Position.Y];

            if (Redirection != null)
            {
                Actor.Position = new Vector3(Redirection.X, Redirection.Y, GetUserStepHeight(Redirection));
            }

            RoomTileEffect Effect = mTileEffects[Actor.Position.X, Actor.Position.Y];

            if (Effect == null)
            {
                return;
            }

            Dictionary<string, string> CurrentStatusses = Actor.UserStatusses;

            if (Effect.Type == RoomTileEffectType.Sit && !CurrentStatusses.ContainsKey("mv"))
            {
                string OldStatus = (CurrentStatusses.ContainsKey("sit") ? CurrentStatusses["sit"] : string.Empty);
                string NewStatus = Actor.Type == RoomActorType.AiBot && ((Bot)Actor.ReferenceObject).PetData != null ? 
                    Math.Round(Effect.InteractionHeight / 2, 1).ToString().Replace(',', '.') : 
                    Math.Round(Effect.InteractionHeight, 1).ToString().Replace(',', '.');

                if (Actor.BodyRotation != Effect.Rotation)
                {
                    Actor.BodyRotation = Effect.Rotation;
                    Actor.HeadRotation = Effect.Rotation;
                    Actor.UpdateNeeded = true;
                }

                if (NewStatus != OldStatus)
                {
                    Actor.SetStatus("sit", NewStatus);
                    Actor.UpdateNeeded = true;
                }
            }
            else if (CurrentStatusses.ContainsKey("sit") && (Actor.Type == RoomActorType.UserCharacter && !Actor.IsSitting))
            {
                Actor.RemoveStatus("sit");
                Actor.UpdateNeeded = true;
            }
        
            if (Effect.Type == RoomTileEffectType.Lay && !CurrentStatusses.ContainsKey("mv"))
            {
                string OldStatus = (CurrentStatusses.ContainsKey("lay") ? CurrentStatusses["lay"] : string.Empty);
                string NewStatus = Math.Round(Effect.InteractionHeight, 1).ToString().Replace(',', '.');

                if (Actor.BodyRotation != Effect.Rotation)
                {
                    Actor.BodyRotation = Effect.Rotation;
                    Actor.HeadRotation = Effect.Rotation;
                    Actor.UpdateNeeded = true;
                }

                if (OldStatus != NewStatus)
                {
                    Actor.SetStatus("lay", NewStatus);
                    Actor.UpdateNeeded = true;
                }
            }
            else if (CurrentStatusses.ContainsKey("lay") && (Actor.Type == RoomActorType.UserCharacter))
            {
                Actor.RemoveStatus("lay");
                Actor.UpdateNeeded = true;
            }

            if (Effect.Type == RoomTileEffectType.Effect)
            {
                if ((Actor.AvatarEffectId != Effect.EffectId || !Actor.AvatarEffectByItem))
                {
                    Actor.ApplyEffect(Effect.EffectId, true, true);
                }             
            }
            else if (Actor.AvatarEffectByItem)
            {
                int ClearEffect = 0;

                if (Actor.Type == RoomActorType.UserCharacter)
                {
                    Session SessionObject = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                    if (SessionObject != null)
                    {
                        ClearEffect = SessionObject.CurrentEffect;
                    }
                }
                else
                {
                    Bot BotObject = (Bot)Actor.ReferenceObject;

                    if (BotObject != null)
                    {
                        ClearEffect = BotObject.Effect;
                    }
                }

                Actor.ApplyEffect(ClearEffect, true, true);
            }

            if (Actor.Type == RoomActorType.UserCharacter && (Effect.QuestData > 0 || Effect.QuestBehavior > ItemBehavior.None))
            {
                Session SessionObject = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                if (SessionObject != null)
                {
                    QuestType Type = Effect.QuestBehavior > ItemBehavior.None ?
                        QuestType.EXPLORE_FIND_ITEM_BEHAVIOR : QuestType.EXPLORE_FIND_SPECIFIC_ITEM;

                    uint Data = Effect.QuestBehavior > ItemBehavior.None ?
                        (uint)Effect.QuestBehavior : Effect.QuestData;

                    QuestManager.ProgressUserQuest(SessionObject, Type, Data);
                }
            } 
        }
    }
}
