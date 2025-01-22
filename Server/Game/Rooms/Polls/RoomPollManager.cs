using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

using Snowlight.Storage;
using Snowlight.Communication;
using Snowlight.Game.Sessions;
using Snowlight.Game.Advertisements;
using Snowlight.Communication.Incoming;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Items;
using Snowlight.Util;
using Snowlight.Game.Rights;

namespace Snowlight.Game.Rooms
{
    public static class RoomPollManager
    {
        private static Dictionary<uint, RoomPoll> mRoomPolls;

        public static void Initialize(SqlDatabaseClient MySqlClient)
        {
            mRoomPolls = new Dictionary<uint, RoomPoll>();

            ReloadRoomPolls(MySqlClient);

            DataRouter.RegisterHandler(OpcodesIn.ROOM_POLL_GET_QUESTIONS, new ProcessRequestCallback(GetRoomPollQuestions));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_POLL_CANCEL, new ProcessRequestCallback(RoomPollCancel));
            DataRouter.RegisterHandler(OpcodesIn.ROOM_POLL_PROCESS_ANSWER, new ProcessRequestCallback(ProcessRoomPollAnswer));
        }

        public static void ReloadRoomPolls(SqlDatabaseClient MySqlClient)
        {
            int CountLoaded = 0;

            lock (mRoomPolls)
            {
                mRoomPolls.Clear();
                Dictionary<uint, Dictionary<uint, RoomPollQuestions>> QuestionsByPollId = new Dictionary<uint, Dictionary<uint, RoomPollQuestions>>();

                DataTable TableQuestions = MySqlClient.ExecuteQueryTable("SELECT * FROM room_poll_questions");

                foreach (DataRow Row in TableQuestions.Rows)
                {
                    RoomPollQuestions Question = RoomPollQuestions.SetByRow(Row);

                    if (!QuestionsByPollId.ContainsKey(Question.PollId))
                    {
                        QuestionsByPollId[Question.PollId] = new Dictionary<uint, RoomPollQuestions>();
                    }

                    QuestionsByPollId[Question.PollId].Add(Question.Id, Question);
                }

                DataTable TablePolls = MySqlClient.ExecuteQueryTable("SELECT * FROM room_polls WHERE room_id > -1 AND (expire_timestamp < 1 or expire_timestamp >= UNIX_TIMESTAMP()) ORDER BY id");
                foreach (DataRow Row in TablePolls.Rows)
                {
                    RoomPoll Poll = RoomPoll.SetByRow(Row);
                    if (QuestionsByPollId.ContainsKey(Poll.Id))
                    {
                        Poll.SetPollQuestions(QuestionsByPollId[Poll.Id]);
                    }

                    mRoomPolls.Add(Poll.RoomId, Poll);
                    CountLoaded++;
                }
            }

            Output.WriteLine("Loaded " + CountLoaded + " room poll(s).", OutputLevel.DebugInformation);
        }

        public static RoomPoll GetRoomPoll(uint RoomId)
        {
            mRoomPolls.TryGetValue(RoomId, out RoomPoll Poll);
            return Poll;
        }

        #region Handlers
        public static void GetRoomPollQuestions(Session Session, ClientMessage Message)
        {
            uint RoomId = Message.PopWiredUInt32();
            RoomPoll Poll = GetRoomPoll(RoomId);

            if (Poll == null || Session.CharacterInfo.FilledPolls.Contains(Poll.Id))
            {
                return;
            }

            Session.SendData(RoomPollQuestionsComposer.Compose(Poll));
        }
        public static void RoomPollCancel(Session Session, ClientMessage Message)
        {
            uint RoomId = Message.PopWiredUInt32();
            RoomPoll Poll = GetRoomPoll(RoomId);

            if(Poll == null || Session.CharacterInfo.FilledPolls.Contains(Poll.Id))
            {
                return;
            }

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                Session.CharacterInfo.SetPollFilled(Poll.Id);
                Session.CharacterInfo.SetAnswer(MySqlClient, Poll.Id);
            }
        }
        public static void ProcessRoomPollAnswer(Session Session, ClientMessage Message)
        {
            uint RoomId = Message.PopWiredUInt32();
            RoomPoll Poll = GetRoomPoll(RoomId);

            if (Poll == null || Session.CharacterInfo.FilledPolls.Contains(Poll.Id))
            {
                return;
            }

            uint QuestionId = Message.PopWiredUInt32();
            Poll.Questions.TryGetValue(QuestionId, out RoomPollQuestions Question);

            if (Question == null)
            {
                return;
            }

            int AnswerCount = Message.PopWiredInt32();

            if (AnswerCount < 1 ||
                (Question.ResponseType != PollQuestionResponseType.CheckBox && AnswerCount > 1) ||
                (Question.ResponseType == PollQuestionResponseType.CheckBox &&
                (AnswerCount < Question.MinimumSelection || AnswerCount > Question.Answers.Count)))
            {
                return;
            }

            List<string> Results = new List<string>();

            for (int i = 0; i < AnswerCount; i++)
            {
                string AnswerData = Message.PopString();

                if (Question.ResponseType != PollQuestionResponseType.TextBox)
                {
                    int AnswerIndex = int.Parse(AnswerData) - 1;
                    string SelectedIndex = Question.Answers[AnswerIndex];

                    if (Results.Contains(SelectedIndex))
                    {
                        continue;
                    }

                    Results.Add(SelectedIndex);
                }
                else
                {
                    if (AnswerData.Length > 256)
                    {
                        AnswerData = AnswerData.Substring(0, 256);
                    }

                    Results.Add(AnswerData.Replace(";", ""));
                }
            }

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                if (Poll.IsLastQuestion(Question.Id))
                {
                    Session.CharacterInfo.SetPollFilled(Poll.Id);

                    #region Give Credits
                    if (Poll.CreditsRewardValue > 0)
                    {
                        Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, Poll.CreditsRewardValue);
                        Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                    }
                    #endregion

                    #region Give Activity Points
                    if (Poll.ActivityPointsRewardValue > 0)
                    {
                        Session.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, Poll.ActivityPointsType, Poll.ActivityPointsRewardValue);
                        if (Poll.ActivityPointsType == SeasonalCurrencyList.Pixels)
                        {
                            Session.SendData(UpdatePixelsBalanceComposer.Compose(Session.CharacterInfo.ActivityPoints[0], Poll.ActivityPointsRewardValue));
                        }
                        Session.SendData(UserActivityPointsBalanceComposer.Compose(Session.CharacterInfo.ActivityPoints));
                    }
                    #endregion

                    #region Give Furniture
                    if (Poll.FurniReward.Count > 0)
                    {
                        Dictionary<int, List<uint>> NotifyItems = new Dictionary<int, List<uint>>();

                        foreach (uint ItemId in Poll.FurniReward)
                        {
                            Item Item = ItemFactory.CreateItem(MySqlClient, ItemId, Session.CharacterId, string.Empty,
                                string.Empty, 0, false);

                            if (Item != null)
                            {
                                int NotifyTabId = Item.Definition.Type == ItemType.WallItem ? 2 : 1;

                                Session.InventoryCache.Add(Item);
                                Session.NewItemsCache.MarkNewItem(MySqlClient, NotifyTabId, Item.Id);

                                if (!NotifyItems.ContainsKey(NotifyTabId))
                                {
                                    NotifyItems.Add(NotifyTabId, new List<uint>());
                                }

                                NotifyItems[NotifyTabId].Add(Item.Id);
                            }
                        }

                        if (NotifyItems.Count > 0)
                        {
                            Session.SendData(InventoryRefreshComposer.Compose());
                            Session.SendData(InventoryNewItemsComposer.Compose(new Dictionary<int, List<uint>>(NotifyItems)));
                        }
                    }
                    #endregion

                    #region Give Badge
                    if (Poll.BadgeReward != string.Empty)
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

                        BadgeDefinition BadgeToGive = RightsManager.GetBadgeDefinitionByCode(Poll.BadgeReward);
                        if (BadgeToGive == null)
                        {
                            return;
                        }

                        if (!Session.BadgeCache.ContainsCode(Poll.BadgeReward))
                        {
                            Session.BadgeCache.UpdateAchievementBadge(MySqlClient, BadgeToGive.Code, BadgeToGive, Session.AchievementCache,"static");

                            InventoryBadge UserBadge = Session.BadgeCache.GetBadge(Poll.BadgeReward);
                            Session.NewItemsCache.MarkNewItem(MySqlClient, 4, UserBadge.Id);
                            Session.NewItemsCache.SendNewItems(Session);
                        }
                    }
                    #endregion
                }

                int QID = int.Parse(Question.Id.ToString());
                Session.CharacterInfo.SetAnswer(MySqlClient, Poll.Id, QID, string.Join("|", Results));
            }
        }
        #endregion
    }
}
