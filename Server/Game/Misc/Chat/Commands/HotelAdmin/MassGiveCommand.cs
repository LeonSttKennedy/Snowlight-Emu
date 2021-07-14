﻿using System;
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
    class MassGiveCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "hotel_admin"; }
        }

        public string Parameters
        {
            get { return "<method> <type> <amount>"; }
        }

        public string Description
        {
            get { return "Give forall users an amount of coin."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Params.Length < 4)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_invalid_syntax") + " - :massgive <method: room or all> <type: coins/credits or pixels> <quantity>", 0, ChatType.Whisper));
                return;
            }

            int Amount;
            string Type = UserInputFilter.FilterString(Params[1].Trim());
            string Currency = UserInputFilter.FilterString(Params[2].Trim());


            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                switch (Currency.ToLower())
                {
                    case "credits":
                    case "coins":
                        {
                            if (int.TryParse(Params[3], out Amount))
                            {
                                switch (Type.ToLower())
                                {
                                    case "room":
                                        {
                                            foreach (RoomActor RoomActors in Instance.Actors)
                                            {
                                                if (!RoomActors.IsBot)
                                                {
                                                    Session TargetSession = SessionManager.GetSessionByCharacterId(RoomActors.ReferenceId);

                                                    if (TargetSession.CharacterId != Session.CharacterId)
                                                    {
                                                        TargetSession.CharacterInfo.UpdateCreditsBalance(MySqlClient, Amount);
                                                        TargetSession.SendData(CreditsBalanceComposer.Compose(TargetSession.CharacterInfo.CreditsBalance));
                                                        TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_give_targetuser_success", new string[] { Amount.ToString(), Currency }) + "\r\n- " + Session.CharacterInfo.Username));
                                                    }
                                                }
                                            }
                                            goto End;
                                        }

                                    case "all":
                                        {
                                            Dictionary<uint, Session> Sessions = SessionManager.Sessions;
                                            foreach (Session TargetSession in Sessions.Values)
                                            {
                                                if (TargetSession.Authenticated && !TargetSession.Stopped)
                                                {
                                                    if (TargetSession.CharacterId != Session.CharacterId)
                                                    {
                                                        TargetSession.CharacterInfo.UpdateCreditsBalance(MySqlClient, Amount);
                                                        TargetSession.SendData(CreditsBalanceComposer.Compose(TargetSession.CharacterInfo.CreditsBalance));
                                                        TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_give_targetuser_success", new string[] { Amount.ToString(), Currency }) + "\r\n- " + Session.CharacterInfo.Username));
                                                    }
                                                }
                                            }
                                            goto End;
                                        }

                                    default:
                                        {
                                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_currency_error", Type), 0, ChatType.Whisper));
                                            return;
                                        }
                                }
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
                                switch (Type.ToLower())
                                {
                                    case "room":
                                        {
                                            foreach (RoomActor RoomActors in Instance.Actors)
                                            {
                                                if (!RoomActors.IsBot)
                                                {
                                                    Session TargetSession = SessionManager.GetSessionByCharacterId(RoomActors.ReferenceId);

                                                    if (TargetSession.CharacterId != Session.CharacterId)
                                                    {
                                                        TargetSession.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, Amount);
                                                        TargetSession.SendData(ActivityPointsBalanceComposer.Compose(TargetSession.CharacterInfo.ActivityPointsBalance, Amount));
                                                        TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_give_targetuser_success", new string[] { Amount.ToString(), Currency }) + "\r\n- " + Session.CharacterInfo.Username));
                                                    }
                                                }
                                            }
                                            goto End;
                                        }

                                    case "all":
                                        {
                                            Dictionary<uint, Session> Sessions = SessionManager.Sessions;
                                            foreach (Session TargetSession in Sessions.Values)
                                            {
                                                if (TargetSession.Authenticated && !TargetSession.Stopped)
                                                {
                                                    if (TargetSession.CharacterId != Session.CharacterId)
                                                    {
                                                        TargetSession.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, Amount);
                                                        TargetSession.SendData(ActivityPointsBalanceComposer.Compose(TargetSession.CharacterInfo.ActivityPointsBalance, Amount));
                                                        TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_give_targetuser_success", new string[] { Amount.ToString(), Currency }) + "\r\n- " + Session.CharacterInfo.Username));
                                                    }
                                                }
                                            }
                                            goto End;
                                        }

                                    default:
                                        {
                                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_method_error", Type), 0, ChatType.Whisper));
                                            return;
                                        }
                                }
                            }
                            else
                            {
                                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_quantity_error"), 0, ChatType.Whisper));
                                return;
                            }
                        }

                    default:
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_currency_error", Currency), 0, ChatType.Whisper));
                            return;
                        }
                }
                End:
                string ToSend = (Type.Equals("room") ? "command_massgive_room_success" : "command_massgive_all_success");
                string ToMethod = (Type.Equals("room") ? "to room: " + Instance.Info.Name + " (ID: " + Instance.Info.Id + ")." : "to all online users.");
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue(ToSend), 0, ChatType.Whisper));
                ModerationLogs.LogModerationAction(MySqlClient, Session, "Had given " + Type,
                    Session.CharacterInfo.Username + " had give " + Amount + " " + Currency + " " + ToMethod);
            }
        }
    }
}
