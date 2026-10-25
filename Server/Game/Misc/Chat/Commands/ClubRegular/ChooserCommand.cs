using Snowlight.Util;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;

namespace Snowlight.Game.Misc
{
    class ChooserCommand : IChatCommand
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
                return ExternalTexts.GetValue("command_chooser_description"); 
            }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {

        }
    }
}
