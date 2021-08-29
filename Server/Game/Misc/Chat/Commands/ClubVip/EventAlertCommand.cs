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
    class EventAlertCommand : IChatCommand
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
            get { return "Send a hotel alert for your event."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            TimeSpan TimeSpan = DateTime.Now - Program.LastEventHosted;

            if (Instance.Info.OwnerId != Session.CharacterId || Instance.Info.Type == RoomType.Public)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_eventalert_error"), 0, ChatType.Whisper));
                return;
            }

            if (TimeSpan.Hours >= 1)
            {
                SessionManager.BroadcastPacket(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_eventalert_text", Session.CharacterInfo.Username)));
                Program.LastEventHosted = DateTime.Now;
                return;
            }
            else
            {
                int num = checked(60 - TimeSpan.Minutes);
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_eventalert_cooldown", num.ToString()), 0, ChatType.Whisper));
                return;
            }
        }
    }
}
