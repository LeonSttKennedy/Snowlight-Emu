using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    class SitCommand : IChatCommand
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
            get { return "Allows you to sit down in your current spot."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            List<RoomActor> ActorsToUpdate = new List<RoomActor>();
            if (!Actor.UserStatusses.ContainsKey("sit"))
            {
                if ((Actor.BodyRotation % 2) == 0)
                {
                    Actor.SetStatus("sit", Math.Round((Actor.Position.Z + 1.0) / 2.0 - Actor.Position.Z * 0.5, 1).ToString().Replace(',', '.'));
                    Actor.IsSitting = true;
                    Actor.UpdateNeeded = true;
                }
                else
                {
                    Actor.BodyRotation--;
                    Actor.SetStatus("sit", Math.Round((Actor.Position.Z + 1.0) / 2.0 - Actor.Position.Z * 0.5, 1).ToString().Replace(',', '.'));
                    Actor.IsSitting = true;
                    Actor.UpdateNeeded = true;
                }

                ActorsToUpdate.Add(Actor);
            }
            else
            {
                Actor.RemoveStatus("sit");
                Actor.IsSitting = false;
                Actor.UpdateNeeded = true;

                ActorsToUpdate.Add(Actor);
            }

            if (ActorsToUpdate.Count > 0)
            {
                Instance.BroadcastMessage(RoomUserStatusListComposer.Compose(ActorsToUpdate));
                ActorsToUpdate.Clear();
            }
        }
    }
}
