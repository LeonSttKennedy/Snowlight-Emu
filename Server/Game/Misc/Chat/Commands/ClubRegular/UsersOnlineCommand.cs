using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    class UsersOnlineCommand : IChatCommand
    {
        public string PermissionRequired 
        {
            get { return "club_regular";  }
        }

        public string Parameters 
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Shows who are online."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            List<string> OnlineUsers = SessionManager.ConnectedUserData.Values.ToList();

            Session.SendData(NotificationMessageComposer.Compose(String.Concat(new object[] {
                            "There are currently " + OnlineUsers.Count + " user(s) online.\n",
                            "List of users online:\n\n",
                            string.Join(", ", OnlineUsers)
                        })));
        }
    }
}
