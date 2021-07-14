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
    class HotelAlertLinkCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "hotel_admin"; }
        }

        public string Parameters
        {
            get { return "<link> <message>"; }
        }

        public string Description
        {
            get { return "Sends a global alert with link."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Params.Length <= 2)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_alert_error"), 0, ChatType.Whisper));
            }

            string Url = Params[1];
            string Message = CommandManager.MergeParams(Params, 2).Replace("\\n", "\n");

            SessionManager.BroadcastPacket(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_ha_fixed_text") + "\r\n" + Message + "\r\n- " + Session.CharacterInfo.Username, Url));

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                ModerationLogs.LogModerationAction(MySqlClient, Session, "Sent a global alert with link",
                   "Message: '" + Message + "' \nLink: " + Url);
            }
        }
    }
}
