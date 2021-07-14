using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    class PickallCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return ""; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Picks up all furniture from your room."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (!Instance.CheckUserRights(Session, true))
            {
                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_pickall_error")));
                return;
            }

            Instance.PickAllToUserInventory(Session);
        }
    }
}
