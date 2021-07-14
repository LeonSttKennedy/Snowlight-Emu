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
    class DisableDiagonalCommand : IChatCommand
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
            get { return "Disable diagonal walking in your room."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (!Instance.CheckUserRights(Session, true))
            {
                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_disablediagonal_error")));
                return;
            }

            Instance.DisableDiagonal = !Instance.DisableDiagonal;
            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_disablediagonal_" + Instance.DisableDiagonal.ToString().ToLower()), 4, ChatType.Whisper));
        }
    }
}
