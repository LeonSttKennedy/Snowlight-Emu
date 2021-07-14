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
    class RoomAlertLinkCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "moderation_tool"; }
        }

        public string Parameters
        {
            get { return "<link> <message>"; }
        }

        public string Description
        {
            get { return "Sends an alert to all users in current room with link."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Params.Length <= 2)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_alert_error"), 0, ChatType.Whisper));
                return;
            }

            string Url = Params[1];
            string Message = CommandManager.MergeParams(Params, 2).Replace("\\n", "\n");

            foreach (RoomActor RoomActor in Instance.Actors)
            {
                if (!RoomActor.IsBot)
                {
                    Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(RoomActor.Name));
                    TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_ra_fixed_text") + "\r\n" + Message + "\r\n- " + Session.CharacterInfo.Username, Url));
                }
            }

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                ModerationLogs.LogModerationAction(MySqlClient, Session, "Sent an alert to a room with link",
                   "Message: '" + Message + "' \nLink: " + Url);
            }
        }
    }
}
