using System;

using Snowlight.Game.Pets;

namespace Snowlight.Communication.Outgoing
{
    public static class PetLevelUpComposer
    {
        public static ServerMessage Compose(Pet Pet)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.PET_LEVEL_UP);
            Message.AppendUInt32(Pet.Id);
            Message.AppendStringWithBreak(Pet.Name);
            Message.AppendInt32(Pet.Level);
            Message.AppendInt32(Pet.Type);
            Message.AppendInt32(Pet.Race);
            Message.AppendStringWithBreak(Pet.ColorCode);
            return Message;
        }
    }
}
