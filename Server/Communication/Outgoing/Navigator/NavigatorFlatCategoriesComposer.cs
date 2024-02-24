using System;
using System.Collections.Generic;

using Snowlight.Game.Navigation;
using Snowlight.Game.Sessions;

namespace Snowlight.Communication.Outgoing
{
    public static class NavigatorFlatCategoriesComposer
    {
        public static ServerMessage Compose(Session Session, List<FlatCategory> Categories)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.NAVIGATOR_FLAT_CATEGORIES);
            Message.AppendInt32(CalcSize(Session, Categories));

            foreach (FlatCategory Category in Categories)
            {
                if(Category.RequiredRight.Length > 0 && !Session.HasRight(Category.RequiredRight))
                {
                    continue;
                }

                Message.AppendInt32(Category.Id);
                Message.AppendStringWithBreak(Category.Title);
                Message.AppendBoolean(Category.Visible);
            }

            return Message;
        }

        private static int CalcSize(Session Session, List<FlatCategory> Categories)
        {
            int Num = 0;

            foreach (FlatCategory Category in Categories)
            {
                if(Category.RequiredRight.Length > 0 && !Session.HasRight(Category.RequiredRight))
                {
                    continue;
                }

                Num++;
            }

            return Num;
        }
    }
}
