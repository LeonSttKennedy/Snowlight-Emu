using System;

using Snowlight.Game.Pets;

namespace Snowlight.Communication.Outgoing
{
    public static class PetAddExperiencePointsComposer
    {
        public static ServerMessage Compose(Pet PetData, int AddExperience)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.ROOM_PET_ADD_EXPERIENCE);
            Message.AppendUInt32(PetData.Id);
            Message.AppendUInt32(PetData.VirtualId);
            Message.AppendInt32(AddExperience);
            return Message;
        }
    }
}
