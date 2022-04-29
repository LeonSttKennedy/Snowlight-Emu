using System;

using Snowlight.Game.Pets;

namespace Snowlight.Communication.Outgoing
{
    public static class PetAddExperiencePointsComposer
    {
        public static ServerMessage Compose(uint PetId, int VirtualID, int AddExperience)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.ROOM_PET_UPDATE);
            Message.AppendUInt32(PetId);
            Message.AppendInt32(VirtualID);
            Message.AppendInt32(AddExperience);
            return Message;
        }
    }
}
