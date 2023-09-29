using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    class AfkCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "club_regular"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Put your eyes to sleep."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Actor.IsSleeping)
            {
                Actor.Unidle();
            }
            else
            {
                Actor.UpdateSleepingState();
                Instance.BroadcastMessage(RoomUserSleepComposer.Compose(Actor.Id, Actor.IsSleeping));
            }
        }
    }
}
