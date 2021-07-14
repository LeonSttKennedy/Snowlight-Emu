using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Rooms;
using Snowlight.Game.Rights;
using Snowlight.Game.Sessions;
using Snowlight.Game.Moderation;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    class MassBadgeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "hotel_admin"; }
        }

        public string Parameters
        {
            get { return "<method> <badge code>"; }
        }

        public string Description
        {
            get { return "Gives to all users an badge."; }
        }
        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Params.Length < 3)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_invalid_syntax") + " - :massbadge <method: room or all> <badge code>", 0, ChatType.Whisper));
                return;
            }

            string BadgeCode = UserInputFilter.FilterString(Params[2].Trim());

            // Verify if badge code is in database
            Badge BadgeToGive = RightsManager.GetBadgeByCode(BadgeCode);
            if (BadgeToGive == null)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_dmbadge_badge_error"), 0, ChatType.Whisper));
                return;
            }

            string Method = UserInputFilter.FilterString(Params[1].Trim());
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                if (RightsManager.GetRightsForBadge(BadgeToGive).Count == 0)
                {
                    switch (Method.ToLower())
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
                                            if (!TargetSession.BadgeCache.Badges.Contains(BadgeToGive))
                                            {
                                                TargetSession.BadgeCache.UpdateAchievementBadge(MySqlClient, BadgeToGive.Code, BadgeToGive, "static");
                                                TargetSession.NewItemsCache.MarkNewItem(MySqlClient, 4, BadgeToGive.Id);
                                                TargetSession.SendData(InventoryNewItemsComposer.Compose(4, BadgeToGive.Id));

                                                TargetSession.BadgeCache.ReloadCache(MySqlClient, TargetSession.AchievementCache);

                                                TargetSession.SendData(UserBadgeInventoryComposer.Compose(TargetSession.BadgeCache.Badges, TargetSession.BadgeCache.EquippedBadges));
                                                TargetSession.SendData(RoomChatComposer.Compose(RoomActors.Id, ExternalTexts.GetValue("command_dmbadge_targetuser_success"), 1, ChatType.Whisper));
                                            }
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
                                            if (!TargetSession.BadgeCache.Badges.Contains(BadgeToGive))
                                            {
                                                TargetSession.BadgeCache.UpdateAchievementBadge(MySqlClient, BadgeToGive.Code, BadgeToGive, "static");
                                                TargetSession.NewItemsCache.MarkNewItem(MySqlClient, 4, BadgeToGive.Id);
                                                TargetSession.SendData(InventoryNewItemsComposer.Compose(4, BadgeToGive.Id));

                                                TargetSession.BadgeCache.ReloadCache(MySqlClient, TargetSession.AchievementCache);

                                                TargetSession.SendData(UserBadgeInventoryComposer.Compose(TargetSession.BadgeCache.Badges, TargetSession.BadgeCache.EquippedBadges));
                                                TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_dmbadge_targetuser_success") + "\r\n- " + Session.CharacterInfo.Username));
                                            }
                                        }
                                    }
                                }
                                goto End;
                            }

                        default:
                            {
                                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_massbadge_method_error"), 0, ChatType.Whisper));
                                return;
                            }
                    }
                }
                else
                {
                    Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_dmbadge_rights_error"), 0, ChatType.Whisper));
                    return;
                }

                End:
                string ToSend = (Method.Equals("room") ? "command_massbadge_room_success" : "command_massbadge_all_success");
                string ToMethod = (Method.Equals("room") ? "to room: " + Instance.Info.Name + " (ID: " + Instance.Info.Id + ")." : "to all online users.");
                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue(ToSend)));
                ModerationLogs.LogModerationAction(MySqlClient, Session, "Had given a badge",
                    Session.CharacterInfo.Username + " had give a badge ( " + BadgeToGive.Code + " ) " + ToMethod);
            }
        }
    }
}
