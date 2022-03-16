using Snowlight.Communication;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Infobus;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Storage;
using Snowlight.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Game.Misc
{
    class InfobusCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "moderation_tool"; }
        }

        public string Parameters
        {
            get { return "<open/pool>"; }
        }

        public string Description
        {
            get { return "Infobus command."; }
        }
        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if(Params.Length == 1)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_infobus_info"), 0, ChatType.Whisper));
                return;
            }

            string InfobusCommand = UserInputFilter.FilterString(Params[1].Trim());

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                switch (InfobusCommand.ToLower())
                {
                    case "pool":
                        {
                            return;
                        }

                    case "open":
                        {
                            ServerSettings.InfobusStatus = InfobusStatus.Open;
                            ServerSettings.UpdateInfobusStatus(MySqlClient);
                            ServerSettings.Initialize(MySqlClient);

                            /* IDK how to open infobus door. 
                             * Old versions the header is 71 
                             * KEPLER by Quackster (Alex)
                            ServerMessage SM = new ServerMessage(71);
                            SM.AppendStringWithBreak("bus open");
                            Instance.BroadcastMessage(SM);*/

                            Instance.BroadcastMessage(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_infobus_opened")));
                            return;
                        }

                    case "close":
                        {
                            ServerSettings.InfobusStatus = InfobusStatus.Closed;
                            ServerSettings.UpdateInfobusStatus(MySqlClient);
                            ServerSettings.Initialize(MySqlClient);

                            /* IDK how to open infobus door. 
                             * Old versions the header is 71 
                             * KEPLER by Quackster (Alex)
                            ServerMessage SM = new ServerMessage(71);
                            SM.AppendStringWithBreak("bus close");
                            Instance.BroadcastMessage(SM);*/
                            return;
                        }

                    default:
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_infobus_info"), 0, ChatType.Whisper));
                            return;
                        }
                }
            }
        }
    }
}
