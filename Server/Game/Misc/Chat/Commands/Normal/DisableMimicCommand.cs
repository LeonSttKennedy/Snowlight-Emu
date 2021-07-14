using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    class DisableMimicCommand : IChatCommand
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
            get { return "Disable the ability to be mimicked or enable it again."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            Session.CharacterInfo.AllowMimic = !Session.CharacterInfo.AllowMimic;

            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_disablemimic_" + Session.CharacterInfo.AllowMimic.ToString().ToLower()), 0, ChatType.Whisper));

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                Session.CharacterInfo.UpdateMimicPreference(MySqlClient);
            }
        }
    }
}
