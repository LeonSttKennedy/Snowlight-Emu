using Snowlight.Communication.Outgoing;
using Snowlight.Game.Achievements;
using Snowlight.Game.Characters;
using Snowlight.Game.Misc;
using Snowlight.Game.Quests;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Specialized;
using Snowlight.Storage;
using Snowlight.Util;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class FreezeHandler
    {
        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.FreezeGate, new ItemEventHandler(HandleFreezeGate));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.FreezeGeysir, new ItemEventHandler(HandleFreezeGeysir));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.FreezeTile, new ItemEventHandler(HandleFreezeTile));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.FreezeBlock, new ItemEventHandler(HandleFreezeBlock));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.FreezeExitTile, new ItemEventHandler(HandleFreezeExitTile));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.FreezeScore, new ItemEventHandler(HandleFreezeScoreboard));
        }

        private static bool HandleFreezeGate(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            GameManager FreezeManager = Instance.GameManager;
            TeamColors GateColor = FreezeManager.GateColor(GameType.Freeze, Item);
            int ClearEffect = 0;
            int TeamLimit = 5;

            switch (Event)
            {
                case ItemEventType.Placed:
                case ItemEventType.InstanceLoaded:

                    Item.RequestUpdate(1);
                    break;

                case ItemEventType.UpdateTick:

                    Item.LockTileSquare = FreezeManager.FreezeStarted;

                    int.TryParse(Item.Flags, out int CurrentState);

                    int NewState = FreezeManager.GameTeamPlayerCount(GameType.Freeze, GateColor);

                    if (CurrentState < 0 || CurrentState >= 6)
                    {
                        NewState = 0;
                    }

                    if (CurrentState != NewState)
                    {
                        Item.Flags = NewState.ToString();
                        Item.DisplayFlags = Item.Flags;

                        RoomManager.MarkWriteback(Item, true);
                    }

                    Item.BroadcastStateUpdate(Instance);
                    Instance.RegenerateRelativeHeightmap(true);

                    Item.RequestUpdate(2);
                    break;

                case ItemEventType.Removing:

                    FreezeManager.RemoveTeamColor(GameType.Freeze, GateColor);
                    break;

                case ItemEventType.WalkOnItem:

                    if (!FreezeManager.FreezeStarted)
                    {
                        int TeamColorPlayerCount = FreezeManager.GameTeamPlayerCount(GameType.Freeze, GateColor);

                        bool MatchActorGameStatus = Actor.PlayingGameType == GameType.Freeze;
                        bool MatchActorTeamStatus = Actor.PlayerTeamColor == GateColor;

                        if (MatchActorGameStatus && MatchActorTeamStatus)
                        {
                            Actor.PlayingGameType = GameType.None;
                            Actor.PlayerTeamColor = TeamColors.None;

                            FreezeManager.UnregisterFreezeActor(Actor);
                        }
                        else if (TeamColorPlayerCount < TeamLimit)
                        {
                            Actor.PlayingGameType = GameType.Freeze;
                            Actor.PlayerTeamColor = GateColor;

                            Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                            if (Session != null)
                            {
                                QuestManager.ProgressUserQuest(Session, QuestType.FREEZE_JOIN);
                            }
                        }

                        int ApplyEffect = 39 + (int)GateColor;

                        TeamColors AlterGameColor = MatchActorTeamStatus ? TeamColors.None : GateColor;

                        int AlterOrClearEffect = AlterGameColor == TeamColors.None ? ClearEffect : ApplyEffect;

                        Actor.ApplyEffect(AlterOrClearEffect, true);
                    }

                    Item.RequestUpdate(2);
                    break;
            }

            return true;
        }

        private static bool HandleFreezeGeysir(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            GameManager FreezeManager = Instance.GameManager;

            switch (Event)
            {
                case ItemEventType.Placed:
                case ItemEventType.InstanceLoaded:

                    Item.Flags = "1";
                    Item.DisplayFlags = Item.Flags;

                    RoomManager.MarkWriteback(Item, true);

                    Item.BroadcastStateUpdate(Instance);
                    Instance.RegenerateRelativeHeightmap(true);

                    Item.RequestUpdate(1);
                    break;

                case ItemEventType.UpdateTick:

                    if (!FreezeManager.FreezeStarted)
                    {
                        goto case ItemEventType.InstanceLoaded;
                    }

                    if (Item.Flags == string.Empty)
                    {
                        Item.Flags = "0";
                    }

                    int.TryParse(Item.Flags, out int CurrentState);

                    int Random = RandomGenerator.GetNext(0, 30);
                    if (Random == 15)
                    {
                        Item.Flags = (CurrentState == 0 ? 1 : 0).ToString();
                        Item.DisplayFlags = Item.Flags;

                        RoomManager.MarkWriteback(Item, true);

                        Item.BroadcastStateUpdate(Instance);
                        Instance.RegenerateRelativeHeightmap(true);
                    }

                    Item.RequestUpdate(1);
                    break;
            }

            return true;
        }

        private static bool HandleFreezeTile(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            GameManager FreezeManager = Instance.GameManager;

            switch (Event)
            {
                case ItemEventType.Placed:
                case ItemEventType.InstanceLoaded:

                    Item.Flags = "";
                    Item.DisplayFlags = Item.Flags;
                    Item.BroadcastStateUpdate(Instance);

                    Item.RequestUpdate(1);
                    break;


                case ItemEventType.ItemInvoking:
                case ItemEventType.UpdateTick:

                    if (!FreezeManager.FreezeStarted)
                    {
                        break;
                    }

                    bool ChainReaction = Event == ItemEventType.ItemInvoking;

                    if (ChainReaction)
                    {
                        Item.Flags = "0";
                        Item.DisplayFlags = Item.Flags;
                        Item.RequestUpdate(5);
                        break;
                    }

                    if (Item.Flags.Length >= 1)
                    {
                        List<RoomActor> Actors = Instance.GetActorsOnPosition(Item.RoomPosition.GetVector2());

                        if (Actors.Count > 0)
                        {
                            RoomActor ActorToHit = Actors.FirstOrDefault();

                            FreezeManager.HandleActorToFreeze(ActorToHit);

                            bool DifferentActors = ActorToHit != null && 
                                Item.TemporaryInteractionReferenceIds.ContainsKey(0) &&
                                Item.TemporaryInteractionReferenceIds[0] != ActorToHit.ReferenceId;

                            RoomActor Interactor = Item.TemporaryInteractionReferenceIds.ContainsKey(0) ?
                                Instance.GetActorByReferenceId(Item.TemporaryInteractionReferenceIds[0]) : null;

                            bool DifferentTeams = Interactor != null && Interactor.PlayerTeamColor != ActorToHit.PlayerTeamColor;

                            bool DoFreezeAchievements = DifferentActors && DifferentTeams;

                            if (DoFreezeAchievements)
                            {
                                Session ThrowSnowball = SessionManager.GetSessionByCharacterId(Interactor.ReferenceId);

                                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                                {
                                    CharacterInfo ThrowInfo = CharacterInfoLoader.GetCharacterInfo(MySqlClient, Interactor.ReferenceId);

                                    if (ThrowInfo.HasLinkedSession)
                                    {
                                        AchievementManager.ProgressUserAchievement(MySqlClient, ThrowSnowball, "ACH_EsA", 1);
                                    }
                                }

                                if (ThrowSnowball != null)
                                {
                                    QuestManager.ProgressUserQuest(ThrowSnowball, QuestType.FREEZE_SNOWBALL_HIT);
                                }
                            }
                        }

                        Item.Flags = "11000";
                        Item.DisplayFlags = Item.Flags;
                        Item.BroadcastStateUpdate(Instance);

                        Item.TemporaryInteractionReferenceIds.Remove(0);
                    }

                    break;

                case ItemEventType.Interact:

                    bool UserIsPlaying = Actor.PlayerTeamColor != TeamColors.None;

                    bool UserFreezed = FreezeManager.FreezeActorFreezedTime.ContainsKey(Actor) 
                        && FreezeManager.FreezeActorFreezedTime[Actor] > 0;

                    bool UserCooldownSnowballActive = FreezeManager.FreezeActorCooldownSnowballTime.ContainsKey(Actor)
                        && FreezeManager.FreezeActorCooldownSnowballTime[Actor] > 0;

                    if (!FreezeManager.FreezeStarted ||
                        !UserIsPlaying ||
                        UserFreezed ||
                        UserCooldownSnowballActive)
                    {
                        break;
                    }

                    FreezePowerUp ActorPowerUp = FreezeManager.FreezeActorPowerUp.ContainsKey(Actor) ?
                        FreezeManager.FreezeActorPowerUp[Actor] : FreezePowerUp.None;

                    if (ActorPowerUp != FreezePowerUp.Snowballs)
                    {
                        FreezeManager.FreezeActorCooldownSnowballTime[Actor] = 5;
                    }

                    FreezeFirePower ActorFirePower = FreezeManager.ActorFreezeFirePower(ActorPowerUp);

                    Item.Flags = ((int)ActorFirePower * 1000).ToString();
                    Item.DisplayFlags = Item.Flags;
                    Item.BroadcastStateUpdate(Instance);

                    Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                    if (Session != null)
                    {
                        QuestManager.ProgressUserQuest(Session, QuestType.FREEZE_THROW_BOMB);
                    }

                    List<Item> AffectedItems = new List<Item>();

                    switch (ActorPowerUp)
                    {
                        case FreezePowerUp.OrangeSnowball:

                            AffectedItems.AddRange(FreezeManager.GetLineItems(Item.RoomPosition.GetVector2(), ActorFirePower));
                            AffectedItems.AddRange(FreezeManager.GetDiagonalLineItems(Item.RoomPosition.GetVector2(), ActorFirePower));
                            break;

                        case FreezePowerUp.GreenArrow:

                            AffectedItems.AddRange(FreezeManager.GetDiagonalLineItems(Item.RoomPosition.GetVector2(), ActorFirePower));
                            break;

                        default:

                            AffectedItems.AddRange(FreezeManager.GetLineItems(Item.RoomPosition.GetVector2(), ActorFirePower));
                            break;
                    }

                    foreach (Item AffectedItem in AffectedItems)
                    {
                        if (AffectedItem.Definition.Behavior == ItemBehavior.FreezeTile)
                        {
                            if (!AffectedItem.TemporaryInteractionReferenceIds.ContainsKey(0))
                            { 
                                AffectedItem.TemporaryInteractionReferenceIds.Add(0, Actor.ReferenceId);
                            }

                            AffectedItem.TemporaryInteractionReferenceIds[0] = Actor.ReferenceId;
                        }

                        ItemEventDispatcher.InvokeItemEventHandler(Actor, AffectedItem, Instance, ItemEventType.ItemInvoking);
                    }

                    if (FreezeManager.FreezeActorPowerUp.ContainsKey(Actor) &&
                        FreezeManager.FreezeActorPowerUp[Actor] > FreezePowerUp.None)
                    {
                        bool ClearPowerUp = true;

                        if (FreezeManager.FreezeActorSnowballsPowerUpCount.ContainsKey(Actor))
                        {
                            FreezeManager.FreezeActorSnowballsPowerUpCount[Actor]--;
                            ClearPowerUp = FreezeManager.FreezeActorSnowballsPowerUpCount[Actor] == 0;

                            if (ClearPowerUp)
                            {
                                FreezeManager.FreezeActorSnowballsPowerUpCount.Remove(Actor);
                            }
                        }

                        if (ClearPowerUp)
                        {
                            FreezeManager.FreezeActorPowerUp[Actor] = FreezePowerUp.None;
                        }
                    }

                    Item.RequestUpdate(5);
                    break;
            }

            return true;
        }

        private static bool HandleFreezeBlock(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            GameManager FreezeManager = Instance.GameManager;

            switch (Event)
            {
                case ItemEventType.Placed:
                case ItemEventType.InstanceLoaded:

                    Item.Flags = "";
                    Item.DisplayFlags = Item.Flags;
                    Item.BroadcastStateUpdate(Instance);

                    Item.LockTileSquare = true;
                    Instance.RegenerateRelativeHeightmap(true);

                    break;

                case ItemEventType.UpdateTick:

                    if (!FreezeManager.FreezeStarted)
                    {
                        int.TryParse(Item.Flags, out int CurrentFlags);

                        Item.Flags = (CurrentFlags + 10000).ToString();
                        Item.DisplayFlags = Item.Flags;
                        Item.BroadcastStateUpdate(Instance);
                        break;
                    }

                    if (Item.LockTileSquare)
                    {
                        FreezePowerUp GeneratePowerUp = FreezeManager.GenerateRandomPowerUp();

                        Item.Flags = (((int)GeneratePowerUp * 1000) + 1000).ToString();
                        Item.DisplayFlags = Item.Flags;
                        Item.BroadcastStateUpdate(Instance);

                        Item.LockTileSquare = false;
                        Instance.RegenerateRelativeHeightmap(true);
                    }

                    List<RoomActor> Actors = Instance.GetActorsOnPosition(Item.RoomPosition.GetVector2());

                    if (Actors != null && Actors.Count > 0)
                    {
                        int.TryParse(Item.Flags, out int CurrentFlags);

                        FreezePowerUp FreezePowerUp = CurrentFlags < 10000 ? ((FreezePowerUp)(CurrentFlags / 1000) - 1) : FreezePowerUp.None;

                        if (FreezePowerUp > FreezePowerUp.None)
                        {
                            RoomActor PickUpActor = Actors.FirstOrDefault();

                            FreezeManager.PickUpPowerUp(PickUpActor, FreezePowerUp);
                            Item.Flags = (CurrentFlags + 10000).ToString();
                            Item.DisplayFlags = Item.Flags;
                            Item.BroadcastStateUpdate(Instance);
                            break;
                        }
                    }

                    Item.RequestUpdate(1);
                    break;

                case ItemEventType.ItemInvoking:

                    Item.RequestUpdate(5);
                    break;
            }

            return true;
        }

        private static bool HandleFreezeExitTile(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            GameManager FreezeManager = Instance.GameManager;

            switch (Event)
            {
                case ItemEventType.Placed:
                case ItemEventType.InstanceLoaded:

                    Item.Flags = "0";
                    Item.DisplayFlags = Item.Flags;
                    Item.BroadcastStateUpdate(Instance);

                    Item.RequestUpdate(1);
                    break;

                case ItemEventType.UpdateTick:

                    int.TryParse(Item.Flags, out int CurrentState);

                    int NewState = FreezeManager.FreezeStarted ? 1 : 0;

                    if (CurrentState != NewState)
                    {
                        Item.Flags = NewState.ToString();
                        Item.DisplayFlags = Item.Flags;
                        Item.BroadcastStateUpdate(Instance);
                    }

                    Item.RequestUpdate(1);
                    break;
            }

            return true;
        }

        private static bool HandleFreezeScoreboard(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.ItemInvoking:

                    int.TryParse(Item.Flags, out int CurrentValue);
                    int NewValue = RequestData != 0 ? CurrentValue + RequestData : 0;

                    if (NewValue != CurrentValue)
                    {
                        Item.Flags = NewValue.ToString();
                        Item.DisplayFlags = Item.Flags;

                        RoomManager.MarkWriteback(Item, true);
                        Item.BroadcastStateUpdate(Instance);
                    }

                    break;
            }

            return true;
        }
    }
}
