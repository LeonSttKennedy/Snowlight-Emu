using System;

using Snowlight.Game.Pets;

namespace Snowlight.Communication.Outgoing
{
    public static class PetRespectComposer
    {
        public static ServerMessage Compose(Pet PetData)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.PET_RESPECT);
            Message.AppendInt32(PetData.Score);
            Message.AppendUInt32(PetData.OwnerId);
            Message.AppendUInt32(PetData.Id);
            Message.AppendStringWithBreak(PetData.Name);
            Message.AppendInt32(PetData.Type);
            Message.AppendInt32(PetData.Race);
            Message.AppendStringWithBreak(PetData.ColorCode);
            return Message;
        }
    }
}
