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


namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class IceSkatingHandler
    {
        private static List<int> mAvatarEffects = new List<int>() { 38, 39, 45, 46 };
        private static Dictionary<uint, List<Item>> mRoomTagPoles =  new Dictionary<uint, List<Item>>();

        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.TagPole, new ItemEventHandler(HandleTagPole));
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.IceSkating, new ItemEventHandler(HandleIceSkating));
        }

        private static bool HandleTagPole(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.Removing:

                    if (mRoomTagPoles.ContainsKey(Instance.RoomId) &&
                        mRoomTagPoles[Instance.RoomId].Contains(Item))
                    {
                        mRoomTagPoles[Instance.RoomId].Remove(Item);
                    }

                    List<Item> RemaingTagPoles = Instance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.TagPole).ToList();

                    if (RemaingTagPoles.Count == 0)
                    {
                        if (mRoomTagPoles.ContainsKey(Instance.RoomId))
                        {
                            mRoomTagPoles.Remove(Instance.RoomId);
                        }
                    }

                    break;
            }

            return true;
        }

        private static bool HandleIceSkating(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            int TagPoles = mRoomTagPoles.ContainsKey(Instance.RoomId) ? mRoomTagPoles[Instance.RoomId].Count() : 0;
            int TaggedUsers = Instance.Actors.Where(A => A.PlayingGameType == GameType.IceSkating && A.PlayerTagStatus == TagStatus.Tagged).Count();

            bool GiveForFirstUsers = TagPoles > 0 && TaggedUsers < TagPoles;

            switch (Event)
            {
                case ItemEventType.UpdateTick:

                    List<Item> IceSkatingPoles = Instance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.TagPole).ToList();

                    foreach (Item mItems in IceSkatingPoles)
                    {
                        if (!mRoomTagPoles.ContainsKey(Instance.RoomId))
                        {
                            mRoomTagPoles.Add(Instance.RoomId, new List<Item>());
                        }

                        if (!mRoomTagPoles[Instance.RoomId].Contains(mItems))
                        {
                            mRoomTagPoles[Instance.RoomId].Add(mItems);
                        }
                    }

                    if (mRoomTagPoles.ContainsKey(Instance.RoomId))
                    {
                        foreach (Item RoomTagPole in mRoomTagPoles[Instance.RoomId])
                        {
                            if (Instance.GameManager.IceTaggedUserList.ContainsKey(RoomTagPole.Id) &&
                                !Instance.Actors.Contains(Instance.GameManager.IceTaggedUserList[RoomTagPole.Id]))
                            {
                                RoomActor NextTagActor = Instance.Actors.OrderBy(r => RandomGenerator.GetNext(int.MinValue, int.MaxValue - 1)).Where(A => A.PlayingGameType == GameType.IceSkating && A.PlayerTagStatus == TagStatus.Playing).ToList().FirstOrDefault();

                                if (NextTagActor != null && NextTagActor.PlayingGameType == GameType.IceSkating)
                                {
                                    Instance.GameManager.IceTaggedUserList[RoomTagPole.Id] = NextTagActor;
                                    NextTagActor.PlayerTagStatus = TagStatus.Tagged;

                                    int GiveTagEffect = NextTagActor.Gender == CharacterGender.Male ? mAvatarEffects[2]
                                        : mAvatarEffects[3];

                                    NextTagActor.ApplyEffect(GiveTagEffect, true);
                                }
                                else
                                {
                                    Instance.GameManager.IceTaggedUserList.Remove(RoomTagPole.Id);
                                }

                                break;
                            }
                        }
                    }

                    if (TaggedUsers > TagPoles)
                    {
                        foreach (KeyValuePair<uint, RoomActor> KeyValues in Instance.GameManager.IceTaggedUserList)
                        {
                            Item ItemProbe = null;

                            if (mRoomTagPoles.ContainsKey(Instance.RoomId))
                            {
                                ItemProbe = mRoomTagPoles[Instance.RoomId].Where(I => I.Id == KeyValues.Key).FirstOrDefault();
                            }

                            if (ItemProbe == null)
                            {
                                KeyValues.Value.PlayerTagStatus = TagStatus.Playing;

                                Session RemoveTagSession = SessionManager.GetSessionByCharacterId(KeyValues.Value.ReferenceId);

                                int RemoveTagEffect = RemoveTagSession.CharacterInfo.Gender == CharacterGender.Male ? mAvatarEffects[0]
                                    : mAvatarEffects[1];

                                KeyValues.Value.ApplyEffect(RemoveTagEffect, true);

                                Instance.GameManager.IceTaggedUserList.Remove(KeyValues.Key);
                                break;
                            }
                        }
                    }
                    else if (TagPoles > TaggedUsers)
                    {
                        foreach (Item RoomTagPole in mRoomTagPoles[Instance.RoomId])
                        {
                            if (!Instance.GameManager.IceTaggedUserList.ContainsKey(RoomTagPole.Id))
                            {
                                RoomActor NextTagActor = Instance.Actors.OrderBy(r => RandomGenerator.GetNext(int.MinValue, int.MaxValue - 1)).Where(A => A.PlayingGameType == GameType.IceSkating && A.PlayerTagStatus == TagStatus.Playing).ToList().FirstOrDefault();

                                if (NextTagActor != null)
                                {
                                    Instance.GameManager.IceTaggedUserList[RoomTagPole.Id] = NextTagActor;
                                    NextTagActor.PlayerTagStatus = TagStatus.Tagged;

                                    int GiveTagEffect = NextTagActor.Gender == CharacterGender.Male ? mAvatarEffects[2]
                                        : mAvatarEffects[3];

                                    NextTagActor.ApplyEffect(GiveTagEffect, true);
                                }
                                else
                                {
                                    Instance.GameManager.IceTaggedUserList.Remove(RoomTagPole.Id);
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
                        Actor.PlayingGameType = GameType.IceSkating;
                        Actor.PlayerTagStatus = TagStatus.Playing;

                        Instance.GameManager.PlayingUsers.Add(Actor.ReferenceId, Actor);

                        if (!Instance.GameManager.IceIceBabyAchievement.ContainsKey(Actor.ReferenceId))
                        {
                            Instance.GameManager.IceIceBabyAchievement.Add(Actor.ReferenceId, DateTime.Now);
                        }
                    }

                    if (GiveForFirstUsers)
                    {
                        foreach (Item TagPoleItem in mRoomTagPoles[Instance.RoomId])
                        {
                            if (!Instance.GameManager.IceTaggedUserList.ContainsKey(TagPoleItem.Id))
                            {
                                Instance.GameManager.IceTaggedUserList.Add(TagPoleItem.Id, Actor);
                            }
                        }

                        Actor.PlayerTagStatus = TagStatus.Tagged;
                    }

                    bool IsPlaying = Actor.PlayingGameType == GameType.IceSkating &&
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
                                    bool IsTargetPlaying = TargetActor.PlayingGameType == GameType.IceSkating &&
                                        TargetActor.PlayerTagStatus > TagStatus.None;

                                    if (IsTargetPlaying)
                                    {
                                        foreach (Item TradeTagItem in mRoomTagPoles[Instance.RoomId])
                                        {
                                            if (Instance.GameManager.IceTaggedUserList.ContainsKey(TradeTagItem.Id) &&
                                                Instance.GameManager.IceTaggedUserList[TradeTagItem.Id] == Actor)
                                            {
                                                Instance.GameManager.IceTaggedUserList[TradeTagItem.Id] = TargetActor;
                                                break;
                                            }
                                        }

                                        Actor.PlayerTagStatus = TagStatus.Playing;
                                        TargetActor.PlayerTagStatus = TagStatus.Tagged;

                                        int TargetApplyEffect = 0;
                                        int TargetEffectId = 0;

                                        if (TargetActor.PlayerTagStatus == TagStatus.Tagged)
                                        {
                                            TargetEffectId += 2;
                                        }

                                        TargetApplyEffect = TargetActor.Gender == CharacterGender.Male ? mAvatarEffects[0 + TargetEffectId]
                                            : mAvatarEffects[1 + TargetEffectId];

                                        TargetActor.ApplyEffect(TargetApplyEffect, true);

                                        Session TargetSessionAchievement = SessionManager.GetSessionByCharacterId(TargetActor.ReferenceId);
                                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                                        {
                                            AchievementManager.ProgressUserAchievement(MySqlClient, TargetSessionAchievement, "ACH_TagB", 1);
                                        }
                                    }
                                }

                                break;
                        }
                    }

                    int ApplyEffect = 0;
                    int EffectId = 0;

                    if (Actor.Type == RoomActorType.UserCharacter)
                    {
                        if (Actor.PlayerTagStatus == TagStatus.Tagged)
                        {
                            EffectId += 2;
                        }

                        ApplyEffect = Actor.Gender == CharacterGender.Male ? mAvatarEffects[0 + EffectId]
                            : mAvatarEffects[1 + EffectId];
                    }

                    Actor.ApplyEffect(ApplyEffect, true);

                    break;

                case ItemEventType.WalkOffItem:

                    if (Actor.PositionToSet != null)
                    {
                        Item NextItem = Instance.GetItemsOnPosition(Actor.PositionToSet).Where(I => I.Definition.Behavior == ItemBehavior.IceSkating).FirstOrDefault();

                        if (NextItem != null)
                        {
                            break;
                        }
                    }

                    Actor.PlayingGameType = GameType.None;
                    Actor.PlayerTagStatus = TagStatus.None;

                    Instance.GameManager.PlayingUsers.Remove(Actor.ReferenceId);

                    if (mRoomTagPoles.ContainsKey(Instance.RoomId))
                    {
                        foreach (Item RoomTagPole in mRoomTagPoles[Instance.RoomId])
                        {
                            if (Instance.GameManager.IceTaggedUserList.ContainsKey(RoomTagPole.Id) &&
                                Instance.GameManager.IceTaggedUserList[RoomTagPole.Id] == Actor)
                            {
                                RoomActor NextTagActor = Instance.Actors.Where(A => A.PlayingGameType == GameType.IceSkating && A.PlayerTagStatus == TagStatus.Playing).OrderBy(r => RandomGenerator.GetNext(int.MinValue, int.MaxValue - 1)).ToList().FirstOrDefault();

                                if (NextTagActor != null)
                                {
                                    Instance.GameManager.IceTaggedUserList[RoomTagPole.Id] = NextTagActor;
                                    NextTagActor.PlayerTagStatus = TagStatus.Tagged;

                                    int TargetApplyEffect = 0;
                                    int TargetEffectId = 0;

                                    if (NextTagActor.PlayerTagStatus == TagStatus.Tagged)
                                    {
                                        TargetEffectId += 2;
                                    }

                                    TargetApplyEffect = NextTagActor.Gender == CharacterGender.Male ? mAvatarEffects[0 + TargetEffectId]
                                        : mAvatarEffects[1 + TargetEffectId];

                                    NextTagActor.ApplyEffect(TargetApplyEffect, true);
                                }
                                else
                                {
                                    Instance.GameManager.IceTaggedUserList.Remove(RoomTagPole.Id);
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

                    if (!mRoomTagPoles.ContainsKey(Instance.RoomId))
                    {
                        mRoomTagPoles.Add(Instance.RoomId, new List<Item>());
                    }

                    mRoomTagPoles[Instance.RoomId].Clear();

                    Item.RequestUpdate(1);
                    break;

                case ItemEventType.Moved:
                case ItemEventType.Removing:

                    List<Vector2> AffectedTiles = Instance.CalculateAffectedTiles(Item, Item.RoomPosition.GetVector2(), Item.RoomRotation);

                    foreach(Vector2 Tile in AffectedTiles)
                    {
                        List<RoomActor> ActorsOnTile = Instance.GetActorsOnPosition(Tile);

                        if (ActorsOnTile.Count > 0)
                        {
                            foreach (RoomActor mActor in ActorsOnTile)
                            {
                                if(Instance.GameManager.PlayingUsers.ContainsKey(mActor.ReferenceId))
                                {
                                    mActor.PlayingGameType = GameType.None;
                                    mActor.PlayerTagStatus = TagStatus.None;

                                    mActor.ApplyEffect(0, true);

                                    Instance.GameManager.PlayingUsers.Remove(mActor.ReferenceId);
                                }

                                if (mRoomTagPoles.ContainsKey(Instance.RoomId))
                                {
                                    foreach (Item RoomTagPole in mRoomTagPoles[Instance.RoomId])
                                    {
                                        if (Instance.GameManager.IceTaggedUserList.ContainsKey(RoomTagPole.Id) &&
                                            Instance.GameManager.IceTaggedUserList[RoomTagPole.Id] == mActor)
                                        {
                                            RoomActor NextTagActor = Instance.Actors.Where(A => A.PlayingGameType == GameType.IceSkating && A.PlayerTagStatus == TagStatus.Playing).OrderBy(r => RandomGenerator.GetNext(int.MinValue, int.MaxValue - 1)).ToList().FirstOrDefault();

                                            if (NextTagActor != null)
                                            {
                                                Instance.GameManager.IceTaggedUserList[RoomTagPole.Id] = NextTagActor;
                                                NextTagActor.PlayerTagStatus = TagStatus.Tagged;

                                                int TargetApplyEffect = 0;
                                                int TargetEffectId = 0;

                                                if (NextTagActor.PlayerTagStatus == TagStatus.Tagged)
                                                {
                                                    TargetEffectId += 2;
                                                }

                                                TargetApplyEffect = NextTagActor.Gender == CharacterGender.Male ? mAvatarEffects[0 + TargetEffectId]
                                                    : mAvatarEffects[1 + TargetEffectId];

                                                NextTagActor.ApplyEffect(TargetApplyEffect, true);
                                            }
                                            else
                                            {
                                                Instance.GameManager.IceTaggedUserList.Remove(RoomTagPole.Id);
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (mRoomTagPoles.ContainsKey(Instance.RoomId))
                    {
                        mRoomTagPoles[Instance.RoomId].Clear();
                    }

                    List<Item> RemaingTagPoles = Instance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.TagPole).ToList();

                    if(RemaingTagPoles.Count == 0)
                    {
                        if (mRoomTagPoles.ContainsKey(Instance.RoomId))
                        {
                            mRoomTagPoles.Remove(Instance.RoomId);
                        }
                    }

                    Item.RequestUpdate(1);
                    break;
            }

            return true;
        }
    }
}
