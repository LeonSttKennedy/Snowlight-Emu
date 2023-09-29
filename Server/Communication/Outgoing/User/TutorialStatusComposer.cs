using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Snowlight.Game.Sessions;
using Snowlight.Game.Achievements;

namespace Snowlight.Communication.Outgoing
{
    public static class TutorialStatusComposer
    {
        public static ServerMessage Compose(Session Session)
        {
            UserAchievement AvatarLooksData = Session.AchievementCache.GetAchievementData("ACH_AvatarLooks");
            bool HasChangedLooks = AvatarLooksData != null;

            UserAchievement NameData = Session.AchievementCache.GetAchievementData("ACH_Name");
            bool HasChangedName = NameData != null;

            UserAchievement StudentData = Session.AchievementCache.GetAchievementData("ACH_Student");
            bool HasCalledGuide = StudentData != null;

            ServerMessage Message = new ServerMessage(OpcodesOut.TUTORIAL_STATUS);
            Message.AppendBoolean(HasChangedLooks);     // User Has Changed Looks
            Message.AppendBoolean(HasChangedName);      // User Has Changed Name
            Message.AppendBoolean(HasCalledGuide);      // User Has Called GuideBot
            return Message;
        }
    }
}
