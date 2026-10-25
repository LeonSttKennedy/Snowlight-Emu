using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Game.Characters;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    class FacelessCommand : IChatCommand
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
                return ExternalTexts.GetValue("command_faceless_description");
            }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            string Faceless = "hd-99999-";

            string GenderString = Session.CharacterInfo.Gender == CharacterGender.Male ? "m" : "f";
            List<string> UserClothes = Session.CharacterInfo.Figure.Split('.').ToList();

            string NewFigure = UserFigureHelper.ReplaceHeadPart(Faceless, UserClothes);

            Session.CharacterInfo.UpdateFigure(GenderString, NewFigure);
            Session.SendInfoUpdate();

            Session.SendData(NotificationEventComposer.Compose(ExternalTexts.GetValue("command_faceless_info")));
        }
    }
}
