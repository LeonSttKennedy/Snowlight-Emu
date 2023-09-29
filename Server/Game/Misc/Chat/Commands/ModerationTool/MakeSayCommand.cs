using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Game.Moderation;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Characters;

namespace Snowlight.Game.Misc
{
    class MakeSayCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "moderation_tool"; }
        }

        public string Parameters
        {
            get { return "<username> <message>"; }
        }

        public string Description
        {
            get { return "Makes another user says a message."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if (Params.Length <= 2)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_make_say_error"), 0, ChatType.Whisper));
                return;
            }

            string TargetUser = Params[1];
            Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(TargetUser));

            if(TargetSession == null)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_make_say_useroff_error"), 0, ChatType.Whisper));
                return;
            }

            RoomActor TargetActor = Instance.GetActorByReferenceId(TargetSession.CharacterId);

            if (TargetSession.CurrentRoomId != Session.CurrentRoomId 
                || TargetActor == null)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_make_say_room_error"), 0, ChatType.Whisper));
                return;
            }

            string isShoutStartMessage = "*shout*";
            string[] Message = CommandManager.MergeParams(Params, 2).Split('|');

            bool Shout = Message[0].StartsWith(isShoutStartMessage);
            int EmoteId = 0;
            int MessageTextId = 0;

            if (Message.Length == 3)
            {
                Shout = (Message[0].Equals("yes") || Message[0].Equals("1") || Message[0].Equals(isShoutStartMessage));
                EmoteId = int.Parse(Message[1]);
                MessageTextId = 2;
            }

            string CorrectedMessage = Message[MessageTextId].StartsWith(isShoutStartMessage) ? Message[MessageTextId].Substring(isShoutStartMessage.Length) : Message[MessageTextId];
            
            Instance.BroadcastChatMessage(TargetActor, CorrectedMessage, Shout, EmoteId);
        }
    }
}
