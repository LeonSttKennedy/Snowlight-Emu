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
    class RoomAlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "moderation_tool"; }
        }

        public string Parameters
        {
            get { return "<message>"; }
        }

        public string Description
        {
            get { return "Sends an alert to all users in current room."; }
        }
        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_alert_error"), 0, ChatType.Whisper));
                return;
            }

            string Message = CommandManager.MergeParams(Params, 1).Replace("\\n", "\n");
            foreach (RoomActor RoomActor in Instance.Actors)
            {
                if (!RoomActor.IsBot)
                {
                    Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(RoomActor.Name));
                    TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_ra_fixed_text") + "\r\n" + Message + "\r\n- " + Session.CharacterInfo.Username));
                }
            }

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                ModerationLogs.LogModerationAction(MySqlClient, Session, "Sent an alert to a room",
                   "Message: '" + Message + "'");
            }
        }
    }
}
