using System;

using Snowlight.Game.Pets;

namespace Snowlight.Communication.Outgoing
{
    public static class PetReceivedComposer
    {
        public static ServerMessage Compose(bool BoughtAsGift, Pet PetData)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.PET_RECEIVED);
            Message.AppendBoolean(BoughtAsGift);
            Message.AppendUInt32(PetData.Id);
            Message.AppendStringWithBreak(PetData.Name);
            Message.AppendInt32(PetData.Type);
            Message.AppendInt32(PetData.Race);
            Message.AppendStringWithBreak(PetData.ColorCode);
            return Message;
        }
    }
}
