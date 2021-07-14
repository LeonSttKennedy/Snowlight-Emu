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
    class AnonimateHotelAlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "hotel_admin"; }
        }

        public string Parameters
        {
            get { return "<message>"; }
        }

        public string Description
        {
            get { return "Sends an anonimate global alert."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if(Params.Length == 1)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_alert_error"), 0, ChatType.Whisper));
                return;
            }

            string Message = CommandManager.MergeParams(Params, 1).Replace("\\n", "\n");

            SessionManager.BroadcastPacket(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_ha_fixed_text") + "\r\n" + Message));
            
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                ModerationLogs.LogModerationAction(MySqlClient, Session, "Sent a anonimate global alert",
                   "Message: '" + Message + "'");
            }
        }
    }
}
