using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Storage;
using Snowlight.Specialized;

using Snowlight.Game.Misc;
using Snowlight.Game.Bots;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Game.Characters;
using Snowlight.Game.Pathfinding;

using Snowlight.Communication.Outgoing;
using Snowlight.Game.Rooms.Games;
using Snowlight.Util;


namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class RollerRinkHandler
    {
        private static List<int> mAvatarEffects = new List<int>() { 55, 56 };

        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.RollerRink, new ItemEventHandler(HandleRollerRink));
        }

        private static bool HandleRollerRink(RoomActor Actor, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            switch (Event)
            {
                case ItemEventType.WalkOnItem:

                    if (!Instance.GameManager.PlayingUsers.ContainsKey(Actor.ReferenceId))
                    {
                        Actor.PlayingGameType = GameType.RollerSkating;

                        Instance.GameManager.PlayingUsers.Add(Actor.ReferenceId, Actor);
                    }

                    int ApplyEffect = 0;

                    switch (Actor.Type)
                    {
                        case RoomActorType.UserCharacter:

                            ApplyEffect = Actor.Gender == CharacterGender.Male ? mAvatarEffects[0] : mAvatarEffects[1];

                            if (!Instance.GameManager.RollerStrollerAchievement.ContainsKey(Actor.ReferenceId))
                            {
                                Instance.GameManager.RollerStrollerAchievement.Add(Actor.ReferenceId, DateTime.Now);
                            }

                            break;

                        case RoomActorType.AiBot:

                            Bot BotObject = (Bot)Actor.ReferenceObject;

                            if (BotObject != null)
                            {
                                ApplyEffect = mAvatarEffects[0];
                            }

                            break;
                    }

                    Actor.ApplyEffect(ApplyEffect, true);

                    break;

                case ItemEventType.WalkOffItem:

                    if (Actor.PositionToSet != null)
                    {
                        Item NextItem = Instance.GetItemsOnPosition(Actor.PositionToSet).Where(I => I.Definition.Behavior == ItemBehavior.RollerRink).FirstOrDefault();

                        if (NextItem != null)
                        {
                            break;
                        }
                    }

                    Actor.PlayingGameType = GameType.None;
                    Instance.GameManager.PlayingUsers.Remove(Actor.ReferenceId);

                    int ClearEffect = 0;

                    switch (Actor.Type)
                    {
                        case RoomActorType.UserCharacter:

                            Session TargetSession = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                            if (TargetSession != null)
                            {
                                ClearEffect = TargetSession.CurrentEffect;
                            }

                            break;

                        case RoomActorType.AiBot:

                            Bot BotObject = (Bot)Actor.ReferenceObject;

                            if (BotObject != null)
                            {
                                ClearEffect = BotObject.Effect;
                            }

                            break;
                    }

                    Actor.ApplyEffect(ClearEffect, true);

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

                                    mActor.ApplyEffect(0, true);

                                    Instance.GameManager.PlayingUsers.Remove(mActor.ReferenceId);
                                }
                            }
                        }
                    }

                    break;
            }

            return true;
        }
    }
}
