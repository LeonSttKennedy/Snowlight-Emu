using System;

using Snowlight.Game.Achievements;
using Snowlight.Game.Rights;

namespace Snowlight.Communication.Outgoing
{
    public static class AchievementUnlockedComposer
    {
        public static ServerMessage Compose(Achievement Achievement, int Level, int PointReward, int ActivityPointsType, int PixelReward)
        {
            Badge Badge = RightsManager.GetBadgeByCode(Achievement.GroupName);

            ServerMessage Message = new ServerMessage(OpcodesOut.ACHIEVEMENT_UNLOCKED);
            Message.AppendUInt32(Achievement.Id);                                                           // Achievement ID
            Message.AppendInt32(Level);                                                                     // Achieved level
            Message.AppendInt32(Badge != null ? (int)Badge.Id : -1);                                        // Badge Id ?
            Message.AppendStringWithBreak(Achievement.GroupName + Level);                                   // Achieved name
            Message.AppendInt32(PointReward);                                                               // Point reward
            Message.AppendInt32(PixelReward);                                                               // Activity points reward
            Message.AppendInt32(ActivityPointsType);                                                        // Activity points type.
            Message.AppendInt32(0);                                                                         // Additional points for share on social network
            Message.AppendUInt32(Achievement.Id);                                                           // Achievement ID (Again?)
            Message.AppendStringWithBreak(Level > 1 ? Achievement.GroupName + (Level - 1) : string.Empty);  // Removed badge code
            Message.AppendStringWithBreak(Achievement.Category.ToString());                                 // Category
            return Message;
        }
    }
}
