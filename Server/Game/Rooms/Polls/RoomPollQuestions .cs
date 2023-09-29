using Snowlight.Storage;
using Snowlight.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Game.Rooms
{
    public enum PollQuestionResponseType
    {
        Radio = 1,
        CheckBox = 2,
        TextBox = 3
    }

    public class RoomPollQuestions
    {
        private uint mId;
        private uint mPollId;
        private string mPollQuestion;
        private PollQuestionResponseType mResponseType;
        private int mMinimumSelection;
        private List<string> mPollAnswers;

        public uint Id
        {
            get
            {
                return mId;
            }
        }
        public uint PollId
        {
            get
            {
                return mPollId;
            }
        }
        public string Question
        {
            get
            {
                return mPollQuestion;
            }
        }
        public PollQuestionResponseType ResponseType
        {
            get
            {
                return mResponseType;
            }
        }
        public int MinimumSelection
        {
            get
            {
                return mMinimumSelection;
            }
        }
        public ReadOnlyCollection<string> Answers
        {
            get
            {
                List<string> Copy = new List<string>();
                Copy.AddRange(mPollAnswers);
                return Copy.AsReadOnly();
            }
        }

        public RoomPollQuestions(uint Id, uint PollId, string PollQuestion, PollQuestionResponseType PollResponseType,
            int MinimumSelection)
        {
            mId = Id;
            mPollId = PollId;
            mPollQuestion = PollQuestion;
            mResponseType = PollResponseType;
            mMinimumSelection = MinimumSelection;
            mPollAnswers = new List<string>();

            if(mResponseType != PollQuestionResponseType.TextBox)
            {
                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    MySqlClient.SetParameter("pollid", mId);
                    DataTable Table = MySqlClient.ExecuteQueryTable("SELECT selection_text FROM room_poll_question_selections WHERE question_id = @pollid ORDER BY id");
                    foreach(DataRow Row in Table.Rows)
                    {
                        if (!mPollAnswers.Contains((string)Row[0]))
                        {
                            mPollAnswers.Add((string)Row[0]);
                        }
                    }
                }
            }
        }

        public static RoomPollQuestions SetByRow(DataRow Row)
        {
            PollQuestionResponseType Type;
            switch ((string)Row["question_type"].ToString().ToLower())
            {
                case "radio":
                    Type = PollQuestionResponseType.Radio;
                    break;

                case "checkbox":
                    Type = PollQuestionResponseType.CheckBox;
                    break;

                default:
                case "textbox":
                    Type = PollQuestionResponseType.TextBox;
                    break;
            }

            return new RoomPollQuestions((uint)Row["id"], (uint)Row["poll_id"], (string)Row["question"],
                Type, (int)Row["minimum_selection"]);
        }
    }
}
