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
using System.Reflection;
using System.Web.ModelBinding;


namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class BanzaiHandler
    {
        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.BanzaiGate, new ItemEventHandler(HandleBanzaiGate));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.BanzaiPuck, new ItemEventHandler(HandleBanzaiPuck));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.BanzaiTile, new ItemEventHandler(HandleBanzaiTile));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.BanzaiSphere, new ItemEventHandler(HandleBanzaiSphere));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.BanzaiPyramid, new ItemEventHandler(HandleBanzaiPyramid));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.BanzaiScore, new ItemEventHandler(HandleBanzaiScoreboard));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.BanzaiTeleporter, new ItemEventHandler(HandleBanzaiTeleporter));
        }

        private static bool HandleBanzaiGate(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            GameManager BanzaiManager = Instance.GameManager;
            TeamColors GateColor = BanzaiManager.GateColor(GameType.BattleBanzai, Item);
            int ClearEffect = 0;
            int TeamLimit = 5;

            switch (Event)
            {

                case ItemEventType.Placed:
                case ItemEventType.InstanceLoaded:

                    Item.RequestUpdate(1);
                    break;

                case ItemEventType.UpdateTick:

                    Item.LockTileSquare = BanzaiManager.BanzaiStarted;

                    if (Item.LockTileSquare)
                    {
                        BanzaiManager.CheckIfHasStuckActors(Item);
                    }

                    int.TryParse(Item.Flags, out int CurrentState);

                    int NewState = BanzaiManager.GameTeamPlayerCount(GameType.BattleBanzai, GateColor);

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

                    BanzaiManager.RemoveTeamColor(GameType.BattleBanzai, GateColor);
                    break;

                case ItemEventType.WalkOnItem:

                    if (!BanzaiManager.BanzaiStarted)
                    {
                        int TeamColorPlayerCount = BanzaiManager.GameTeamPlayerCount(GameType.BattleBanzai, GateColor);

                        bool IsPlayingBanzai = Actor.PlayingGameType == GameType.BattleBanzai;
                        bool IsSameTeam = Actor.PlayerTeamColor == GateColor;

                        bool ShouldClear = IsPlayingBanzai && IsSameTeam;

                        if (ShouldClear)
                        {
                            Actor.PlayingGameType = GameType.None;
                            Actor.PlayerTeamColor = TeamColors.None;
                        }
                        else if (TeamColorPlayerCount < TeamLimit)
                        {
                            Actor.PlayingGameType = GameType.BattleBanzai;
                            Actor.PlayerTeamColor = GateColor;

                            Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                            if (Session != null)
                            {
                                QuestManager.ProgressUserQuest(Session, QuestType.BANZAI_JOIN);
                            }
                        }

                        int ApplyEffect = 32 + (int)GateColor;

                        TeamColors AlterGameColor = ShouldClear ? TeamColors.None : GateColor;

                        int AlterOrClearEffect = AlterGameColor == TeamColors.None ? ClearEffect : ApplyEffect;

                        Actor.ApplyEffect(AlterOrClearEffect, true);
                    }

                    Item.RequestUpdate(2);
                    break;
            }

            return true;
        }

        private static bool HandleBanzaiPyramid(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            GameManager BanzaiManager = Instance.GameManager;

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

                    if (!BanzaiManager.BanzaiStarted)
                    {
                        goto case ItemEventType.InstanceLoaded;
                    }

                    int Random = RandomGenerator.GetNext(0, 30);

                    if (Random == 15)
                    {
                        int.TryParse(Item.Flags, out int CurrentState);

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

        private static bool HandleBanzaiTile(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            GameManager BanzaiManager = Instance.GameManager;

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

                    if (!BanzaiManager.BanzaiStarted)
                    {
                        if (Item.InteractHelper == 3)
                        {
                            if (!BanzaiManager.BanzaiHelpStateHolder.ContainsKey(Item.Id))
                            {
                                BanzaiManager.BanzaiHelpBlinkCount.Add(Item.Id, 10);
                                BanzaiManager.BanzaiHelpStateHolder.Add(Item.Id, Item.Flags);
                            }

                            string OldFlags = BanzaiManager.BanzaiHelpStateHolder[Item.Id].ToString();

                            int.TryParse(OldFlags, out int OldState);
                            TeamColors ItemColor = BanzaiManager.BanzaiTileColor(OldState);

                            if (ItemColor == BanzaiManager.Winner(GameType.BattleBanzai))
                            {
                                bool OddEven = BanzaiManager.BanzaiHelpBlinkCount[Item.Id] % 2 == 0;

                                Item.Flags = OddEven ? "0" : OldFlags;
                                Item.DisplayFlags = Item.Flags;
                                Item.BroadcastStateUpdate(Instance);

                                BanzaiManager.BanzaiHelpBlinkCount[Item.Id]--;

                                if (BanzaiManager.BanzaiHelpBlinkCount[Item.Id] == 0)
                                {
                                    BanzaiManager.UpdateBanzaiTileState(BanzaiTileStatus.PowerOff);
                                }
                            }
                        }
                    }

                    Item.RequestUpdate(2);
                    break;

                case ItemEventType.ItemInvoking:

                    int.TryParse(Item.Flags, out int TileState);

                    if (TileState != RequestData)
                    {
                        Item.Flags = RequestData.ToString();
                        Item.DisplayFlags = Item.Flags;
                        Item.BroadcastStateUpdate(Instance);
                    }

                    break;

                case ItemEventType.WalkOnItem:

                    bool ActorIsPlaying = Actor.PlayingGameType == GameType.BattleBanzai && Actor.PlayerTeamColor != TeamColors.None;

                    if (!BanzaiManager.BanzaiStarted ||
                        !ActorIsPlaying)
                    {
                        break;
                    }

                    TeamColors ActorTeamColor = Actor.PlayerTeamColor;
                    int.TryParse(Item.Flags, out int CurrentState);

                    TeamColors CurrentColor = BanzaiManager.BanzaiTileColor(CurrentState);

                    if (Item.InteractHelper < 3)
                    {
                        if (ActorTeamColor != CurrentColor)
                        {
                            Item.InteractHelper = 0;
                        }

                        Item.InteractHelper++;

                        int NewState = Item.InteractHelper + ((int)ActorTeamColor * 3) - 1;

                        if (NewState != CurrentState)
                        {
                            Item.Flags = NewState.ToString();
                            Item.DisplayFlags = Item.Flags;
                            Item.BroadcastStateUpdate(Instance);
                        }

                        if (Item.InteractHelper == 3)
                        {
                            BanzaiManager.HandleBanzaiTileLock(Actor, Item, NewState);
                        }
                    }

                    break;
            }

            return true;
        }

        private static bool HandleBanzaiSphere(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            GameManager BanzaiManager = Instance.GameManager;

            switch (Event)
            {
                case ItemEventType.Placed:
                case ItemEventType.InstanceLoaded:

                    Item.Flags = "0";
                    Item.DisplayFlags = Item.Flags;
                    Item.BroadcastStateUpdate(Instance);

                    RoomManager.MarkWriteback(Item, true);
                    Item.RequestUpdate(1);
                    break;

                case ItemEventType.UpdateTick:

                    if (BanzaiManager.BanzaiStarted)
                    {
                        if (RequestData > 0)
                        {
                            if (!BanzaiManager.BanzaiHelpBlinkCount.ContainsKey(Item.Id))
                            {
                                BanzaiManager.BanzaiHelpBlinkCount.Add(Item.Id, 4);
                                BanzaiManager.BanzaiHelpStateHolder.Add(Item.Id, RequestData.ToString());
                            }
                            else
                            {
                                BanzaiManager.BanzaiHelpBlinkCount[Item.Id] = 4;
                                BanzaiManager.BanzaiHelpStateHolder[Item.Id] = RequestData.ToString();
                            }

                        }

                        bool ContainsOldState = BanzaiManager.BanzaiHelpStateHolder.ContainsKey(Item.Id);

                        string Flags = ContainsOldState ? BanzaiManager.BanzaiHelpStateHolder[Item.Id] : Item.Flags;

                        int.TryParse(Flags, out int SphereState);

                        bool Execute = BanzaiManager.BanzaiHelpBlinkCount.ContainsKey(Item.Id);

                        if (Execute)
                        {
                            Item.Flags = SphereState.ToString();
                            Item.DisplayFlags = Item.Flags;
                            Item.BroadcastStateUpdate(Instance);

                            BanzaiManager.BanzaiHelpBlinkCount[Item.Id]--;

                            if (BanzaiManager.BanzaiHelpBlinkCount[Item.Id] == 0)
                            {
                                BanzaiManager.UpdateBanzaiSphere(BanzaiSphere.BlinkLights);
                            }
                        }
                    }

                    if (!BanzaiManager.BanzaiStarted)
                    {
                        bool ContainsOldState = BanzaiManager.BanzaiHelpStateHolder.ContainsKey(Item.Id);

                        string Flags = ContainsOldState ? BanzaiManager.BanzaiHelpStateHolder[Item.Id] : Item.Flags;

                        int.TryParse(Flags, out int SphereState);

                        bool ItemIsActive = (BanzaiSphere)SphereState > BanzaiSphere.PowerOff;

                        if (ItemIsActive)
                        {
                            if (!BanzaiManager.BanzaiHelpBlinkCount.ContainsKey(Item.Id))
                            {
                                BanzaiManager.BanzaiHelpBlinkCount.Add(Item.Id, 10);
                                BanzaiManager.BanzaiHelpStateHolder.Add(Item.Id, Item.Flags);
                            }
                            else
                            {
                                BanzaiManager.BanzaiHelpBlinkCount[Item.Id] = 10;
                                BanzaiManager.BanzaiHelpStateHolder[Item.Id] = Item.Flags;
                            }

                            bool OddEven = BanzaiManager.BanzaiHelpBlinkCount[Item.Id] % 2 == 0;

                            int Winner = (int)BanzaiManager.Winner(GameType.BattleBanzai) + 19;

                            Item.Flags = OddEven ? Winner.ToString() : "0";
                            Item.DisplayFlags = Item.Flags;
                            Item.BroadcastStateUpdate(Instance);

                            BanzaiManager.BanzaiHelpBlinkCount[Item.Id]--;

                            if (BanzaiManager.BanzaiHelpBlinkCount[Item.Id] == 0)
                            {
                                BanzaiManager.UpdateBanzaiSphere(BanzaiSphere.PowerOff);
                            }
                        }
                    }

                    Item.RequestUpdate(2);
                    break;

                case ItemEventType.ItemInvoking:

                    int.TryParse(Item.Flags, out int CurrentState);

                    int NewState = RequestData;

                    if (CurrentState != NewState)
                    {
                        Item.Flags = NewState.ToString();
                        Item.DisplayFlags = Item.Flags;

                        Item.BroadcastStateUpdate(Instance);
                        Instance.RegenerateRelativeHeightmap(true);
                    }

                    break;
            }

            return true;
        }

        private static bool HandleBanzaiTeleporter(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            GameManager BanzaiManager = Instance.GameManager;

            switch (Event)
            {
                case ItemEventType.Placed:
                case ItemEventType.InstanceLoaded:

                    Item.Flags = "0";
                    Item.DisplayFlags = Item.Flags;
                    Item.BroadcastStateUpdate(Instance);

                    RoomManager.MarkWriteback(Item, true);
                    Item.RequestUpdate(1);
                    break;

                case ItemEventType.UpdateTick:

                    uint ActorId = Item.TemporaryInteractionReferenceIds.ContainsKey(0) ?
                        Item.TemporaryInteractionReferenceIds[0] : 0;

                    RoomActor Interactor = Instance.GetActorByReferenceId(ActorId);

                    Item.Flags = Interactor != null ? "1" : "0";
                    Item.DisplayFlags = Item.Flags;
                    Item.BroadcastStateUpdate(Instance);

                    if (Interactor != null)
                    {
                        Interactor.PositionToSet = Item.RoomPosition.GetVector2();
                        Interactor.MoveTo(Interactor.PositionToSet);
                        Interactor.UpdateNeeded = true;

                        Interactor.UnblockWalking();

                        Item.TemporaryInteractionReferenceIds.Remove(0);
                    }

                    Item.RequestUpdate(2);
                    break;

                case ItemEventType.WalkOnItem:

                    if (!BanzaiManager.BanzaiStarted)
                    {
                        break;
                    }

                    List<Item> TeleporterItems = Instance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.BanzaiTeleporter && I.Id != Item.Id).ToList();

                    if (TeleporterItems.Count > 0)
                    {
                        Item.Flags = "1";
                        Item.DisplayFlags = Item.Flags;
                        Item.BroadcastStateUpdate(Instance);

                        int ListCount = TeleporterItems.Count - 1;
                        int Random = RandomGenerator.GetNext(0, ListCount);

                        Item RandomTeleporter = TeleporterItems[Random];

                        if (RandomTeleporter != null)
                        {
                            Actor.BlockWalking();

                            RandomTeleporter.TemporaryInteractionReferenceIds.Add(0, Actor.ReferenceId);
                            RandomTeleporter.RequestUpdate(2);
                        }
                    }

                    Item.RequestUpdate(2);
                    break;
            }

            return true;
        }

        private static bool HandleBanzaiPuck(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            GameManager BanzaiManager = Instance.GameManager;

            switch (Event)
            {
                case ItemEventType.Placed:
                case ItemEventType.InstanceLoaded:

                    Item.Flags = "0";
                    Item.DisplayFlags = Item.Flags;
                    Item.BroadcastStateUpdate(Instance);

                    RoomManager.MarkWriteback(Item, true);
                    Item.RequestUpdate(1);
                    break;

                case ItemEventType.UpdateTick:

                    Item.RequestUpdate(2);
                    break;

                case ItemEventType.WalkOnItem:

                    List<RollerEvents> BanzaiPuckEvents = new List<RollerEvents>();

                    int PlayerDistance = Distance.Calculate(Actor.FromPosition, Item.RoomPosition.GetVector2());

                    int KickPower = RandomGenerator.GetNext(1, PlayerDistance);

                    int TempX = 0;
                    int TempY = 0;

                    switch (Actor.BodyRotation)
                    {
                        case 4:

                            TempY = 1;
                            break;

                        case 0:

                            TempY = -1;
                            break;

                        case 6:

                            TempX = -1;
                            break;

                        case 2:

                            TempX = 1;
                            break;

                        case 3:

                            TempX = 1;
                            TempY = 1;
                            break;

                        case 1:

                            TempX = 1;
                            TempY = -1;
                            break;

                        case 7:

                            TempX = -1;
                            TempY = -1;
                            break;

                        case 5:

                            TempX = -1;
                            TempY = 1;
                            break;

                    }

                    Vector2 UpdatedPos = Item.RoomPosition.GetVector2();
                    BanzaiManager.UpdateBanzaiTileState(Actor, UpdatedPos);

                    for (int i = 0; i < KickPower; i++)
                    {
                        Vector2 BouncePosition = new Vector2(UpdatedPos.X + TempX, UpdatedPos.Y + TempY);

                        if (Instance.CanMoveItem(BouncePosition))
                        {
                            UpdatedPos = BouncePosition;

                            BanzaiPuckEvents.Add(new RollerEvents(Item.RoomPosition.Z, Instance.GetUserStepHeight(UpdatedPos), 0, Item.Id));

                            BanzaiManager.UpdateBanzaiTileState(Actor, UpdatedPos);
                        }
                        else
                        {
                            Vector2 TestX = new Vector2(UpdatedPos.X + TempX, UpdatedPos.Y);
                            Vector2 TestY = new Vector2(UpdatedPos.X, UpdatedPos.Y + TempY);

                            bool CanMoveX = !Instance.CanMoveItem(TestX);
                            bool CanMoveY = !Instance.CanMoveItem(TestY);

                            if (CanMoveX && CanMoveY)
                            {
                                TempX = -TempX;
                                TempY = -TempY;
                            }
                            else if (CanMoveX)
                            {
                                TempX = -TempX;
                            }
                            else if (CanMoveY)
                            {
                                TempY = -TempY;
                            }
                            else
                            {
                                TempX = -TempX;
                                TempY = -TempY;
                            }

                            BouncePosition = new Vector2(UpdatedPos.X + TempX, UpdatedPos.Y + TempY);

                            if (Instance.CanMoveItem(BouncePosition))
                            {
                                UpdatedPos = BouncePosition;

                                BanzaiPuckEvents.Add(new RollerEvents(Item.RoomPosition.Z, Instance.GetUserStepHeight(UpdatedPos), 0, Item.Id));

                                BanzaiManager.UpdateBanzaiTileState(Actor, UpdatedPos);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (UpdatedPos != Item.RoomPosition.GetVector2())
                    {
                        if (BanzaiManager.BanzaiStarted)
                        {
                            Item.DisplayFlags = ((int)Actor.PlayerTeamColor).ToString();
                            Item.BroadcastStateUpdate(Instance);

                            Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                            if (Session != null)
                            {
                                QuestManager.ProgressUserQuest(Session, QuestType.BANZAI_PUSH_PUCK);
                            }
                        }

                        double Z = Instance.GetUserStepHeight(UpdatedPos);
                        Vector3 TargetVector3 = new Vector3(UpdatedPos.X, UpdatedPos.Y, Z);

                        Instance.BroadcastMessage(RollerEventComposer.Compose(Item.RoomPosition.GetVector2(), UpdatedPos, 0, BanzaiPuckEvents));

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            Item.MoveToRoom(MySqlClient, Item.RoomId, TargetVector3, Item.RoomRotation);
                        }

                        Instance.RegenerateRelativeHeightmap(true);
                    }

                    break;
            }

            return true;
        }

        private static bool HandleBanzaiScoreboard(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
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
