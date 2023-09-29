using System;
using System.Linq;
using System.Collections.Generic;

using Snowlight.Game.Pets;

namespace Snowlight.Communication.Outgoing
{
    public static class PetTrainingPanelComposer
    {
        public static ServerMessage Compose(Pet PetData)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.PET_TRAINING_PANEL);
            Message.AppendUInt32(PetData.Id);
            Message.AppendInt32(PetData.CommandList.Count);
            
            foreach (PetCommands Commands in PetData.CommandList)
            {
                Message.AppendInt32((int)Commands.Id);
            }

            Message.AppendInt32(PetData.AvailablePetCommands.Count());
            
            foreach (PetCommands Commands in PetData.AvailablePetCommands)
            {
                Message.AppendInt32((int)Commands.Id);
            }

            return Message;
        }
    }
}