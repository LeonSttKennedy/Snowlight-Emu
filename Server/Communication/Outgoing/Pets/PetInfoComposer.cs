﻿using System;

using Snowlight.Game.Pets;

namespace Snowlight.Communication.Outgoing
{
    public static class PetInfoComposer
    {
        public static ServerMessage Compose(Pet Pet)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.ROOM_PET_INFO);
            Message.AppendUInt32(Pet.Id);
            Message.AppendStringWithBreak(Pet.Name);
            Message.AppendInt32(Pet.Level); // Cur lvl
            Message.AppendInt32(Pet.TotalLevels); // Tar(MAX) lvl
            Message.AppendInt32(Pet.Experience); // Cur Exp??
            Message.AppendInt32(Pet.ExperienceTarget); // Tar Exp??
            Message.AppendInt32(Pet.Energy); // Cur Energy??
            Message.AppendInt32(Pet.EnergyLimit); // Tar Energy??
            Message.AppendInt32(Pet.Happiness); // Nutrition
            Message.AppendInt32(Pet.HappinessLimit); // Max nutrition
            /*
             * Message.AppendInt32(Pet.Type); // Look (type)??
             * Message.AppendInt32(Pet.Race); // Breed??
            */
            Message.AppendStringWithBreak(Pet.Look.ToLower()); // Color
            Message.AppendInt32(Pet.Score); // Respect?
            Message.AppendUInt32(Pet.OwnerId);
            Message.AppendInt32(Pet.AgeDays);
            Message.AppendStringWithBreak(Pet.OwnerName);
            return Message;
        }
    }
}
