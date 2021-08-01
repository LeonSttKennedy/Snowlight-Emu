using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;

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
            Actor.OverrideClipping = !Actor.OverrideClipping;
            Actor.ApplyEffect(Actor.TeleportEnabled ? 0 : 23);
            Session.CurrentEffect = 0;
        }
    }
}
