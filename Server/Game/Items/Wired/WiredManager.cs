using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Timers;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Bots;
using Snowlight.Game.Misc;
using Snowlight.Game.Rooms;
using Snowlight.Specialized;
using Snowlight.Game.Sessions;
using Snowlight.Communication;
using Snowlight.Communication.Incoming;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Items.Wired
{
    public enum WiredTriggerTypes
    {
        says_something = 0,
        walks_on_furni = 1,
        walks_off_furni = 2,
        at_given_time = 3,
        state_changed = 4,
        periodically = 6,
        enter_room = 7,
        game_starts = 8,
        game_ends = 9,
        score_achieved = 10
    }

    public enum WiredActionTypes
    {
        toggle_state = 0,
        match_to_sshot = 3,
        move_rotate = 4,
        give_score = 6,
        show_message = 7,
        teleport_to = 8,
        reset_timer = 10
    }

    public enum WiredConditionTypes
    {
        match_snapshot = 0,
        furnis_hv_avatars = 1,
        trgger_on_frn = 2,
        time_more_than = 3,
        time_less_than = 4,
        has_furni_on = 7
    }

    public enum WiredAddonTypes
    {
        random = 0, // ??
        unseen = 0  // ??
    }

    public static class WiredTypesUtil
    {
        public static WiredTriggerTypes TriggerFromInt(int Type)
        {
            switch (Type)
            {
                default:
                case 0:

                    return WiredTriggerTypes.says_something;

                case 1:

                    return WiredTriggerTypes.walks_on_furni;

                case 2:

                    return WiredTriggerTypes.walks_off_furni;

                case 3:

                    return WiredTriggerTypes.at_given_time;

                case 4:

                    return WiredTriggerTypes.state_changed;

                case 6:
                    
                    return WiredTriggerTypes.periodically;

                case 7:

                    return WiredTriggerTypes.enter_room;

                case 8:

                    return WiredTriggerTypes.game_starts;

                case 9:

                    return WiredTriggerTypes.game_ends;

                case 10:

                    return WiredTriggerTypes.score_achieved;
                
            }
        }

        public static WiredActionTypes ActionFromInt(int Type)
        {
            switch (Type)
            {
                default:
                case 0:

                    return WiredActionTypes.toggle_state;

                case 3:

                    return WiredActionTypes.match_to_sshot;

                case 4:

                    return WiredActionTypes.move_rotate;

                case 6:

                    return WiredActionTypes.give_score;

                case 7:

                    return WiredActionTypes.show_message;

                case 8:

                    return WiredActionTypes.teleport_to;

                case 10:

                    return WiredActionTypes.reset_timer;

            }
        }

        public static WiredConditionTypes ConditionFromInt(int Type)
        {
            switch (Type)
            {
                default:
                case 0:

                    return WiredConditionTypes.match_snapshot;

                case 1:

                    return WiredConditionTypes.furnis_hv_avatars;

                case 2:

                    return WiredConditionTypes.trgger_on_frn;

                case 3:

                    return WiredConditionTypes.time_more_than;

                case 4:

                    return WiredConditionTypes.time_less_than;

                case 7:

                    return WiredConditionTypes.has_furni_on;

            }
        }

        public static WiredAddonTypes AddonFromInt(int Type) 
        {
            switch(Type) 
            {
                default:
                case 0:

                    return WiredAddonTypes.unseen;

                case 1:

                    return WiredAddonTypes.random;
            }
        }
    }

    public class WiredManager
    {
        private Dictionary<uint, WiredData> mWired;
        private RoomInstance mInstance;
        private Dictionary<uint, uint> mRegisteredWalkItems;

        public WiredManager(RoomInstance Instance)
        {
            mInstance = Instance;
            mWired = new Dictionary<uint, WiredData>();
            mRegisteredWalkItems = new Dictionary<uint, uint>();
        }

        public WiredData LoadWired(uint ItemId, int Type)
        {

            if (!mWired.ContainsKey(ItemId))
            {
                mWired.Add(ItemId, new WiredData(ItemId, Type));
            }


            return mWired[ItemId];
        }

        public void RemoveWired(uint ItemId, SqlDatabaseClient MySqlClient)
        {
            if (mWired.ContainsKey(ItemId))
            {
                mWired.Remove(ItemId);
                DeRegisterWalkItems(ItemId);
                MySqlClient.SetParameter("id", ItemId);
                MySqlClient.ExecuteNonQuery("DELETE FROM wired_items WHERE item_id = @id");
            }
        }

        public void SynchronizeDatabase(SqlDatabaseClient MySqlClient)
        {
            foreach (WiredData data in mWired.Values)
            {
                data.SynchronizeDatabase(MySqlClient);
            }
        }

        public void HandleSave(Session Session, ClientMessage Message)
        {
            uint ItemId = Message.PopWiredUInt32();

            if (!mInstance.CheckUserRights(Session) || !mWired.ContainsKey(ItemId))
            {
                return;
            }

            Item item = mInstance.GetItem(ItemId);

            if (item == null)
            {
                return;
            }

            WiredData data = mWired[ItemId];

            string Data1 = "";
            int Data2 = 0;
            int Data3 = 0;
            int Data4 = 0;
            int Time = 0;
            string Data5 = "";

            Message.PopWiredInt32();
            Data2 = Message.PopWiredInt32();

            bool Simple = true;

            if (item.Definition.Behavior == ItemBehavior.WiredEffect)
            {
                switch (WiredTypesUtil.ActionFromInt(item.Definition.BehaviorData))
                {
                    case WiredActionTypes.match_to_sshot:
                    case WiredActionTypes.move_rotate:
                    case WiredActionTypes.teleport_to:
                    case WiredActionTypes.toggle_state:
                        Simple = false;
                        break;
                }
            }

            if (item.Definition.Behavior == ItemBehavior.WiredTrigger)
            {
                switch (WiredTypesUtil.TriggerFromInt(item.Definition.BehaviorData))
                {
                    case WiredTriggerTypes.state_changed:
                    case WiredTriggerTypes.walks_off_furni:
                    case WiredTriggerTypes.walks_on_furni:
                        Simple = false;
                        break;
                    case WiredTriggerTypes.periodically:
                        item.RequestUpdate(Data2);
                        break;
                }
            }

            if (!Simple)
            {
                Data3 = Message.PopWiredInt32();

                if (item.Definition.Behavior == ItemBehavior.WiredEffect && WiredTypesUtil.ActionFromInt(item.Definition.BehaviorData) == WiredActionTypes.match_to_sshot)
                {
                    Data4 = Message.PopWiredInt32();
                }

                Message.PopString();
                int c = Message.PopWiredInt32();
                for (int i = 0; i < c; i++)
                {
                    uint tmp = Message.PopWiredUInt32();
                    if (mInstance.GetItem(tmp) == null)
                    {
                        continue;
                    }
                    if (tmp != 0)
                    {
                        Data1 += "" + tmp.ToString() + "|";
                    }
                }

                Time = Message.PopWiredInt32();
            }
            else
            {
                Data1 = Message.PopString();
                Data3 = Message.PopWiredInt32();
            }


            if (item.Definition.Behavior == ItemBehavior.WiredEffect)
            {
                switch (WiredTypesUtil.ActionFromInt(item.Definition.BehaviorData))
                {
                    case WiredActionTypes.match_to_sshot:
                        string[] Selected = Data1.Split('|');

                        foreach (string ItemIdS in Selected)
                        {
                            uint SelectedItemId;
                            uint.TryParse(ItemIdS, out SelectedItemId);
                            Item Item = mInstance.GetItem(SelectedItemId);
                            if (Item == null)
                            {
                                continue;
                            }

                            Data5 += Item.Id + "#" + Item.RoomPosition.ToString() + "#" + Item.RoomRotation + "#" + Item.DisplayFlags + "+";
                        }
                        break;
                }
            }

            if (data.Data1 == Data1 && data.Data2 == Data2 && data.Data3 == Data3 && data.Data4 == Data4 && data.Time == Time && data.Data5 == Data5)
            {
                return;
            }

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {

                data.Data1 = Data1;
                data.Data2 = Data2;
                data.Data3 = Data3;
                data.Data4 = Data4;
                data.Data5 = Data5;
                data.Time = Time;
                data.SynchronizeDatabase(MySqlClient);
            }

            if (item.Definition.Behavior == ItemBehavior.WiredTrigger)
            {
                switch (WiredTypesUtil.TriggerFromInt(item.Definition.BehaviorData))
                {
                    case WiredTriggerTypes.at_given_time:
                        item.RequestUpdate(Data2);
                        break;
                    case WiredTriggerTypes.walks_on_furni:
                    case WiredTriggerTypes.walks_off_furni:
                        DeRegisterWalkItems(item.Id);
                        RegisterWalkItems(item.Id);
                        break;
                }
            }

        }

        public uint GetRegisteredWalkItem(uint Id)
        {
            if (mRegisteredWalkItems.ContainsKey(Id))
            {
                return mRegisteredWalkItems[Id];
            }
            return 0;
        }

        public void RegisterWalkItems(uint ItemId)
        {

            string[] Selected = mWired[ItemId].Data1.Split('|');

            foreach (string ItemIdS in Selected)
            {
                uint Id;
                uint.TryParse(ItemIdS, out Id);
                Item check = mInstance.GetItem(Id);
                if (check == null)
                {
                    continue;
                }

                if (!mRegisteredWalkItems.ContainsKey(Id))
                {
                    mRegisteredWalkItems.Add(Id, ItemId);
                }
            }
        }

        private void DeRegisterWalkItems(uint ItemId)  // DeRegister by WiredItem
        {
            if (!mRegisteredWalkItems.ContainsValue(ItemId))
            {
                return;
            }

            List<uint> ToRemove = new List<uint>();
            foreach (uint Id in mRegisteredWalkItems.Keys)
            {
                if (mRegisteredWalkItems[Id] == ItemId)
                {
                    ToRemove.Add(Id);
                }
            }

            foreach (uint Id in ToRemove)
            {
                if (mRegisteredWalkItems.ContainsKey(Id))
                {
                    mRegisteredWalkItems.Remove(Id);
                }
            }
        }

        public void DeRegisterWalkItem(uint Id)  // Deregister by Walkable Item
        {
            if (mRegisteredWalkItems.ContainsKey(Id))
            {
                mRegisteredWalkItems.Remove(Id);
            }
        }

        public void HandleEnterRoom(RoomActor Actor)
        {
            foreach (WiredData data in mWired.Values)
            {
                Item Item = mInstance.GetItem(data.ItemId);
                if (Item.Definition.Behavior == ItemBehavior.WiredTrigger && WiredTypesUtil.TriggerFromInt(Item.Definition.BehaviorData) == WiredTriggerTypes.enter_room)
                {
                    if (data.Data1 != "" && data.Data1 != Actor.Name)
                    {
                        continue;
                    }

                    Item.DisplayFlags = "1";
                    Item.BroadcastStateUpdate(mInstance);
                    Item.DisplayFlags = "";
                    Item.RequestUpdate(4);

                    ExecuteActions(Item, Actor);
                }
            }
        }

        public void HandleToggleState(RoomActor Actor, Item TheItemChanged)
        {
            foreach (WiredData data in mWired.Values)
            {
                Item Item = mInstance.GetItem(data.ItemId);
                if (Item.Definition.Behavior == ItemBehavior.WiredTrigger && WiredTypesUtil.TriggerFromInt(Item.Definition.BehaviorData) == WiredTriggerTypes.state_changed)
                {
                    string[] Selected3 = Item.WiredData.Data1.Split('|');

                    foreach (string ItemIdS2 in Selected3)
                    {
                        uint.TryParse(ItemIdS2, out uint ItemId3);
                        
                        Item AffectedItem3 = mInstance.GetItem(ItemId3);
                        if (AffectedItem3 == null)
                        {
                            continue;
                        }

                        if (AffectedItem3 == TheItemChanged)
                        {
                            Item.DisplayFlags = "1";
                            Item.BroadcastStateUpdate(mInstance);
                            Item.DisplayFlags = "";
                            Item.RequestUpdate(4);

                            ExecuteActions(Item, Actor);
                        }
                    }
                }
            }
        }

        public List<Item> TriggerRequiresActor(int BehaviorData, Vector2 Position)
        {
            List<Item> Items = new List<Item>();

            if (WiredTypesUtil.TriggerFromInt(BehaviorData) != WiredTriggerTypes.periodically)
            {
                return Items;
            }


            foreach (Item Item in mInstance.GetItemsOnPosition(Position))
            {
                if (Item.Definition.Behavior != ItemBehavior.WiredEffect)
                {
                    continue;
                }

                if (WiredTypesUtil.ActionFromInt(Item.Definition.BehaviorData) == WiredActionTypes.show_message)
                {
                    Items.Add(Item);
                }
            }

            return Items;
        }

        public List<Item> ActionRequiresActor(int BehaviorData, Vector2 Position)
        {
            List<Item> Items = new List<Item>();

            if (WiredTypesUtil.ActionFromInt(BehaviorData) != WiredActionTypes.show_message)
            {
                return Items;
            }

            foreach (Item Item in mInstance.GetItemsOnPosition(Position))
            {
                if (Item.Definition.Behavior != ItemBehavior.WiredTrigger)
                {
                    continue;
                }

                if (WiredTypesUtil.TriggerFromInt(Item.Definition.BehaviorData) == WiredTriggerTypes.periodically)
                {
                    Items.Add(Item);
                }
            }


            return Items;
        }

        public bool HandleChat(string Message, RoomActor Actor)
        {
            bool doneAction = false;
            foreach (WiredData data in mWired.Values)
            {
                Item Item = mInstance.GetItem(data.ItemId);

                if(Item == null)
                {
                    continue;
                }

                RoomInfo Info = RoomInfoLoader.GetRoomInfo(Item.RoomId);

                if (Item.Definition.Behavior == ItemBehavior.WiredTrigger &&
                    WiredTypesUtil.TriggerFromInt(Item.Definition.BehaviorData) == WiredTriggerTypes.says_something &&
                    Message.ToLower().Equals(data.Data1.ToLower()) && (data.Data2 == 0 || data.Data2 == 1 && Info.OwnerId == Actor.ReferenceId) && data.Data1 != ""
                    )
                {

                    Item.DisplayFlags = "1";
                    Item.BroadcastStateUpdate(mInstance);
                    Item.DisplayFlags = "2";
                    Item.RequestUpdate(4);

                    ExecuteActions(Item, Actor);
                    doneAction = true;
                }
            }
            return doneAction;
        }

        public bool HandlePeriodicly(Item Item, RoomActor Actor)
        {
            uint ItemID = Item.Id;
            int time = mWired[ItemID].Data2 * 100;
            time = time % 2;
            time = time * 10;
            if (time == 0)
            {
                time = 500;
            }
            System.Timers.Timer dispatcherTimer = new System.Timers.Timer(time);
            dispatcherTimer.Interval = time;
            dispatcherTimer.Elapsed += delegate { ExecuteActions(Item, Actor); };
            dispatcherTimer.Start();
            return true;
        }

        public void ExecuteActions(Item Item, RoomActor Actor)
        {
            try
            {
                Random rnd = new Random();
                
                foreach (Item ActionItem in mInstance.GetItemsOnPosition(Item.RoomPosition.GetVector2()))
                {
                    if (ActionItem.Definition.Behavior == ItemBehavior.WiredEffect)
                    {
                        ActionItem.DisplayFlags = "1";
                        ActionItem.BroadcastStateUpdate(mInstance);
                        ActionItem.DisplayFlags = "2";
                        ActionItem.RequestUpdate(4);

                        RoomInstance Instance = mInstance;
                        if (Actor != null)
                        {
                            Instance = RoomManager.GetInstanceByRoomId(Item.RoomId);
                        }

                        int time = 0;
                        if (mWired[ActionItem.Id].Time != 0)
                        {
                            time = mWired[ActionItem.Id].Time * 100;
                            time = time % 2;
                            time = time * 10;
                        }

                        switch (WiredTypesUtil.ActionFromInt(ActionItem.Definition.BehaviorData))
                        {
                            #region reset_timer
                            case WiredActionTypes.reset_timer:
                                // for every item in the room...
                                foreach (Item nItem in Instance.GetFloorItems())
                                {
                                    // if the item is a wired trigger
                                    if (nItem.Definition.Behavior == ItemBehavior.WiredTrigger)
                                    {
                                        //if the item is Trigger: At Set Time
                                        if (nItem.Definition.BehaviorData == 3)
                                        {
                                            //reset the timer
                                            nItem.RequestUpdate(nItem.WiredData.Data2);
                                        }
                                    }
                                }
                                break;
                            #endregion

                            #region show_message
                            case WiredActionTypes.show_message:
                                if (Actor == null)
                                {
                                    continue;
                                }
                                
                                System.Threading.Thread.Sleep(time);
                                Actor.Whisper(mWired[ActionItem.Id].Data1, 0, true);
                                break;
                            #endregion

                            #region move_rotate
                            case WiredActionTypes.move_rotate:
                                
                                if (ActionItem.WiredData.Data2 == 0 && ActionItem.WiredData.Data3 == 0)
                                {
                                    continue;
                                }

                                System.Threading.Thread.Sleep(time);
                                string[] ItemsToMove = ActionItem.WiredData.Data1.Split('|');
                                
                                foreach (string toMove in ItemsToMove)
                                {
                                    uint.TryParse(toMove, out uint ItemId);
                                    
                                    Item Move = mInstance.GetItem(ItemId);
                                    if (Move == null)
                                    {
                                        break;
                                    }

                                    Vector2 NewPosition = new Vector2(Move.RoomPosition.X, Move.RoomPosition.Y);
                                    Vector2 OldPosition = new Vector2(Move.RoomPosition.X, Move.RoomPosition.Y);

                                    switch (ActionItem.WiredData.Data2)
                                    {
                                        case 1:

                                            switch (rnd.Next(1, 4))
                                            {
                                                case 1:
                                                    NewPosition = new Vector2(Move.RoomPosition.X - 1, Move.RoomPosition.Y);
                                                    break;

                                                case 2:
                                                    NewPosition = new Vector2(Move.RoomPosition.X + 1, Move.RoomPosition.Y);
                                                    break;


                                                case 3:
                                                    NewPosition = new Vector2(Move.RoomPosition.X, Move.RoomPosition.Y + 1);
                                                    break;


                                                case 4:
                                                    NewPosition = new Vector2(Move.RoomPosition.X, Move.RoomPosition.Y - 1);
                                                    break;
                                            }

                                            break;

                                        case 2:

                                            if (rnd.Next(0, 2) == 1)
                                            {
                                                NewPosition = new Vector2(Move.RoomPosition.X - 1, Move.RoomPosition.Y);
                                            }
                                            else
                                            {
                                                NewPosition = new Vector2(Move.RoomPosition.X + 1, Move.RoomPosition.Y);
                                            }

                                            break;

                                        case 3:

                                            if (rnd.Next(0, 2) == 1)
                                            {
                                                NewPosition = new Vector2(Move.RoomPosition.X, Move.RoomPosition.Y - 1);
                                            }
                                            else
                                            {
                                                NewPosition = new Vector2(Move.RoomPosition.X, Move.RoomPosition.Y + 1);
                                            }

                                            break;

                                        case 4:

                                            NewPosition = new Vector2(Move.RoomPosition.X, Move.RoomPosition.Y - 1);
                                            
                                            break;

                                        case 5:
                                            
                                            NewPosition = new Vector2(Move.RoomPosition.X + 1, Move.RoomPosition.Y);
                                            
                                            break;

                                        case 6:
                                            
                                            NewPosition = new Vector2(Move.RoomPosition.X, Move.RoomPosition.Y + 1);
                                            
                                            break;
                                        
                                        case 7:
                                            
                                            NewPosition = new Vector2(Move.RoomPosition.X - 1, Move.RoomPosition.Y);
                                            
                                            break;
                                    }

                                    int NewRotation = Move.RoomRotation;

                                    switch (ActionItem.WiredData.Data3)
                                    {
                                        case 1:
                                            
                                            NewRotation += 2;
                                            
                                            if (NewRotation == 8)
                                            {
                                                NewRotation = 0;
                                            }

                                            break;

                                        case 2:
                                            
                                            NewRotation -= 2;
                                            
                                            if (NewRotation == -2)
                                            {
                                                NewRotation = 6;
                                            }
                                            
                                            break;
                                        
                                        case 3:

                                            if (rnd.Next(0, 1) == 0)
                                            {
                                                goto case 1;
                                            }
                                            else
                                            {
                                                goto case 2;
                                            }
                                    }



                                    bool IsRotationOnly = (ActionItem.WiredData.Data2 == 0);
                                    Vector3 FinalizedPosition = mInstance.SetFloorItem(null, Move, NewPosition, NewRotation);
                                    Vector3 FinalizedOld = mInstance.SetFloorItem(null, Move, OldPosition, NewRotation);

                                    if (FinalizedPosition != null && FinalizedOld != null && Instance.IsValidStep(Move.RoomPosition.GetVector2(), FinalizedPosition.GetVector2(), true))
                                    {
                                        Move.MoveToRoom(null, mInstance.RoomId, FinalizedPosition, NewRotation, "");
                                        RoomManager.MarkWriteback(Move, false);

                                        mInstance.RegenerateRelativeHeightmap();
                                        mInstance.BroadcastMessage(RoomItemUpdatedComposer.Compose(Move));
                                        mInstance.BroadcastMessage(RollerEventComposer.Compose(FinalizedOld.GetVector2(), FinalizedPosition.GetVector2(),
                                            Item.Id, new List<RollerEvents>() { new RollerEvents(FinalizedOld.Z, FinalizedPosition.Z, 0, Move.Id) }));
                                        ItemEventDispatcher.InvokeItemEventHandler(null, Move, mInstance, ItemEventType.Moved, IsRotationOnly ? 1 : 0);
                                    }
                                }
                                break;
                            #endregion

                            #region match_to_sshot
                            case WiredActionTypes.match_to_sshot:

                                string[] Selected = ActionItem.WiredData.Data5.Split('+');
                                foreach (string FullData in Selected)
                                {
                                    System.Threading.Thread.Sleep(time);
                                    if (!FullData.Contains('#'))
                                    {
                                        continue;
                                    }

                                    string[] Data = FullData.Split('#');
                                    if (Data.Length != 4)
                                    {
                                        continue;
                                    }

                                    uint Id = uint.Parse(Data[0]);
                                    string[] Position = Data[1].Split('|');
                                    int Rotation = int.Parse(Data[2]);
                                    string Flags = Data[3];

                                    int X = int.Parse(Position[0]);
                                    int Y = int.Parse(Position[1]);
                                    uint Z = uint.Parse(Position[2]);

                                    Item AffectedItem = mInstance.GetItem(Id);

                                    if (AffectedItem == null)
                                    {
                                        continue;
                                    }

                                    bool IsRotationOnly = (X == AffectedItem.RoomPosition.X && Y == AffectedItem.RoomPosition.Y && Z == AffectedItem.RoomPosition.Z);

                                    Vector2 NewPosition = new Vector2(X, Y);
                                    Vector2 OldPosition = new Vector2(AffectedItem.RoomPosition.X, AffectedItem.RoomPosition.Y);

                                    if (ActionItem.WiredData.Data2 == 1)
                                    {
                                        AffectedItem.Flags = Flags;
                                        AffectedItem.DisplayFlags = Item.Flags;
                                        AffectedItem.BroadcastStateUpdate(mInstance);
                                    }

                                    if (ActionItem.WiredData.Data3 == 0)
                                    {
                                        Rotation = AffectedItem.RoomRotation;
                                    }

                                    if (ActionItem.WiredData.Data4 == 0)
                                    {
                                        NewPosition = AffectedItem.RoomPosition.GetVector2();
                                    }

                                    if (ActionItem.WiredData.Data4 == 1 || ActionItem.WiredData.Data3 == 1)
                                    {
                                        Vector3 FinalizedPosition = mInstance.SetFloorItem(null, AffectedItem, NewPosition, Rotation);
                                        AffectedItem.MoveToRoom(null, mInstance.RoomId, FinalizedPosition, Rotation, "");

                                        RoomManager.MarkWriteback(AffectedItem, false);

                                        mInstance.RegenerateRelativeHeightmap();
                                        mInstance.BroadcastMessage(RoomItemUpdatedComposer.Compose(AffectedItem));

                                        ItemEventDispatcher.InvokeItemEventHandler(null, AffectedItem, mInstance, ItemEventType.Moved, IsRotationOnly ? 1 : 0);
                                    }
                                    else if (ActionItem.WiredData.Data2 == 1)
                                    {
                                        RoomManager.MarkWriteback(AffectedItem, true);
                                    }
                                }

                                break;
                            #endregion

                            #region teleport_to_furni
                            case WiredActionTypes.teleport_to:
                                
                                if (Actor == null)
                                {
                                    continue;
                                }

                                System.Threading.Thread.Sleep(time);
                                string[] Selected2 = ActionItem.WiredData.Data1.Split('|');
                                string ItemIdS = Actor.FurniOnId.ToString();

                                while (Actor.FurniOnId.ToString() == ItemIdS)
                                {
                                    ItemIdS = Selected2[rnd.Next(0, Selected2.Length - 1)];
                                }

                                uint.TryParse(ItemIdS, out uint ItemId2);
                                
                                Item AffectedItem2 = mInstance.GetItem(ItemId2);
                                if (AffectedItem2 == null)
                                {
                                    continue;
                                }

                                int OldEffect = Actor.AvatarEffectId;

                                Actor.BlockWalking();
                                Actor.ApplyEffect(4);

                                Actor.TeleportEnabled = true;
                                Actor.PositionToSet = AffectedItem2.RoomPosition.GetVector2();
                                Actor.UpdateNeeded = true;

                                Actor.UnblockWalking();

                                System.Threading.Thread.Sleep(1000);

                                Actor.TeleportEnabled = false;

                                Actor.ApplyEffect(OldEffect);

                                break;
                            #endregion

                            #region toggle_furni_state
                            case WiredActionTypes.toggle_state:
                                string[] Selected3 = ActionItem.WiredData.Data1.Split('|');

                                System.Threading.Thread.Sleep(time);
                                foreach (string ItemIdS2 in Selected3)
                                {
                                    uint.TryParse(ItemIdS2, out uint ItemId3);
                                    
                                    Item AffectedItem3 = mInstance.GetItem(ItemId3);
                                    if (AffectedItem3 == null)
                                    {
                                        continue;
                                    }

                                    int.TryParse(AffectedItem3.Flags, out int CurrentState);

                                    int NewState = CurrentState + 1;

                                    if (CurrentState < 0 || CurrentState >= (AffectedItem3.Definition.BehaviorData - 1))
                                    {
                                        NewState = 0;
                                    }

                                    if (AffectedItem3.Definition.Behavior == ItemBehavior.Fireworks)
                                    {
                                        int.TryParse(AffectedItem3.Flags, out int CurrentCharges);
                                        
                                        if (AffectedItem3.DisplayFlags == "2")
                                        {

                                        }
                                        else if (CurrentCharges > 0)
                                        {
                                            AffectedItem3.DisplayFlags = "2";
                                            AffectedItem3.BroadcastStateUpdate(mInstance);

                                            AffectedItem3.Flags = (--CurrentCharges).ToString();
                                            RoomManager.MarkWriteback(AffectedItem3, true);

                                            AffectedItem3.RequestUpdate(AffectedItem3.Definition.BehaviorData);
                                        }
                                    }
                                    else if (AffectedItem3.Definition.Behavior == ItemBehavior.HoloDice || AffectedItem3.Definition.Behavior == ItemBehavior.Dice)
                                    {
                                        AffectedItem3.Flags = "-1";
                                        AffectedItem3.DisplayFlags = "-1";

                                        AffectedItem3.BroadcastStateUpdate(mInstance);
                                        AffectedItem3.RequestUpdate(3);
                                    }
                                    else if (AffectedItem3.Definition.Behavior == ItemBehavior.TraxPlayer)
                                    {
                                        if (mInstance.MusicController.IsPlaying)
                                        {
                                            mInstance.MusicController.Stop();
                                            mInstance.MusicController.BroadcastCurrentSongData(mInstance);
                                            AffectedItem3.DisplayFlags = "0";
                                            AffectedItem3.BroadcastStateUpdate(mInstance);
                                        }
                                        else
                                        {
                                            if (mInstance.MusicController.PlaylistSize > 0)
                                            {
                                                mInstance.MusicController.Start();
                                                AffectedItem3.DisplayFlags = "1";
                                                AffectedItem3.BroadcastStateUpdate(mInstance);
                                            }
                                            else
                                            {
                                                AffectedItem3.DisplayFlags = "0";
                                                AffectedItem3.BroadcastStateUpdate(mInstance);
                                            }
                                        }
                                    }
                                    else if(AffectedItem3.Definition.Behavior == ItemBehavior.GameCounter)
                                    {
                                        AffectedItem3.TimmerRunning = !AffectedItem3.TimmerRunning;
                                        AffectedItem3.RequestUpdate(0);
                                        AffectedItem3.BroadcastStateUpdate(mInstance);
                                    }
                                    else if (CurrentState != NewState)
                                    {
                                        AffectedItem3.Flags = NewState.ToString();
                                        AffectedItem3.DisplayFlags = AffectedItem3.Flags;

                                        RoomManager.MarkWriteback(AffectedItem3, true);

                                        AffectedItem3.BroadcastStateUpdate(mInstance);
                                    }
                                    Instance.WiredManager.HandleToggleState(Actor, AffectedItem3);
                                }

                                break;
                                #endregion
                        }
                    }
                }
            }

            catch (StackOverflowException e)
            {
                string text = System.IO.File.ReadAllText(Environment.CurrentDirectory + "\\error-log.txt");
                Output.WriteLine("Error in Wired Action: " + e.Message);
                System.IO.StreamWriter file = new System.IO.StreamWriter(Environment.CurrentDirectory + "\\error-log.txt");
                file.WriteLine(text + "Error in Wired Action: " + e.Message + "\n\n" + e.StackTrace,
                    OutputLevel.Notification + "\n\n");
            }
        }

        private List<Item> ItemsOnPosition(Vector3 Position)
        {
            return mInstance.GetItemsOnPosition(Position.GetVector2()).ToList();
        }
    }
}
