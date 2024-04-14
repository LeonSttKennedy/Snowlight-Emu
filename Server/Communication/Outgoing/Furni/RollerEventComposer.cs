using System;
using System.Linq;
using System.Collections.Generic;

using Snowlight.Game.Misc;
using Snowlight.Game.Items;
using Snowlight.Specialized;

namespace Snowlight.Communication.Outgoing
{
    public enum MovementType
    {
        None = 0,
        Walk = 1,
        Slide = 2
    }

    public static class RollerEventComposer
    {
        public static ServerMessage Compose(Vector2 From, Vector2 Target, uint RollerId, List<RollerEvents> Events)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.ROLLER_EVENT);
            Message.AppendInt32(From.X);
            Message.AppendInt32(From.Y);
            Message.AppendInt32(Target.X);
            Message.AppendInt32(Target.Y);

            List<RollerEvents> EventsWithItems = Events.Where(E => E.ItemId > 0).ToList();
            Message.AppendInt32(EventsWithItems.Count);

            foreach(RollerEvents ItemEvent in EventsWithItems)
            {
                Message.AppendUInt32(ItemEvent.ItemId);
                Message.AppendRawDouble(ItemEvent.FromZ);
                Message.AppendRawDouble(ItemEvent.TargetZ);
            }

            Message.AppendUInt32(RollerId);

            RollerEvents EventWithActor = Events.Where(E => E.ActorId > 0).FirstOrDefault();
            MovementType RollingType = EventWithActor != null ? EventWithActor.MovementType : MovementType.None;
            Message.AppendInt32((int)RollingType);

            if(EventWithActor != null) 
            {
                Message.AppendUInt32(EventWithActor.ActorId);
                Message.AppendRawDouble(EventWithActor.FromZ);
                Message.AppendRawDouble(EventWithActor.TargetZ);
            }

            return Message;
        }
    }
}
