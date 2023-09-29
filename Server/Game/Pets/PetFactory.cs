using System;
using System.Data;

using Snowlight.Storage;
using Snowlight.Specialized;

namespace Snowlight.Game.Pets
{
    public static class PetFactory
    {
        public static Pet CreatePet(SqlDatabaseClient MySqlClient, uint UserId, int Type, string Name, int Race, string Color)
        {
            MySqlClient.SetParameter("userid", UserId);
            MySqlClient.SetParameter("type", Type);
            MySqlClient.SetParameter("name", Name);
            MySqlClient.SetParameter("race", Race);
            MySqlClient.SetParameter("color", Color);
            MySqlClient.SetParameter("timestamp", UnixTimestamp.GetCurrent());

            string RawId = MySqlClient.ExecuteScalar("INSERT INTO user_pets (user_id,type,name,race,color,timestamp) VALUES (@userid,@type,@name,@race,@color,@timestamp); SELECT LAST_INSERT_ID();").ToString();

            uint.TryParse(RawId, out uint Id);

            if (Id == 0)
            {
                return null;
            }

            return new Pet(Id, Name, Type, Race, Color, UserId, 0, new Vector3(0, 0, 0), UnixTimestamp.GetCurrent(), 0, 120, 100, 0);
        }

        public static Pet GetPetFromDatabaseRow(DataRow Row)
        {
            return new Pet((uint)Row["id"], (string)Row["name"], (int)Row["type"], (int)Row["race"], (string)Row["color"],
                (uint)Row["user_id"], (uint)Row["room_id"], Vector3.FromString((string)Row["room_pos"]),
                (double)Row["timestamp"], (int)Row["experience"], (int)Row["energy"], (int)Row["happiness"],
                (int)Row["score"]);
        }
    }
}