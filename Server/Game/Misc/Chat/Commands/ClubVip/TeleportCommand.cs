using Snowlight.Communication.Outgoing;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Game.Misc
{
    class TeleportCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "club_vip"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Teleport to wherever you want within in a room."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            Actor.TeleportEnabled = !Actor.TeleportEnabled;
            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_teleport_" + Actor.TeleportEnabled.ToString().ToLower()), 0, ChatType.Whisper));
        }
    }
}
