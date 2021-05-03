using System;

namespace Snowlight.Communication.Outgoing
{
    public static class PetTrainingPanelComposer
    {
        public static ServerMessage Compose(uint PetId, int PetLevel, int i = 0)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.PET_TRAINING_PANEL);
            Message.AppendUInt32(PetId);
            Message.AppendInt32(18); //commands count
            Message.AppendInt32(0); //free
            Message.AppendInt32(1); //sit
            Message.AppendInt32(2); //down
            Message.AppendInt32(3); //here
            Message.AppendInt32(4); //beg
            Message.AppendInt32(17); //play football
            Message.AppendInt32(5); //play dead
            Message.AppendInt32(6); //stay
            Message.AppendInt32(7); //follow
            Message.AppendInt32(8); //stand
            Message.AppendInt32(9); //jump
            Message.AppendInt32(10); //speak
            Message.AppendInt32(11); //play
            Message.AppendInt32(12); //silent
            Message.AppendInt32(13); //nest
            Message.AppendInt32(14); //drink
            Message.AppendInt32(15); //follow left
            Message.AppendInt32(16); //follow right
            while (PetLevel > i)
            {
                i++;
            }
            Message.AppendInt32((i == 1? i + 1 : i));
            Message.AppendInt32(0); //free
            Message.AppendInt32(1); //sit
            Message.AppendInt32(2); //down
            Message.AppendInt32(3); //here
            Message.AppendInt32(4); //beg
            Message.AppendInt32(17); //play football
            Message.AppendInt32(5); //play dead
            Message.AppendInt32(6); //stay
            Message.AppendInt32(7); //follow
            Message.AppendInt32(8); //stand
            Message.AppendInt32(9); //jump
            Message.AppendInt32(10); //speak
            Message.AppendInt32(11); //play
            Message.AppendInt32(12); //silent
            Message.AppendInt32(13); //nest
            Message.AppendInt32(14); //drink
            Message.AppendInt32(15); //follow left
            Message.AppendInt32(16); //follow right
            return Message;
        }
    }
}