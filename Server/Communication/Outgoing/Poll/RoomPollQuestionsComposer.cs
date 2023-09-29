using Snowlight.Game.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Communication.Outgoing
{
    public static class RoomPollQuestionsComposer
    {
        public static ServerMessage Compose(RoomPoll Poll)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.ROOM_POLL_QUESTIONS);
            Message.AppendUInt32(Poll.RoomId);
            Message.AppendStringWithBreak(Poll.Title);
            Message.AppendStringWithBreak(Poll.ThanksMessage);
            int num = 0;
            Message.AppendInt32(Poll.Questions.Count);
            foreach (RoomPollQuestions questions in Poll.Questions.Values)
            {
                Message.AppendUInt32(questions.Id);
                Message.AppendInt32(++num);
                Message.AppendInt32((int)questions.ResponseType);
                Message.AppendStringWithBreak(questions.Question);
                if (questions.ResponseType != PollQuestionResponseType.TextBox)
                {
                    Message.AppendInt32(questions.Answers.Count);
                    Message.AppendInt32(questions.MinimumSelection);
                    Message.AppendInt32(questions.Answers.Count);
                    foreach (string str in questions.Answers)
                    {
                        Message.AppendStringWithBreak(str);
                    }
                }
            }
            return Message;

        }
    }
}
