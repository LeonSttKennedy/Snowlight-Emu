using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;
using Snowlight.Util;

namespace Snowlight.Game.Misc
{
    class ClippingCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "hotel_admin"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Walk wherever you want."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Actor.TeleportEnabled)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_clipping_error"), 0, ChatType.Whisper));
                return;
            }

            Actor.OverrideClipping = !Actor.OverrideClipping;
            Actor.ApplyEffect(Actor.ClippingEnabled ? 0 : 23);
            Session.CurrentEffect = 0;
        }
    }
}
