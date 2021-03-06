using System;

using Snowlight.Game.AvatarEffects;

namespace Snowlight.Communication.Outgoing
{
    public static class UserEffectAddedComposer
    {
        public static ServerMessage Compose(AvatarEffect Effect)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.USER_EFFECT_ADDED);
            Message.AppendInt32(Effect.SpriteId);
            Message.AppendInt32((int)Effect.Duration);
            return Message;
        }
    }
}
