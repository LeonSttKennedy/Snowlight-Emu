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


namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class SkateRailHandler
    {
        private static List<int> mAvatarEffects = new List<int>() { 71, 72 };
        private static Dictionary<uint, List<Vector2>> mAffectedTiles = new Dictionary<uint, List<Vector2>>();
        private static List<string> mAchievementsToUnlock = new List<string>() { "ACH_SkateBoardJump", "ACH_SkateBoardSlide" };

        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.SkateRail, new ItemEventHandler(HandleSkateRail));
        }

        private static bool HandleSkateRail(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.UpdateTick:

                    List<Item> SkateRails = Instance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.SkateRail).ToList();

                    foreach (Item SkateRail in SkateRails)
                    {
                        List<Vector2> AffectedTiles = Instance.CalculateAffectedTiles(Item, Item.RoomPosition.GetVector2(), Item.RoomRotation);

                        foreach (Vector2 Tile in AffectedTiles)
                        {
                            if (!mAffectedTiles[Instance.Info.Id].Contains(Tile))
                            {
                                mAffectedTiles[Instance.Info.Id].Add(Tile);
                            }
                        }
                    }

                    foreach (RoomActor _Actor in Instance.Actors)
                    {
                        if (_Actor.Type == RoomActorType.UserCharacter)
                        {
                            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                            {
                                Session TargetSession = SessionManager.GetSessionByCharacterId(_Actor.ReferenceId);

                                uint UserId = TargetSession.CharacterId;

                                if (mAvatarEffects.Contains(TargetSession.CurrentEffect))
                                {
                                    if (mAffectedTiles[Instance.Info.Id].Contains(_Actor.PositionToSet) ||
                                        mAffectedTiles[Instance.Info.Id].Contains(_Actor.Position.GetVector2()))
                                    {
                                        if (!Instance.GameManager.SkateboardUserList.ContainsKey(UserId))
                                        {
                                            AchievementManager.ProgressUserAchievement(MySqlClient, TargetSession, mAchievementsToUnlock[0], 1);
                                            Instance.GameManager.SkateboardUserList.Add(UserId, 0);
                                        }
                                    }
                                    else
                                    {
                                        if (Instance.GameManager.SkateboardUserList.ContainsKey(UserId))
                                        {
                                            AchievementManager.ProgressUserAchievement(MySqlClient, TargetSession, mAchievementsToUnlock[0], 1);
                                            Instance.GameManager.SkateboardUserList.Remove(UserId);
                                        }
                                    }
                                }
                                else
                                {
                                    if (Instance.GameManager.SkateboardUserList.ContainsKey(UserId))
                                    {
                                        Instance.GameManager.SkateboardUserList.Remove(UserId);
                                    }
                                }

                                bool IsUserSkateboarding = Instance.GameManager.SkateboardUserList.ContainsKey(UserId);

                                if (IsUserSkateboarding && Item.Id == _Actor.FurniOnId && _Actor.IsMoving)
                                {
                                    AchievementManager.ProgressUserAchievement(MySqlClient, TargetSession, mAchievementsToUnlock[1], 1);
                                }
                            }
                        }
                    }

                    Item.RequestUpdate(1);
                    break;

                case ItemEventType.Placed:
                case ItemEventType.InstanceLoaded:

                    if (!mAffectedTiles.ContainsKey(Instance.Info.Id))
                    {
                        mAffectedTiles.Add(Instance.Info.Id, new List<Vector2>());
                    }

                    mAffectedTiles[Instance.Info.Id].Clear();

                    Item.RequestUpdate(1);
                    break;

                case ItemEventType.Moved:
                case ItemEventType.Removing:

                    mAffectedTiles[Instance.Info.Id].Clear();

                    List<Item> RemaingSkateRail = Instance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.SkateRail).ToList();
                    
                    if(RemaingSkateRail.Count == 0)
                    {
                        if (mAffectedTiles.ContainsKey(Instance.Info.Id))
                        {
                            mAffectedTiles.Remove(Instance.Info.Id);
                        }
                    }

                    Item.RequestUpdate(1);
                    break;
            }

            return true;
        }
    }
}
