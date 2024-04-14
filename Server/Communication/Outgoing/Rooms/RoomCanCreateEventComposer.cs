using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Communication.Outgoing
{
    public enum RoomCanCreateEventError
    {
        CanCreate = 0,
        NeedToBeInaRoom = 1,
        OnlyRoomOwner = 2,
        DoorLocked = 3,
        EventsAreDisabled = 4,
        AlreadyCreated = 5,
        AlreadyCreatedInAnotherRoom = 6
    }

    public static class RoomCanCreateEventComposer
    {
        public static ServerMessage Compose(RoomCanCreateEventError ErrorCode)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.ROOM_EVENT_CAN_CREATE);
            Message.AppendBoolean((int)ErrorCode < 1);
            Message.AppendInt32((int)ErrorCode);
            return Message;
        }
    }
}
