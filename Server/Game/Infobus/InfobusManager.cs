using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Snowlight.Game.Rooms;
using Snowlight.Communication;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Incoming;
using Snowlight.Communication.Outgoing;
using Snowlight.Util;
using System.Collections.ObjectModel;

namespace Snowlight.Game.Infobus
{
    public static class InfobusManager
    {
        private static Dictionary<uint, InfobusQuestion> mInfobusQuestions;
        private static List<string> mInfobusPoolOptions;

        public static List<string> InfobusPoolOptions
        {
            get
            {
                return mInfobusPoolOptions;
            }

            set
            {
                mInfobusPoolOptions = value;
            }
        }

        public static void Initialize()
        {
            mInfobusQuestions = new Dictionary<uint, InfobusQuestion>();
            mInfobusPoolOptions = new List<string>();

            DataRouter.RegisterHandler(OpcodesIn.INFOBUS_ENTER, new ProcessRequestCallback(EnterInfobus));
            DataRouter.RegisterHandler(OpcodesIn.INFOBUS_SUBMIT_ANSWER, new ProcessRequestCallback(SubmitAnswer));
        }

        public static void StartPoll(uint RoomId, string Question, List<string> Answers)
        {
            lock (mInfobusQuestions)
            {
                if (mInfobusQuestions.ContainsKey(RoomId))
                {
                    if (!mInfobusQuestions[RoomId].Completed)
                    {
                        mInfobusQuestions[RoomId].EndQuestion();
                    }

                    mInfobusQuestions.Remove(RoomId);
                }

                RoomInstance Instance = RoomManager.GetInstanceByRoomId(RoomId);

                if (Instance == null)
                {
                    return;
                }

                mInfobusQuestions.Add(RoomId, new InfobusQuestion(Instance, Question, Answers));
            }
        }

        private static void EnterInfobus(Session Session, ClientMessage Message)
        {
            if (ServerSettings.InfobusStatus == InfobusStatus.Closed)
            {
                Session.SendData(InfobusClosedComposer.Compose(ExternalTexts.GetValue("infobus_closed_message")));
            }
        }
        private static void SubmitAnswer(Session Session, ClientMessage Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);

            if (Instance == null)
            {
                return;
            }

            RoomActor Actor = Instance.GetActorByReferenceId(Session.CharacterId);

            if (Actor == null)
            {
                return;
            }

            int AnswerId = Message.PopWiredInt32();

            lock (mInfobusQuestions)
            {
                if (mInfobusQuestions.ContainsKey(Instance.RoomId))
                {
                    mInfobusQuestions[Instance.RoomId].SubmitAnswer(Actor.Id, AnswerId);
                }
            }
        }
    }
}
