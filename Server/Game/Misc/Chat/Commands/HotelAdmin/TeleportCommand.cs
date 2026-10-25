using Snowlight.Util;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    class TeleportCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get
            {
                return "hotel_admin";
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
                return ExternalTexts.GetValue("command_teleport_description");
            }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Actor.OverrideClipping)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_teleport_error"), 0, ChatType.Whisper));
                return;
            }
            
            Actor.TeleportEnabled = !Actor.TeleportEnabled;
            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_teleport_" + Actor.TeleportEnabled.ToString().ToLower()), 0, ChatType.Whisper));
        }
    }
}
