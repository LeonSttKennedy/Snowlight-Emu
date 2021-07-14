using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    class FollowCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return ""; }
        }

        public string Parameters
        {
            get { return "<username>"; }
        }

        public string Description
        {
            get { return "Allows you to goes to in specificied room that user in."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Params.Length < 2)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_invalid_syntax") + " - :follow <username>", 0, ChatType.Whisper));
                return;
            }

            string Username = UserInputFilter.FilterString(Params[1].Trim());

            Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Username));
            if (TargetSession == null)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_follow_targetuser_error", Username), 0, ChatType.Whisper));
                return;
            }

            if (TargetSession.CharacterInfo.Username == Session.CharacterInfo.Username)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_follow_targetuser_same_name", Username), 0, ChatType.Whisper));
                return;
            }

            if (TargetSession.CurrentRoomId == Session.CurrentRoomId)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_follow_targetuser_same_room", Username), 0, ChatType.Whisper));
                return;
            }

            RoomInstance TargetRoom = RoomManager.GetInstanceByRoomId(TargetSession.CurrentRoomId);
            if (TargetRoom == null)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_follow_room_error", Username), 0, ChatType.Whisper));
                return;
            }

            if (TargetRoom.Info.AccessType != RoomAccessType.Open && !Session.HasRight("enter_locked_rooms"))
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_follow_room_locked", Username), 0, ChatType.Whisper));
                return;
            }

            Session.SendData(MessengerFollowResultComposer.Compose(TargetRoom.Info));
        }
    }
}
