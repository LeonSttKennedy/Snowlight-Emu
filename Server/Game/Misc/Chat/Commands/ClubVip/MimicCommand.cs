using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Game.Characters;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    class MimicCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get
            { 
                return "club_vip";
            }
        }

        public string Parameters
        {
            get
            { 
                return "<username>";
            }
        }

        public string Description
        {
            get
            {
                return ExternalTexts.GetValue("command_mimic_description");
            }
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

                    CharacterInfo Info = CharacterInfoLoader.GetCharacterInfo(MySqlClient, CharacterResolverCache.GetUidFromName(Username));

                    if (Info.AllowMimic)
                    {
                        Session.CharacterInfo.UpdateFigure(MySqlClient, Info.Gender.ToString(), Info.Figure);

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
