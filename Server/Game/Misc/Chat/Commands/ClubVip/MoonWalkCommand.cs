using Snowlight.Util;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    class MoonWalkCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get
            { 
                return "club_vip";
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
                return ExternalTexts.GetValue("command_moonwalk_description");
            }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Actor.OverrideClipping || Actor.TeleportEnabled)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_moonwalk_error"), 0, ChatType.Whisper));
                return;
            }

            Actor.MoonWalkEnabled = !Actor.MoonWalkEnabled;
            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_moonwalk_" + Actor.MoonWalkEnabled.ToString().ToLower()), 0, ChatType.Whisper));
        }
    }
}
