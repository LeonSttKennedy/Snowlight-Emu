using Snowlight.Communication.Outgoing;
using Snowlight.Game.Achievements;
using Snowlight.Game.Bots;
using Snowlight.Game.Characters;
using Snowlight.Game.Misc;
using Snowlight.Game.Pathfinding;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Specialized;
using Snowlight.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class SkateRailHandler
    {
        private static List<int> mAvatarEffects = new List<int>() { 71, 72 };
        private static List<string> mAchievementsToUnlock = new List<string>() { "ACH_SkateBoardJump", "ACH_SkateBoardSlide" };
        
        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.SkateRail, new ItemEventHandler(HandleSkateRail));
        }

        private static bool HandleSkateRail(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                switch (Event)
                {
                    case ItemEventType.WalkOnItem:

                        if (Actor.IsBot)
                        {
                            break;
                        }

                        Session TargetSession = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                        bool IsUserWhitSkateboard = mAvatarEffects.Contains(TargetSession.CurrentEffect);

                        if (IsUserWhitSkateboard)
                        {
                            Item CurrentItem = Instance.GetItemsOnPosition(Actor.Position.GetVector2()).Where(I => I.Definition.Behavior == ItemBehavior.SkateRail).FirstOrDefault();
                            Item NextItem = null;

                            if (Actor.PositionToSet != null)
                            {
                                NextItem = Instance.GetItemsOnPosition(Actor.PositionToSet).Where(I => I.Definition.Behavior == ItemBehavior.SkateRail).FirstOrDefault();
                            }

                            if (NextItem != null)
                            {
                                if (CurrentItem != null)
                                {
                                    AchievementManager.ProgressUserAchievement(MySqlClient, TargetSession, mAchievementsToUnlock[1], 1);
                                }
                                else
                                {
                                    AchievementManager.ProgressUserAchievement(MySqlClient, TargetSession, mAchievementsToUnlock[0], 1);
                                }
                            }

                            switch (NextItem.RoomRotation)
                            {
                                case 0:

                                    switch (Actor.HeadRotation)
                                    {
                                        case 0:

                                            Actor.SkateboardRotation = 6;

                                            break;

                                        case 4:

                                            Actor.SkateboardRotation = 2;

                                            break;
                                    }

                                    break;

                                case 2:

                                    switch (Actor.HeadRotation)
                                    {
                                        case 2:

                                            Actor.SkateboardRotation = 0;

                                            break;

                                        case 6:

                                            Actor.SkateboardRotation = 4;

                                            break;
                                    }

                                    break;
                            }

                            Actor.UpdateNeeded = true;
                        }

                        break;

                    case ItemEventType.WalkOffItem:

                        if (Actor.IsBot)
                        {
                            break;
                        }
                        
                        if (Actor.PositionToSet != null)
                        {
                            Item NextItem = Instance.GetItemsOnPosition(Actor.PositionToSet).Where(I => I.Definition.Behavior == ItemBehavior.SkateRail).FirstOrDefault();

                            if (NextItem != null)
                            {
                                break;
                            }
                        }

                        TargetSession = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                        IsUserWhitSkateboard = mAvatarEffects.Contains(TargetSession.CurrentEffect);

                        if (IsUserWhitSkateboard)
                        {
                            Item CurrentItem = Instance.GetItemsOnPosition(Actor.Position.GetVector2()).Where(I => I.Definition.Behavior == ItemBehavior.SkateRail).FirstOrDefault();
                            Item NextItem = null;

                            if (Actor.PositionToSet != null)
                            {
                                NextItem = Instance.GetItemsOnPosition(Actor.PositionToSet).Where(I => I.Definition.Behavior == ItemBehavior.SkateRail).FirstOrDefault();
                            }

                            if (CurrentItem != null && NextItem == null)
                            {
                                AchievementManager.ProgressUserAchievement(MySqlClient, TargetSession, mAchievementsToUnlock[0], 1);
                            }

                            if (Actor.IsUserSkateboarding)
                            {
                                Actor.SkateboardRotation = -1;
                            }

                            Actor.UpdateNeeded = true;
                        }

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
                                    if (mActor.IsUserSkateboarding)
                                    {
                                        mActor.SkateboardRotation = -1;
                                    }

                                    mActor.UpdateNeeded = true;
                                }
                            }
                        }

                        break;
                }
            }

            return true;
        }
    }
}
