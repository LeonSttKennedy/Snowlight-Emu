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
    class FlagmeCommand : IChatCommand
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
            get { return "Changes your Username."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            Session.SendData(ChangeUsernameWindowComposer.Compose());
        }
    }
}
