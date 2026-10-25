using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;

namespace Snowlight.Game.Misc
{
    class CommandsCommand : IChatCommand
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
                return ExternalTexts.GetValue("command_commands_description");
            }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {

        }
    }
}
