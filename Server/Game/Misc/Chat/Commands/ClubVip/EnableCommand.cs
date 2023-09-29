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
    class EnableCommand : IChatCommand
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
            get { return "Sets a effect on your avatar."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if(Params.Length < 2)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_enable_info"), 0, ChatType.Whisper));
                return;
            }

            string EnableCommand = UserInputFilter.FilterString(Params[1].Trim());
            switch (EnableCommand.ToLower())
            {
                case "clear":

                    Actor.ApplyEffect(0);
                    Session.CurrentEffect = 0;

                    return;

                case "help":

                    Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_enable_help")));

                    return;

                default:

                    int EffectId = int.Parse(Params[1]);
                    if (EffectId > 0 && EffectId <= 70)
                    {

                        Actor.ApplyEffect(EffectId);
                        Session.CurrentEffect = EffectId;
                    }
                    else
                    {
                        goto case "clear";
                    }

                    return;
            }
        }
    }
}
