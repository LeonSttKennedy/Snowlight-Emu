using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

using Snowlight.Storage;
using Snowlight.Specialized;

using Snowlight.Game.Bots;
using Snowlight.Game.Misc;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Game.Characters;
using Snowlight.Game.Pathfinding;
using Snowlight.Game.Achievements;

using Snowlight.Communication.Outgoing;
using Snowlight.Game.Rooms.Games;
using Snowlight.Util;
using System.Diagnostics.Eventing.Reader;


namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class BunnyRunHandler
    {
        private static List<int> mAvatarEffects = new List<int>() { 68 };
        private static Dictionary<uint, List<Item>> mRoomEggTagPoles = new Dictionary<uint, List<Item>>();

        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.EggTagPole, new ItemEventHandler(HandleEggTagPole));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.BunnyRun, new ItemEventHandler(HandleBunnyRunField));
        }

        private static bool HandleEggTagPole(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Removing:

                    if (mRoomEggTagPoles.ContainsKey(Instance.RoomId) &&
                        mRoomEggTagPoles[Instance.RoomId].Contains(Item))
                    {
                        mRoomEggTagPoles[Instance.RoomId].Remove(Item);
                    }

                    List<Item> RemaingTagPoles = Instance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.EggTagPole).ToList();

                    if (RemaingTagPoles.Count == 0)
                    {
                        if (mRoomEggTagPoles.ContainsKey(Instance.RoomId))
                        {
                            mRoomEggTagPoles.Remove(Instance.RoomId);
                        }
                    }

                    break;
            }

            return true;
        }

        private static bool HandleBunnyRunField(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            int EggTagPoles = mRoomEggTagPoles.ContainsKey(Instance.RoomId) ? mRoomEggTagPoles[Instance.RoomId].Count() : 0;
            int TaggedUsers = Instance.Actors.Where(A => A.PlayingGameType == GameType.BunnyRun && A.PlayerTagStatus == TagStatus.Tagged).Count();

            bool GiveForFirstUsers = EggTagPoles > 0 && TaggedUsers < EggTagPoles;

            switch (Event)
            {
                case ItemEventType.UpdateTick:

                    List<Item> BunnyRunPoles = Instance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.EggTagPole).ToList();

                    foreach (Item mItem in BunnyRunPoles)
                    {
                        if (!mRoomEggTagPoles.ContainsKey(Instance.RoomId))
                        {
                            mRoomEggTagPoles.Add(Instance.RoomId, new List<Item>());
                        }

                        if (!mRoomEggTagPoles[Instance.RoomId].Contains(mItem))
                        {
                            mRoomEggTagPoles[Instance.RoomId].Add(mItem);
                        }
                    }

                    if (mRoomEggTagPoles.ContainsKey(Instance.RoomId))
                    {
                        foreach (Item RoomEggTagPole in mRoomEggTagPoles[Instance.RoomId])
                        {
                            if (Instance.GameManager.BunnyRunTaggedUserList.ContainsKey(RoomEggTagPole.Id) &&
                                !Instance.Actors.Contains(Instance.GameManager.BunnyRunTaggedUserList[RoomEggTagPole.Id]))
                            {
                                RoomActor NextTagActor = Instance.Actors.Where(A => A.PlayingGameType == GameType.BunnyRun && A.PlayerTagStatus == TagStatus.Playing).OrderBy(r => RandomGenerator.GetNext(int.MinValue, int.MaxValue - 1)).ToList().FirstOrDefault();

                                if (NextTagActor != null)
                                {
                                    Instance.GameManager.BunnyRunTaggedUserList[RoomEggTagPole.Id] = NextTagActor;
                                    NextTagActor.PlayerTagStatus = TagStatus.Tagged;

                                    NextTagActor.ApplyEffect(mAvatarEffects[0], true);
                                }
                                else
                                {
                                    Instance.GameManager.BunnyRunTaggedUserList.Remove(RoomEggTagPole.Id);
                                }

                                break;
                            }
                        }
                    }

                    if (TaggedUsers > EggTagPoles)
                    {
                        foreach (KeyValuePair<uint, RoomActor> KeyValues in Instance.GameManager.BunnyRunTaggedUserList)
                        {
                            Item ItemProbe = null;

                            if (mRoomEggTagPoles.ContainsKey(Instance.RoomId))
                            {
                                ItemProbe = mRoomEggTagPoles[Instance.RoomId].Where(I => I.Id == KeyValues.Key).FirstOrDefault();
                            }

                            if (ItemProbe == null)
                            {
                                KeyValues.Value.PlayerTagStatus = TagStatus.Playing;
                                KeyValues.Value.ApplyEffect(0, true);

                                Instance.GameManager.BunnyRunTaggedUserList.Remove(KeyValues.Key);
                                break;
                            }
                        }
                    }
                    else if (EggTagPoles > TaggedUsers)
                    {
                        foreach (Item RoomEggTagPole in mRoomEggTagPoles[Instance.RoomId])
                        {
                            if (!Instance.GameManager.BunnyRunTaggedUserList.ContainsKey(RoomEggTagPole.Id))
                            {
                                RoomActor NextTagActor = Instance.Actors.Where(A => A.PlayingGameType == GameType.BunnyRun && A.PlayerTagStatus == TagStatus.Playing).OrderBy(r => RandomGenerator.GetNext(int.MinValue, int.MaxValue - 1)).ToList().FirstOrDefault();

                                if (NextTagActor != null)
                                {
                                    Instance.GameManager.BunnyRunTaggedUserList[RoomEggTagPole.Id] = NextTagActor;
                                    NextTagActor.PlayerTagStatus = TagStatus.Tagged;

                                    NextTagActor.ApplyEffect(mAvatarEffects[0], true);
                                }
                                else
                                {
                                    Instance.GameManager.BunnyRunTaggedUserList.Remove(RoomEggTagPole.Id);
                                }

                                break;
                            }
                        }
                    }

                    Item.RequestUpdate(1);
                    break;

                case ItemEventType.WalkOnItem:

                    if (!Instance.GameManager.PlayingUsers.ContainsKey(Actor.ReferenceId))
                    {
                        Actor.PlayingGameType = GameType.BunnyRun;
                        Actor.PlayerTagStatus = TagStatus.Playing;

                        Instance.GameManager.PlayingUsers.Add(Actor.ReferenceId, Actor);
                    }

                    if (GiveForFirstUsers)
                    {
                        foreach (Item EggTagPoleItem in mRoomEggTagPoles[Instance.RoomId])
                        {
                            if (!Instance.GameManager.BunnyRunTaggedUserList.ContainsKey(EggTagPoleItem.Id))
                            {
                                Instance.GameManager.BunnyRunTaggedUserList.Add(EggTagPoleItem.Id, Actor);
                            }
                        }

                        Actor.PlayerTagStatus = TagStatus.Tagged;
                    }

                    bool IsPlaying = Actor.PlayingGameType == GameType.BunnyRun &&
                                Actor.PlayerTagStatus > TagStatus.None;

                    if (IsPlaying)
                    {
                        switch (Actor.PlayerTagStatus)
                        {
                            case TagStatus.Tagged:

                                Vector2 Position = new Vector2(Actor.Position.X + 1, Actor.Position.Y);

                                RoomActor TargetActor = Instance.IsValidPosition(Position) ?
                                    Instance.GetActorsOnPosition(Position).ToList().FirstOrDefault() : null;

                                if (TargetActor == null)
                                {
                                    Position = new Vector2(Actor.Position.X - 1, Actor.Position.Y);

                                    TargetActor = Instance.IsValidPosition(Position) ?
                                        Instance.GetActorsOnPosition(Position).ToList().FirstOrDefault() : null;

                                    if (TargetActor == null)
                                    {
                                        Position = new Vector2(Actor.Position.X, Actor.Position.Y + 1);

                                        TargetActor = Instance.IsValidPosition(Position) ?
                                            Instance.GetActorsOnPosition(Position).ToList().FirstOrDefault() : null;

                                        if (TargetActor == null)
                                        {
                                            Position = new Vector2(Actor.Position.X, Actor.Position.Y - 1);

                                            TargetActor = Instance.IsValidPosition(Position) ?
                                                Instance.GetActorsOnPosition(Position).ToList().FirstOrDefault() : null;
                                        }
                                    }
                                }


                                if (TargetActor != null)
                                {
                                    bool IsTargetPlaying = TargetActor.PlayingGameType == GameType.BunnyRun &&
                                        TargetActor.PlayerTagStatus > TagStatus.None;

                                    if (IsTargetPlaying)
                                    {
                                        foreach (Item TradeTagItem in mRoomEggTagPoles[Instance.RoomId])
                                        {
                                            if (Instance.GameManager.BunnyRunTaggedUserList.ContainsKey(TradeTagItem.Id) &&
                                                Instance.GameManager.BunnyRunTaggedUserList[TradeTagItem.Id] == Actor)
                                            {
                                                Instance.GameManager.BunnyRunTaggedUserList[TradeTagItem.Id] = TargetActor;
                                                break;
                                            }
                                        }

                                        Actor.PlayerTagStatus = TagStatus.Playing;
                                        TargetActor.PlayerTagStatus = TagStatus.Tagged;

                                        TargetActor.ApplyEffect(mAvatarEffects[0], true);
                                    }
                                }

                                break;
                        }
                    }

                    int ApplyEffect = 0;

                    if (Actor.Type == RoomActorType.UserCharacter)
                    {
                        if (Actor.PlayerTagStatus == TagStatus.Tagged)
                        {
                            ApplyEffect = mAvatarEffects[0];
                        }

                        Actor.ApplyEffect(ApplyEffect, true);
                    }
                    
                    break;

                case ItemEventType.WalkOffItem:

                    if (Actor.PositionToSet != null)
                    {
                        Item NextItem = Instance.GetItemsOnPosition(Actor.PositionToSet).Where(I => I.Definition.Behavior == ItemBehavior.BunnyRun).FirstOrDefault();

                        if (NextItem != null)
                        {
                            break;
                        }
                    }

                    Actor.PlayingGameType = GameType.None;
                    Actor.PlayerTagStatus = TagStatus.None;

                    Instance.GameManager.PlayingUsers.Remove(Actor.ReferenceId);

                    if (mRoomEggTagPoles.ContainsKey(Instance.RoomId))
                    {
                        foreach (Item RoomEggTagPole in mRoomEggTagPoles[Instance.RoomId])
                        {
                            if (Instance.GameManager.BunnyRunTaggedUserList.ContainsKey(RoomEggTagPole.Id) &&
                                Instance.GameManager.BunnyRunTaggedUserList[RoomEggTagPole.Id] == Actor)
                            {
                                RoomActor NextTagActor = Instance.Actors.Where(A => A.PlayingGameType == GameType.BunnyRun && A.PlayerTagStatus == TagStatus.Playing).OrderBy(r => RandomGenerator.GetNext(int.MinValue, int.MaxValue - 1)).FirstOrDefault();

                                if (NextTagActor != null)
                                {
                                    Instance.GameManager.BunnyRunTaggedUserList[RoomEggTagPole.Id] = NextTagActor;
                                    NextTagActor.PlayerTagStatus = TagStatus.Tagged;

                                    NextTagActor.ApplyEffect(mAvatarEffects[0], true);
                                }
                                else
                                {
                                    Instance.GameManager.BunnyRunTaggedUserList.Remove(RoomEggTagPole.Id);
                                }

                                break;
                            }
                        }
                    }
                    
                    int ClearEffect = 0;

                    if (Actor.Type == RoomActorType.UserCharacter)
                    {
                        Session TargetSession = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                        if (TargetSession != null)
                        {
                            ClearEffect = TargetSession.CurrentEffect;
                        }
                    }
                    
                    Actor.ApplyEffect(ClearEffect, true);
                    break;

                case ItemEventType.Placed:
                case ItemEventType.InstanceLoaded:

                    if (!mRoomEggTagPoles.ContainsKey(Instance.RoomId))
                    {
                        mRoomEggTagPoles.Add(Instance.RoomId, new List<Item>());
                    }

                    mRoomEggTagPoles[Instance.RoomId].Clear();

                    Item.RequestUpdate(1);
                    break;

                case ItemEventType.Moved:
                case ItemEventType.Removing:

                    List<Vector2> AffectedTiles = Instance.CalculateAffectedTiles(Item, Item.RoomPosition.GetVector2(), Item.RoomRotation);

                    foreach (Vector2 Tile in AffectedTiles)
                    { 
                        List<RoomActor> ActorsOnTile = Instance.GetActorsOnPosition(Tile);

                        if (ActorsOnTile.Count > 0)
                        {
                            foreach (RoomActor mActor in ActorsOnTile)
                            {
                                if (Instance.GameManager.PlayingUsers.ContainsKey(mActor.ReferenceId))
                                {
                                    mActor.PlayingGameType = GameType.None;
                                    mActor.PlayerTagStatus = TagStatus.None;

                                    mActor.ApplyEffect(0, true);

                                    Instance.GameManager.PlayingUsers.Remove(mActor.ReferenceId);
                                }

                                if (mRoomEggTagPoles.ContainsKey(Instance.RoomId))
                                {
                                    foreach (Item RoomEggTagPole in mRoomEggTagPoles[Instance.RoomId])
                                    {
                                        if (Instance.GameManager.BunnyRunTaggedUserList.ContainsKey(RoomEggTagPole.Id) &&
                                            Instance.GameManager.BunnyRunTaggedUserList[RoomEggTagPole.Id] == Actor)
                                        {
                                            RoomActor NextTagActor = Instance.Actors.Where(A => A.PlayingGameType == GameType.BunnyRun && A.PlayerTagStatus == TagStatus.Playing).OrderBy(r => RandomGenerator.GetNext(int.MinValue, int.MaxValue - 1)).FirstOrDefault();

                                            if (NextTagActor != null)
                                            {
                                                Instance.GameManager.BunnyRunTaggedUserList[RoomEggTagPole.Id] = NextTagActor;
                                                NextTagActor.PlayerTagStatus = TagStatus.Tagged;

                                                NextTagActor.ApplyEffect(mAvatarEffects[0], true);
                                            }
                                            else
                                            {
                                                Instance.GameManager.BunnyRunTaggedUserList.Remove(RoomEggTagPole.Id);
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (mRoomEggTagPoles.ContainsKey(Instance.RoomId))
                    {
                        mRoomEggTagPoles[Instance.RoomId].Clear();
                    }

                    List<Item> RemaingTagPoles = Instance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.EggTagPole).ToList();

                    if (RemaingTagPoles.Count == 0)
                    {
                        if (mRoomEggTagPoles.ContainsKey(Instance.RoomId))
                        {
                            mRoomEggTagPoles.Remove(Instance.RoomId);
                        }
                    }

                    Item.RequestUpdate(1);
                    break;
            }

            return true;
        }
    }
}
