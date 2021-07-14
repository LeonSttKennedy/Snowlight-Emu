using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    class MimicCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "club_vip"; }
        }

        public string Parameters
        {
            get { return "<username>"; }
        }

        public string Description
        {
            get { return "Allows you to copy the look from another user."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_mimic_info")));
                return;
            }
            else
            {
                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    string Username = Params[1];
                    DataRow Data = null;
                    Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Username));
                    if (TargetSession == null)
                    {
                        MySqlClient.SetParameter("username", Username);
                        Data = MySqlClient.ExecuteQueryRow("SELECT * FROM characters WHERE username LIKE @username LIMIT 1");
                        if (Data == null)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_mimic_error", Username), 4, ChatType.Whisper));
                            return;
                        }
                    }

                    bool MimicPermitted = Data != null ? Data["allow_mimic"].ToString() == "1" : TargetSession.CharacterInfo.AllowMimic;

                    if (MimicPermitted)
                    {
                        Session.CharacterInfo.UpdateFigure(MySqlClient,
                            (Data != null ? (string)Data["gender"] : TargetSession.CharacterInfo.Gender.ToString()),
                            (Data != null ? UserInputFilter.FilterString((string)Data["figure"]) : TargetSession.CharacterInfo.Figure));

                        Session.SendInfoUpdate();
                    }
                    else
                    {
                        Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_mimic_error", Username), 0, ChatType.Whisper));
                    }
                }
            }
        }
    }
}
