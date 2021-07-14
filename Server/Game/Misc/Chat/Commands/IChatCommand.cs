using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;

namespace Snowlight.Game.Misc
{
    public interface IChatCommand
    {
        string PermissionRequired { get; }
        string Parameters { get; }
        string Description { get; }
        void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params);
    }
}
