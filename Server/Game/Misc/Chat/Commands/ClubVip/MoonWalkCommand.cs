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
    class MoonWalkCommand : IChatCommand
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
            get { return "Allows you walk backwards."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Actor.OverrideClipping || Actor.TeleportEnabled)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_moonwalk_error"), 0, ChatType.Whisper));
                return;
            }

            Actor.MoonWalkEnabled = !Actor.MoonWalkEnabled;
            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_moonwalk_" + Actor.MoonWalkEnabled.ToString().ToLower()), 0, ChatType.Whisper));
        }
    }
}
