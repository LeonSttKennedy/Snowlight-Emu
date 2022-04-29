using System;

using Snowlight.Game.Pets;

namespace Snowlight.Communication.Outgoing
{
    public static class PetRespectComposer
    {
        public static ServerMessage Compose(uint PetId, Pet Pet)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.PET_RESPECT);
            Message.AppendInt32(Pet.Score);
            Message.AppendUInt32(Pet.OwnerId);
            Message.AppendUInt32(PetId);
            Message.AppendStringWithBreak(Pet.Name);
            Message.AppendBoolean(false);
            Message.AppendInt32(10);
            Message.AppendBoolean(false);
            Message.AppendInt32(-2);
            Message.AppendBoolean(true);
            Message.AppendStringWithBreak("281");
            return Message;
        }
    }
}
