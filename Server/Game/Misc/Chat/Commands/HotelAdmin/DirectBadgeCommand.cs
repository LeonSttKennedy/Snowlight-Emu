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
    class DirectBadgeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "hotel_admin"; }
        }

        public string Parameters
        {
            get { return "<username> <code>"; }
        }

        public string Description
        {
            get { return "Gives to a single user an badge."; }
        }
        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            // Verify if username or badge code is empty
            if (Params.Length < 3)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_invalid_syntax") + " - :directbadge <username> <badge code>", 0, ChatType.Whisper));
                return;
            }

            string Username = UserInputFilter.FilterString(Params[1].Trim());
            string BadgeCode = UserInputFilter.FilterString(Params[2].Trim());

            // Verify if badge code is in database
            BadgeDefinition BadgeToGive = RightsManager.GetBadgeDefinitionByCode(BadgeCode);
            if (BadgeToGive == null)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_dmbadge_badge_error"), 0, ChatType.Whisper));
                return;
            }

            // Verify the user session is connected
            Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Username));
            if (TargetSession == null)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_targetuser_error"), 0, ChatType.Whisper));
                return;
            }

            // Getting the current room instance the user is in
            RoomInstance TargetInstance = RoomManager.GetInstanceByRoomId(TargetSession.CurrentRoomId);
            if (TargetInstance == null)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_directbadge_instance_error"), 0, ChatType.Whisper));
                return;
            }

            // Just to confirm if user is in a room
            RoomActor TargetActor = TargetInstance.GetActorByReferenceId(TargetSession.CharacterId);
            if (TargetActor == null)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_directbadge_actor_error"), 0, ChatType.Whisper));
                return;
            }

            // Also verify if badge has rights set, after this verification, executes the addition of badge
            if (RightsManager.GetRightsForBadge(BadgeToGive).Count == 0)
            {
                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    if (!TargetSession.BadgeCache.ContainsCode(BadgeCode))
                    {
                        TargetSession.BadgeCache.UpdateAchievementBadge(MySqlClient, BadgeToGive.Code, BadgeToGive, TargetSession.AchievementCache, "static");

                        InventoryBadge UserBadge = Session.BadgeCache.GetBadge(BadgeCode);
                        TargetSession.NewItemsCache.MarkNewItem(MySqlClient, 4, UserBadge.Id);
                        TargetSession.NewItemsCache.SendNewItems(TargetSession);

                        TargetSession.SendData(UserBadgeInventoryComposer.Compose(TargetSession.BadgeCache.Badges, TargetSession.BadgeCache.EquippedBadges));
                        TargetSession.SendData(RoomChatComposer.Compose(TargetActor.Id, ExternalTexts.GetValue("command_dmbadge_targetuser_success"), 1, ChatType.Whisper));

                        Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_directbadge_success")));
                        ModerationLogs.LogModerationAction(MySqlClient, Session, "Had given a badge",
                            Session.CharacterInfo.Username + " had give a badge ( " + BadgeToGive.Code + " ) to " + TargetSession.CharacterInfo.Username);
                    }
                    else if (TargetSession.BadgeCache.ContainsCode(BadgeCode))
                    {
                        Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_directbadge_error"), 0, ChatType.Whisper));
                    }
                }
            }
            else
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_dmbadge_rights_error"), 0, ChatType.Whisper));
                return;
            }
        }
    }
}
