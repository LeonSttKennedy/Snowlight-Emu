using System;

using Snowlight.Storage;

namespace Snowlight.Game.AvatarEffects
{
    public static class AvatarEffectFactory
    {
        public static AvatarEffect CreateEffect(SqlDatabaseClient MySqlClient, uint UserId, int SpriteId, double Duration)
        {
            MySqlClient.SetParameter("userid", UserId);
            MySqlClient.SetParameter("spriteid", SpriteId);
            MySqlClient.SetParameter("duration", Duration);

            string RawId = MySqlClient.ExecuteScalar("INSERT INTO avatar_effects (user_id,sprite_id,duration) VALUES (@userid,@spriteid,@duration); SELECT LAST_INSERT_ID();").ToString();

            uint.TryParse(RawId, out uint Id);

            if (Id == 0)
            {
                return null;
            }

            return new AvatarEffect(Id, SpriteId, Duration, 1, false, 0.0);
        }
    }
}
