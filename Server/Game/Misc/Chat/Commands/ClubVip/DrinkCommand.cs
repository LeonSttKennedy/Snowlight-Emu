using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;
using Snowlight.Util;
using Snowlight.Game.AvatarEffects;

namespace Snowlight.Game.Misc
{
    class DrinkCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "club_vip"; }
        }

        public string Parameters
        {
            get { return "<help>"; }
        }

        public string Description
        {
            get { return "Gives to you a drink."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if(Params.Length < 2)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_drink_info"), 0, ChatType.Whisper));
                return;
            }

            string DrinkCommand = UserInputFilter.FilterString(Params[1].Trim());
            switch (DrinkCommand.ToLower())
            {
                case "drop":

                    Actor.CarryItem(0, true);

                    return;

                case "help":

                    Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_drink_help")));

                    return;

                default:

                    int DrinkId = int.Parse(Params[1]);

                    if (DrinkId > 0 && DrinkId <= 66)
                    {
                        Actor.CarryItem(DrinkId, true);
                    }
                    else
                    {
                        goto case "drop";
                    }

                    return;
            }
        }
    }
}
