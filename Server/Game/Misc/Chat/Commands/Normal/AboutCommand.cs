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
    class AboutCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get
            {
                return "";
            }
        }

        public string Parameters
        {
            get
            {
                return "";
            }
        }

        public string Description
        {
            get
            {
                return ExternalTexts.GetValue("command_about_description");
            }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            Session.SendData(UserAlertModernComposer.Compose("Powered by Snowlight", "This hotel is proudly powered by Snowlight, the premium open-source Habbo Hotel emulator."));
        }
    }
}
