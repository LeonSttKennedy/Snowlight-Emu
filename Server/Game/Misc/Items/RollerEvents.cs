using System;
using System.Linq;
using System.Collections.Generic;

using Snowlight.Specialized;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    public class RollerEvents
    {
        private double mFromZ;
        private double mTargetZ;
        private uint mActorId;
        private uint mItemId;
        private MovementType mMovementType;

        public double FromZ
        {
            get
            {
                return mFromZ;
            }
        }

        public double TargetZ
        {
            get
            {
                return mTargetZ;
            }
        }

        public uint ActorId
        {
            get
            {
                return mActorId;
            }
        }

        public uint ItemId
        {
            get
            {
                return mItemId;
            }
        }

        public MovementType MovementType
        {
            get
            {
                return mMovementType;
            }
        }

        public RollerEvents(double FromZ, double TargetZ, uint ActorId, uint ItemId, MovementType Type = MovementType.None)
        {
            mFromZ = FromZ;
            mTargetZ = TargetZ;
            mActorId = ActorId;
            mItemId = ItemId;
            mMovementType = Type;
        }
    }
}
