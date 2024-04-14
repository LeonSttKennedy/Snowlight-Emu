using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Rooms;
using Snowlight.Game.Infobus;
using Snowlight.Game.Sessions;
using Snowlight.Communication;
using Snowlight.Game.Advertisements;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    class InfobusCommand : IChatCommand
    {
        private string mInfobusPoolQuestion;
        public string InfobusPoolQuestion
        {
            get { return mInfobusPoolQuestion; }
        }

        public string PermissionRequired
        {
            get { return "moderation_tool"; }
        }

        public string Parameters
        {
            get { return "<help>"; }
        }

        public string Description
        {
            get { return "Infobus command."; }
        }
        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if(Params.Length == 1)
            {
                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_infobus_info")));
                return;
            }

            string InfobusCommand = UserInputFilter.FilterString(Params[1].Trim());

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                switch (InfobusCommand.ToLower())
                {
                    case "poll":
                        {
                            if(InfobusPoolQuestion != string.Empty && InfobusPoolQuestion != null)
                            {
                                if(InfobusManager.InfobusPoolOptions != null && InfobusManager.InfobusPoolOptions.Count > 0)
                                {
                                    InfobusManager.StartPoll(Session.CurrentRoomId, InfobusPoolQuestion, InfobusManager.InfobusPoolOptions);
                                }
                                else
                                {
                                    Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_infobus_pool_option_error"), 0, ChatType.Whisper));
                                    return;
                                }
                            }
                            else
                            {
                                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_infobus_pool_question_error"), 0, ChatType.Whisper));
                                return;
                            }

                            return;
                        }

                    case "add":
                        {
                            string Add = UserInputFilter.FilterString(Params[2].Trim());
                            switch(Add.ToLower())
                            {
                                case "option":
                                    {
                                        string Option = UserInputFilter.FilterString(CommandManager.MergeParams(Params, 3));
                                        if (Option != string.Empty)
                                        {
                                            InfobusManager.InfobusPoolOptions.Add(Option);
                                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_infobus_option_added", Option), 0, ChatType.Whisper));
                                        }
                                        else
                                        {
                                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_infobus_add_option_empty", Option), 0, ChatType.Whisper));
                                        }
                                        return;
                                    }

                                case "question":
                                    {
                                        string Question = UserInputFilter.FilterString(CommandManager.MergeParams(Params, 3));
                                        if (Question != string.Empty)
                                        {
                                            mInfobusPoolQuestion = Question;
                                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_infobus_question_set", mInfobusPoolQuestion), 0, ChatType.Whisper));
                                        }
                                        else
                                        {
                                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_infobus_add_question_empty", mInfobusPoolQuestion), 0, ChatType.Whisper));
                                        }
                                        return;
                                    }

                                default:
                                    {
                                        Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_infobus_add_info"), 0, ChatType.Whisper));
                                        return;
                                    }
                            }
                        }

                    case "reset":
                        {
                            if (InfobusManager.InfobusPoolOptions != null)
                            {
                                InfobusManager.InfobusPoolOptions.Clear();
                            }
                            mInfobusPoolQuestion = null;
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_infobus_reset"), 0, ChatType.Whisper));
                            return;
                        }

                    case "status":
                        {
                            RoomInfo InfobusRoom = RoomInfoLoader.GetRoomInfo(17);
                            string Question = InfobusPoolQuestion != null ? InfobusPoolQuestion : ExternalTexts.GetValue("command_infobus_question_empty");
                            string Options = InfobusManager.InfobusPoolOptions.Count > 0 ? string.Join(", ", InfobusManager.InfobusPoolOptions) : ExternalTexts.GetValue("command_infobus_options_empty");
                            Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_infobus_status", new string[] { InfobusRoom.CurrentUsers.ToString(), Question, Options, ServerSettings.InfobusStatus.ToString() })));
                            return;
                        }

                    case "open":
                        {
                            ServerSettings.InfobusStatus = InfobusStatus.Open;
                            ServerSettings.UpdateInfobusStatus(MySqlClient);
                            ServerSettings.Initialize(MySqlClient);

                            RoomInstance ParkRoomInstance = RoomManager.GetInstanceByModelName("park_a");

                            if (ParkRoomInstance != null)
                            {
                                ParkRoomInstance.BroadcastMessage(ParkInfobusDoorComposer.Compose((int)ServerSettings.InfobusStatus));
                                ParkRoomInstance.BroadcastMessage(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_infobus_opened")));
                            }

                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_infobus_open_successyfully"), 0, ChatType.Whisper));
                            return;
                        }

                    case "close":
                        {
                            ServerSettings.InfobusStatus = InfobusStatus.Closed;
                            ServerSettings.UpdateInfobusStatus(MySqlClient);
                            ServerSettings.Initialize(MySqlClient);
                            RoomInstance ParkRoomInstance = RoomManager.GetInstanceByModelName("park_a");

                            if (ParkRoomInstance != null)
                            {
                                ParkRoomInstance.BroadcastMessage(ParkInfobusDoorComposer.Compose((int)ServerSettings.InfobusStatus));
                            }

                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_infobus_close_successyfully"), 0, ChatType.Whisper));
                            return;
                        }

                    case "help":
                    default:
                        {
                            Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_infobus_info")));
                            return;
                        }
                }
            }
        }
    }
}
