using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Game.Moderation;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    class DirectGiveCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "hotel_admin"; }
        }

        public string Parameters
        {
            get { return "<username> <type> <amount>"; }
        }

        public string Description
        {
            get { return "Gives to a user an amount of coin."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Params.Length < 4)
            {
                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_invalid_syntax") + " - :directgive <username> <type: coins/credits, pixels, snowflake, hearts, giftpoints or shells> <quantity>"));
                return;
            }

            int Amount;
            string Username = UserInputFilter.FilterString(Params[1].Trim());

            // Verify the user session is connected
            Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Username));
            if (TargetSession == null)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_targetuser_error"), 0, ChatType.Whisper));
                return;
            }

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                string Type = UserInputFilter.FilterString(Params[2].Trim());
                switch (Type.ToLower())
                {
                    case "coins":
                    case "credits":
                        {
                            if (int.TryParse(Params[3], out Amount))
                            {
                                TargetSession.CharacterInfo.UpdateCreditsBalance(MySqlClient, Amount);
                                TargetSession.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                                TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_give_targetuser_success", new string[] { Amount.ToString(), Type }) + "\r\n- " + Session.CharacterInfo.Username));
                                goto End;
                            }
                            else
                            {
                                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_quantity_error"), 0, ChatType.Whisper));
                                return;
                            }
                        }

                    case "pixels":
                        {
                            if (int.TryParse(Params[3], out Amount))
                            {
                                TargetSession.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, SeasonalCurrencyList.Pixels, Amount);
                                TargetSession.SendData(UpdatePixelsBalanceComposer.Compose(TargetSession.CharacterInfo.ActivityPoints[0], Amount));
                                TargetSession.SendData(UserActivityPointsBalanceComposer.Compose(TargetSession.CharacterInfo.ActivityPoints));
                                TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_give_targetuser_success", new string[] { Amount.ToString(), Type }) + "\r\n- " + Session.CharacterInfo.Username));
                                goto End;
                            }
                            else
                            {
                                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_quantity_error"), 0, ChatType.Whisper));
                                return;
                            }
                        }

                    case "snowflakes":
                        {
                            if (int.TryParse(Params[3], out Amount))
                            {
                                TargetSession.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, SeasonalCurrencyList.Snowflakes, Amount);
                                TargetSession.SendData(UserActivityPointsBalanceComposer.Compose(TargetSession.CharacterInfo.ActivityPoints));
                                TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_give_targetuser_success", new string[] { Amount.ToString(), Type }) + "\r\n- " + Session.CharacterInfo.Username));
                                goto End;
                            }
                            else
                            {
                                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_quantity_error"), 0, ChatType.Whisper));
                                return;
                            }
                        }

                    case "hearts":
                        {
                            if (int.TryParse(Params[3], out Amount))
                            {
                                TargetSession.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, SeasonalCurrencyList.Hearts, Amount);
                                TargetSession.SendData(UserActivityPointsBalanceComposer.Compose(TargetSession.CharacterInfo.ActivityPoints));
                                TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_give_targetuser_success", new string[] { Amount.ToString(), Type }) + "\r\n- " + Session.CharacterInfo.Username));
                                goto End;
                            }
                            else
                            {
                                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_quantity_error"), 0, ChatType.Whisper));
                                return;
                            }
                        }

                    case "giftpoints":
                        {
                            if (int.TryParse(Params[3], out Amount))
                            {
                                TargetSession.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, SeasonalCurrencyList.Giftpoints, Amount);
                                TargetSession.SendData(UserActivityPointsBalanceComposer.Compose(TargetSession.CharacterInfo.ActivityPoints));
                                TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_give_targetuser_success", new string[] { Amount.ToString(), Type }) + "\r\n- " + Session.CharacterInfo.Username));
                                goto End;
                            }
                            else
                            {
                                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_quantity_error"), 0, ChatType.Whisper));
                                return;
                            }
                        }

                    case "shells":
                        {
                            if (int.TryParse(Params[3], out Amount))
                            {
                                TargetSession.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, SeasonalCurrencyList.Shells, Amount);
                                TargetSession.SendData(UserActivityPointsBalanceComposer.Compose(TargetSession.CharacterInfo.ActivityPoints));
                                TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_give_targetuser_success", new string[] { Amount.ToString(), Type }) + "\r\n- " + Session.CharacterInfo.Username));
                                goto End;
                            }
                            else
                            {
                                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_quantity_error"), 0, ChatType.Whisper));
                                return;
                            }
                        }

                    case "diamonds":
                        {
                            if (int.TryParse(Params[3], out Amount))
                            {
                                TargetSession.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, SeasonalCurrencyList.Diamonds, Amount);
                                TargetSession.SendData(UserActivityPointsBalanceComposer.Compose(TargetSession.CharacterInfo.ActivityPoints));
                                TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_give_targetuser_success", new string[] { Amount.ToString(), Type }) + "\r\n- " + Session.CharacterInfo.Username));
                                goto End;
                            }
                            else
                            {
                                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_quantity_error"), 0, ChatType.Whisper));
                                return;
                            }
                        }

                    case "clubgiftpoints":
                        {
                            if (int.TryParse(Params[3], out Amount))
                            {
                                TargetSession.SubscriptionManager.GiveGiftPoints(Amount);
                                TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_give_targetuser_success", new string[] { Amount.ToString(), Type }) + "\r\n- " + Session.CharacterInfo.Username));
                                goto End;
                            }
                            else
                            {
                                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_quantity_error"), 0, ChatType.Whisper));
                                return;
                            }
                        }

                    default:
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_currency_error", Type), 0, ChatType.Whisper));
                            return;
                        }
                }

                End:
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_success", new string[] { Amount.ToString(), Type, TargetSession.CharacterInfo.Username }), 0, ChatType.Whisper));
                ModerationLogs.LogModerationAction(MySqlClient, Session, "Had given " + Type,
                    Session.CharacterInfo.Username + " had give " + Amount + " " + Type + " to " + TargetSession.CharacterInfo.Username);

            }
        }
    }
}
