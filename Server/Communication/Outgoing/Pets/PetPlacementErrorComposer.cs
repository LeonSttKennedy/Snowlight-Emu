using System;

using Snowlight.Game.Pets;

namespace Snowlight.Communication.Outgoing
{
    public enum PetPlacingError
    {
        PetsDisabledInThisHotel = 0,
        PetsDisabledInThisRoom = 1,
        ReachPetLimitForThisRoom = 2,
        GuestPetPlacingError = 3,
        RoomOwnerPlacingError = 4
    }

    public static class PetPlacementErrorComposer
    {
        public static ServerMessage Compose(PetPlacingError ErrorCode)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.PET_PLACING_ERROR);
            Message.AppendInt32((int)ErrorCode);
            return Message;
        }
    }
}
