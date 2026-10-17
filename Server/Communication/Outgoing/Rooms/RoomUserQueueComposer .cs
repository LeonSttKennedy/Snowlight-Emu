using Snowlight.Game.Sessions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Snowlight.Communication.Outgoing
{
    public enum QueueType
    {
        Spectator = 1,
        Visitor = 2
    }

    public static class RoomUserQueueComposer
    {
        public static ServerMessage Compose(int QueuesCount, string QueueList, int QueueType, string Username, int Position, int OpositeQuestType = 0)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.ROOM_USER_QUEUE);
            Message.AppendInt32(QueuesCount);                   // User queue count

            Message.AppendStringWithBreak(QueueList);           // List type (C = Club User | D = Default User)
            Message.AppendInt32(QueueType);                     // List target id (Spectator or visitor)

            Message.AppendInt32(1);                             // User in list count

            Message.AppendStringWithBreak(Username);            // Character username
            Message.AppendInt32(Position);                      // Queue position

            if (OpositeQuestType > 0)
            {
                Message.AppendStringWithBreak(QueueList);       // List type (C = Club User | D = Default User)
                Message.AppendInt32(OpositeQuestType);          // List target id (Spectator or visitor)

                Message.AppendUInt32(0);                        // User in list count
            }
            return Message;
        }
    }
}
