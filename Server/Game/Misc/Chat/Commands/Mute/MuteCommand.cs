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
    class MuteCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "mute"; }
        }

        public string Parameters
        {
            get { return "<username> <seconds>"; }
        }

        public string Description
        {
            get { return "Makes a user mute for time determined."; }
        }
        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Params.Length < 2)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_invalid_syntax") + " - :mute <username> [length in seconds]", 0, ChatType.Whisper));
                return;
            }

            string Username = UserInputFilter.FilterString(Params[1].Trim());
            int TimeToMute = 0;

            if (Params.Length >= 3)
            {
                int.TryParse(Params[2], out TimeToMute);
            }

            if (TimeToMute <= 0)
            {
                TimeToMute = 300;
            }

            if (TimeToMute > 3600)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_mute_maxtime"), 0, ChatType.Whisper));
                return;
            }

            Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Username));

            if (TargetSession == null || TargetSession.HasRight("mute"))
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_mute_error", Username), 0, ChatType.Whisper));
                return;
            }

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                TargetSession.CharacterInfo.Mute(MySqlClient, TimeToMute);
                ModerationLogs.LogModerationAction(MySqlClient, Session, "Muted user",
                    "User '" + TargetSession.CharacterInfo.Username + "' (ID " + TargetSession.CharacterId + ") for " + TimeToMute + " seconds.");
            }

            TargetSession.SendData(RoomMutedComposer.Compose(TimeToMute));
            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_mute_success", new string[] { Username, TimeToMute.ToString() }), 0, ChatType.Whisper));
        }
    }
}
