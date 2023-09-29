using System;

using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Game.Misc;
using Snowlight.Communication.Outgoing;
using Snowlight.Storage;
using Snowlight.Util;
using Snowlight.Game.Achievements;

namespace Snowlight.Game.Items.DefaultBehaviorHandlers
{
    public static class FireworksHandler
    {
        private static readonly int CHARGE_COSTS_CREDITS = 0;
        private static readonly int CHARGE_COSTS_ACTIVITY_POINTS = 20;
        private static readonly SeasonalCurrencyList SEASONAL_CURRENCY = SeasonalCurrencyList.Pixels;
        private static readonly int CHARGE_AMOUNT = 10;

        public static void Register()
        {
            ItemEventDispatcher.RegisterEventHandler(ItemBehavior.Fireworks, new ItemEventHandler(HandleFireworks));
        }

        private static bool HandleFireworks(Session Session, Item Item, RoomInstance Instance, ItemEventType Event, int RequestData)
        {
            int.TryParse(Item.Flags, out int CurrentCharges);

            switch (Event)
            {
                case ItemEventType.InstanceLoaded:
                case ItemEventType.Placed:
                case ItemEventType.Moved:
                case ItemEventType.UpdateTick:

                    string DesiredDisplayFlags = "0";

                    if (CurrentCharges > 0)
                    {
                        DesiredDisplayFlags = "1";
                    }

                    if (Item.DisplayFlags != DesiredDisplayFlags)
                    {
                        Item.DisplayFlags = DesiredDisplayFlags;
                        Item.BroadcastStateUpdate(Instance);
                    }

                    break;

                case ItemEventType.Interact:

                    RoomActor Actor = Instance.GetActorByReferenceId(Session.CharacterId);

                    if (Actor == null)
                    {
                        return true;
                    }

                    if (Distance.Calculate(Actor.Position.GetVector2(), Item.RoomPosition.GetVector2()) > 1)
                    {
                        Actor.MoveToItemAndInteract(Item, RequestData, Item.SquareBehind);
                        return true;
                    }

                    switch (RequestData)
                    {
                        // Purchase charges
                        case 2:

                            int UserActivityPoints = Session.CharacterInfo.ActivityPoints.ContainsKey((int)SEASONAL_CURRENCY) ?
                                Session.CharacterInfo.ActivityPoints[(int)SEASONAL_CURRENCY] : 0;

                            if (Session.CharacterInfo.CreditsBalance < CHARGE_COSTS_CREDITS)
                            {
                                return true;
                            }

                            if (UserActivityPoints < CHARGE_COSTS_ACTIVITY_POINTS)
                            {
                                return true;
                            }

                            bool Update = (CurrentCharges <= 0);
                            CurrentCharges += CHARGE_AMOUNT;

                            Item.Flags = CurrentCharges.ToString();

                            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                            {
                                if (CHARGE_COSTS_CREDITS > 0)
                                {
                                    Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, -CHARGE_COSTS_CREDITS);
                                    Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                                }

                                if (CHARGE_COSTS_ACTIVITY_POINTS > 0)
                                {
                                    Session.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, SEASONAL_CURRENCY, -CHARGE_COSTS_ACTIVITY_POINTS);
                                    if (SEASONAL_CURRENCY == SeasonalCurrencyList.Pixels)
                                    {
                                        Session.SendData(UpdatePixelsBalanceComposer.Compose(Session.CharacterInfo.ActivityPoints[0],
                                            -CHARGE_COSTS_ACTIVITY_POINTS));
                                        
                                        AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_FireworksCharger", CHARGE_COSTS_ACTIVITY_POINTS);
                                    }
                                    
                                    Session.SendData(UserActivityPointsBalanceComposer.Compose(Session.CharacterInfo.ActivityPoints));
                                }

                                Item.SynchronizeDatabase(MySqlClient, true);
                            }

                            Session.SendData(FireworksChargeInfoComposer.Compose(Item.Id, CurrentCharges, CHARGE_COSTS_CREDITS,
                                CHARGE_COSTS_ACTIVITY_POINTS, (int)SEASONAL_CURRENCY, CHARGE_AMOUNT));

                            if (Update)
                            {
                                Item.DisplayFlags = "1";
                                Item.BroadcastStateUpdate(Instance);
                            }

                            break;

                        case 1:

                            Session.SendData(FireworksChargeInfoComposer.Compose(Item.Id, CurrentCharges, CHARGE_COSTS_CREDITS,
                                CHARGE_COSTS_ACTIVITY_POINTS, (int)SEASONAL_CURRENCY, CHARGE_AMOUNT));
                            break;

                        default:
                        case 0:

                            if (Item.DisplayFlags == "2")
                            {
                                return true;
                            }

                            if (CurrentCharges > 0)
                            {
                                Item.DisplayFlags = "2";
                                Item.BroadcastStateUpdate(Instance);

                                Item.Flags = (--CurrentCharges).ToString();
                                RoomManager.MarkWriteback(Item, true);

                                Item.RequestUpdate(Item.Definition.BehaviorData);
                            }
                            else
                            {
                                goto case 1;
                            }

                            break;
                    }

                    break;
            }

            return true;
        }
    }
}
