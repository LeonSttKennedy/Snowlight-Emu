using Snowlight.Util;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    class SpectatorCommand : IChatCommand
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
                return "<help>";
            }
        }

        public string Description
        {
            get
            {
                return ExternalTexts.GetValue("command_spectator_description");
            }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Params.Length > 1)
            {
                string Help = Params[1];

                if (Help == "help")
                {
                    Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_spectator_help")));
                }
                else
                {
                    Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_invalid_syntax") + " - :spectator <help>", 0, ChatType.Whisper));
                }

                return;
            }

            Session.SpectatorQueue = !Session.SpectatorQueue;
            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_spectator_" + Session.SpectatorQueue.ToString().ToLower()), 0, ChatType.Whisper));
        }
    }
}
