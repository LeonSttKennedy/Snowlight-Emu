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
    class UnmuteCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "mute"; }
        }

        public string Parameters
        {
            get { return "<username>"; }
        }

        public string Description
        {
            get { return "Unmute the user."; }
        }
        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Params.Length < 2)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_invalid_syntax") + " - :unmute <username>", 0, ChatType.Whisper));
                return;
            }

            string Username = UserInputFilter.FilterString(Params[1].Trim());

            Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Username));

            if (TargetSession == null)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_unmute_error", Username), 0, ChatType.Whisper));
                return;
            }

            if (!TargetSession.CharacterInfo.IsMuted)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_unmute_isnt_muted", Username), 0, ChatType.Whisper));
                return;
            }

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                TargetSession.CharacterInfo.Unmute(MySqlClient);
            }

            TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_unmute_target_success")));
            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_unmute_success", Username), 0, ChatType.Whisper));

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                ModerationLogs.LogModerationAction(MySqlClient, Session, "Unmuted user",
                    "User '" + TargetSession.CharacterInfo.Username + "' (ID " + TargetSession.CharacterId + ").");
            }
        }
    }
}