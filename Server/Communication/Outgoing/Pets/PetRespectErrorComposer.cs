using System;

using Snowlight.Util;

namespace Snowlight.Communication.Outgoing
{
    public static class PetRespectErrorComposer
    {
        public static ServerMessage Compose(int AccountDays)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.PET_SCRATCHING_ERROR);
            Message.AppendInt32(ServerSettings.PetScratchingAccountDaysOld);
            Message.AppendInt32(AccountDays);
            return Message;
        }
    }
}
