using Snowlight.Util;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    class FlagmeCommand : IChatCommand
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
                return ExternalTexts.GetValue("command_flagme_description");
            }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            Session.SendData(ChangeUsernameWindowComposer.Compose());
        }
    }
}
