using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;
using Snowlight.Util;

namespace Snowlight.Game.Misc
{
    class GiveCommand : IChatCommand
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
            get { return "Gives your hand item to a another habbo."; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            if(Params.Length < 2)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_giveitem_info"), 0, ChatType.Whisper));
                return;
            }

            string Username = Params[1];

            Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Username));
            if(TargetSession == null)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_giveitem_user_not_found", Username), 0, ChatType.Whisper));
                return;
            }

            RoomActor TargetActor = Instance.GetActorByReferenceId(TargetSession.CharacterId);
            if(TargetActor == null)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_giveitem_user_not_found", Username), 0, ChatType.Whisper));
                return;
            }

            if(Session.CurrentRoomId != TargetSession.CurrentRoomId)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_giveitem_user_not_found_in_room"), 0, ChatType.Whisper));
                return;
            }

            if(Actor.CarryItemId <= 0)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_giveitem_carrying_nothing"), 0, ChatType.Whisper));
                return;
            }

            if (Distance.Calculate(Actor.Position.GetVector2(), TargetActor.Position.GetVector2()) < 3)
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_giveitem_success"), 0, ChatType.Whisper));
                TargetActor.CarryItem(Actor.CarryItemId, true);
                Actor.CarryItem(0, true);
                return;
            }
            else
            {
                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_giveitem_distance_error"), 0, ChatType.Whisper));
                return;
            }
        }
    }
}
