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
    class RoomMuteCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "mute"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Mutes the current room."; }
        }
        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (!Instance.RoomMuted)
            {
                Instance.RoomMuted = true;
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_roommute_success"), 0, ChatType.Whisper));

                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    ModerationLogs.LogModerationAction(MySqlClient, Session, "Muted room", "Room '"
                        + Instance.Info.Name + "' (ID " + Instance.RoomId + ")");
                }
            }
            else
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_roommute_error"), 0, ChatType.Whisper));
            }
        }
    }
}