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
    class UsersOnlineCommand : IChatCommand
    {
        public string PermissionRequired 
        {
            get
            {
                return "club_regular";  
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
                return ExternalTexts.GetValue("command_whos_online_description");
            }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            List<string> OnlineUsers = SessionManager.ConnectedUserData.Values.ToList();

            Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_whos_online_text",
                new string[] { OnlineUsers.Count.ToString(), string.Join(", ", OnlineUsers) })));
        }
    }
}
