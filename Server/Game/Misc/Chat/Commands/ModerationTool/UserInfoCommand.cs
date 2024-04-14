using Snowlight.Communication.Outgoing;
using Snowlight.Game.Characters;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Storage;
using Snowlight.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Game.Misc
{
    class UserInfoCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "moderation_tool";  }
        }

        public string Parameters
        {
            get { return "<username>";  }
        }

        public string Description
        {
            get { return "View another users profile information.";  }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_userinfo_info"), 0, ChatType.Whisper));
                return;
            }

            string Username = UserInputFilter.FilterString(Params[1].Trim());

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                CharacterInfo Info = CharacterInfoLoader.GetCharacterInfo(MySqlClient, CharacterResolverCache.GetUidFromName(Username));

                if (Info == null)
                {
                    Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_userinfo_not_found", Username), 0, ChatType.Whisper));
                    return;
                }

                List<string> ActivityPoints = new List<string>();

                if (Info.ActivityPoints != null)
                {
                    string Currency = string.Empty;
                    foreach(KeyValuePair<int, int> APs in Info.ActivityPoints)
                    {
                        switch(APs.Key)
                        {
                            case 0:

                                Currency = Info.ActivityPoints[0] > 0 ? "Pixels: " + Info.ActivityPoints[0] : string.Empty;
                                break;

                            case 1:

                                Currency = Info.ActivityPoints[1] > 0 ? "Snowflakes: " + Info.ActivityPoints[1] : string.Empty;
                                break;

                            case 2:

                                Currency = Info.ActivityPoints[2] > 0 ? "Hearts: " + Info.ActivityPoints[2] : string.Empty;
                                break;

                            case 3:

                                Currency = Info.ActivityPoints[3] > 0 ? "Gift Points: " + Info.ActivityPoints[3] : string.Empty;
                                break;

                            case 4:

                                Currency = Info.ActivityPoints[4] > 0 ? "Shells: " + Info.ActivityPoints[4] : string.Empty;
                                break;

                            case 5:

                                Currency = Info.ActivityPoints[5] > 0 ? "Diamonds: " + Info.ActivityPoints[5] : string.Empty;
                                break;
                        }

                        if (Currency != string.Empty)
                        {
                            ActivityPoints.Add(Currency);
                        }
                    }
                }

                string UserText = ExternalTexts.GetValue("command_userinfo_information",
                    new string[] { Username, Info.Id.ToString(), Info.HasLinkedSession.ToString(),
                    Info.Motto, Info.Gender.ToString(), Info.CreditsBalance.ToString(), string.Join("\n", ActivityPoints),
                    Info.MarketplaceTokens.ToString(), Info.GetClubGiftsCount().ToString(), Info.IsMuted.ToString(),
                    Info.GetRoomCount().ToString()});

                string RoomText = string.Empty;

                if (Info.HasLinkedSession)
                {
                    Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Username));

                    if (TargetSession.CurrentRoomId != 0)
                    {
                        RoomInfo RoomInfo = RoomInfoLoader.GetRoomInfo(TargetSession.CurrentRoomId, true);

                        string RoomAccess = string.Empty;
                        switch (RoomInfo.AccessType)
                        {
                            default:
                            case RoomAccessType.Open:

                                RoomAccess = "Open";
                                break;

                            case RoomAccessType.Locked:

                                RoomAccess = "Doorbell";
                                break;

                            case RoomAccessType.PasswordProtected:

                                RoomAccess = "Password";
                                break;
                        }

                        RoomText = ExternalTexts.GetValue("command_userinfo_roominformation",
                            new string[] { RoomInfo.Id.ToString(), RoomInfo.Name,
                                (RoomInfo.Type == RoomType.Public ? ExternalTexts.GetValue("command_userinfo_roominformation_publicroom") : RoomInfo.OwnerName),
                                RoomInfo.CurrentUsers.ToString(), RoomInfo.MaxUsers.ToString(), RoomAccess });
                    }
                }

                Session.SendData(MessageOfTheDayComposer.Compose(UserText + RoomText));
            }
        }
    }
}
