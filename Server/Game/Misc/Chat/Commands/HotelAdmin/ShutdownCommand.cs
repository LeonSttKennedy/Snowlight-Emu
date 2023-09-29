using Snowlight.Communication.Outgoing;
using Snowlight.Game.Moderation;
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
    class ShutdownCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "hotel_admin"; }
        }

        public string Parameters
        {
            get { return "<hour>"; }
        }

        public string Description
        {
            get { return "Shutdown schedule command."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Params.Length < 2)
            {
                if (ShutdownCommandWorker.Shutdown == true)
                {
                    Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_shutdown_cancel_scheduled"), 0, ChatType.Whisper));
                    ShutdownCommandWorker.Shutdown = false;
                    ShutdownCommandWorker.TenMinToCloseAdvise = false;
                }
                else
                {
                    Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_shutdown_error"), 0, ChatType.Whisper));
                    return;
                }
            }

            string Hour = Params[1];

            if (Hour.Contains(":"))
            {
                TimeSpan TimeDiff = TimeSpan.Parse(Hour) - TimeSpan.Parse(DateTime.Now.ToShortTimeString());

                if (TimeDiff.TotalMinutes <= 5)
                {
                    Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_shutdown_time_smaller_than_5min_error"), 0, ChatType.Whisper));
                    return;
                }
                else
                {
                    ShutdownCommandWorker.Shutdown = true;
                    ShutdownCommandWorker.CloseHour = TimeSpan.Parse(Hour);
                    Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_shutdown_success", Hour), 0, ChatType.Whisper));

                    using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                    {
                        ModerationLogs.LogModerationAction(MySqlClient, Session, "Shutdown command log",
                           "Staff member " + Session.CharacterInfo.Username + " had power off the server.");
                    }
                } 
            }
            else
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_shutdown_set_error"), 0, ChatType.Whisper));
                return;
            }
        }
    }
}
