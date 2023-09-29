using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Snowlight.Storage;
using System.Collections.ObjectModel;

using Snowlight.Specialized;
using Snowlight.Game.Pets;
using Snowlight.Util;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Rooms;

namespace Snowlight.Game.Bots
{
    public static class BotManager
    {
        private static Dictionary<uint, Bot> mBotDefinitions;
        private static Dictionary<uint, List<BotResponse>> mDefinedResponses;
        private static Dictionary<uint, List<BotRandomSpeech>> mDefinedSpeech;
        private static Dictionary<int, Bot> mPetHandlerIndex;
        private static object mSyncRoot;
        
        public static void Initialize(SqlDatabaseClient MySqlClient)
        {
            mBotDefinitions = new Dictionary<uint, Bot>();
            mDefinedResponses = new Dictionary<uint, List<BotResponse>>();
            mDefinedSpeech = new Dictionary<uint, List<BotRandomSpeech>>();
            mPetHandlerIndex = new Dictionary<int, Bot>();
            mSyncRoot = new object();

            LoadBotDefinitions(MySqlClient);
        }

        public static void LoadBotDefinitions(SqlDatabaseClient MySqlClient)
        {           
            mBotDefinitions.Clear();
            mDefinedResponses.Clear();
            mDefinedSpeech.Clear();
            mPetHandlerIndex.Clear();

            DataTable ResponseTable = MySqlClient.ExecuteQueryTable("SELECT bot_id,triggers,responses,response_mode,response_serve_id FROM bot_responses");
                
            foreach (DataRow Row in ResponseTable.Rows)
            {
                ChatType Type;
                switch ((string)Row["response_mode"])
                {
                    default:
                    case "say":

                        Type = ChatType.Say;
                        break;

                    case "shout":

                        Type = ChatType.Shout;
                        break;

                    case "whisper":

                        Type = ChatType.Whisper;
                        break;
                }

                BotResponse Response = new BotResponse(Row["triggers"].ToString().Split('|').ToList(),
                    Row["responses"].ToString().Split('|').ToList(), Type, (int)Row["response_serve_id"]);
                         
                if (!mDefinedResponses.ContainsKey((uint)Row["bot_id"]))
                {
                    mDefinedResponses.Add((uint)Row["bot_id"], new List<BotResponse>());
                }

                mDefinedResponses[(uint)Row["bot_id"]].Add(Response);
            }

            DataTable SpeechTable = MySqlClient.ExecuteQueryTable("SELECT bot_id,message,speech_mode FROM bots_speech");

            foreach (DataRow Row in SpeechTable.Rows)
            {
                ChatType Type;
                switch ((string)Row["speech_mode"])
                {
                    default:
                    case "say":

                        Type = ChatType.Say;
                        break;

                    case "shout":

                        Type = ChatType.Shout;
                        break;
                
                }

                if (!mDefinedSpeech.ContainsKey((uint)Row["bot_id"]))
                {
                    mDefinedSpeech.Add((uint)Row["bot_id"], new List<BotRandomSpeech>());
                }

                BotRandomSpeech RandomSpeech = new BotRandomSpeech((string)Row["message"], Type);

                mDefinedSpeech[(uint)Row["bot_id"]].Add(RandomSpeech);
            }

            MySqlClient.SetParameter("enabled", "1");
            DataTable BotTable = MySqlClient.ExecuteQueryTable("SELECT * FROM bots WHERE enabled = @enabled");

            foreach (DataRow Row in BotTable.Rows)
            {
                BotWalkMode WMode = BotWalkMode.STAND;

                switch ((string)Row["walk_mode"])
                {
                    case "freeroam":

                        WMode = BotWalkMode.FREEROAM;
                        break;

                    case "defined":

                        WMode = BotWalkMode.SPECIFIED_RANGE;
                        break;
                }

                List<Vector2> DefinedPositions = new List<Vector2>();
                string[] DefPosBits = Row["pos_defined_range"].ToString().Split(';');

                foreach (string DefPosBit in DefPosBits)
                {
                    DefinedPositions.Add(Vector2.FromString(DefPosBit));
                }

                Bot Bot = new Bot((uint)Row["id"], (uint)Row["id"], (string)Row["ai_type"], (string)Row["name"],
                    (string)Row["look"], (string)Row["motto"], (uint)Row["room_id"],
                    Vector3.FromString((string)Row["pos_start"]), Vector2.FromString((string)Row["pos_serve"]),
                    DefinedPositions, WMode, (Row["kickable"].ToString() == "1"), (int)Row["rotation"],
                    (mDefinedResponses.ContainsKey((uint)Row["id"]) ? mDefinedResponses[(uint)Row["id"]] : new List<BotResponse>()),
                    (int)Row["effect"], (int)Row["response_distance"]);

                mBotDefinitions.Add((uint)Row["id"], Bot);

                int PetHandler = (int)Row["pet_type_handler_id"];

                if (PetHandler >= 0)
                {
                    mPetHandlerIndex.Add(PetHandler, Bot);
                }
            }
        }

        public static Bot CreateNewInstance(Bot Definition, uint RoomId, Vector3 Position, Pet PetData = null)
        {
            return new Bot(Definition.Id, Definition.Id, Definition.BehaviorType, Definition.Name,
                Definition.Look, Definition.Motto, RoomId, Position, Definition.ServePosition,
                Definition.PredefinedPositions.ToList(), Definition.WalkMode, Definition.Kickable,
                Definition.Rotation, Definition.Responses, Definition.Effect, Definition.ResponseDistance,
                PetData);
        }

        public static ReadOnlyCollection<Bot> GenerateBotInstancesForRoom(uint RoomId)
        {
            List<Bot> NewInstances = new List<Bot>();

            lock (mSyncRoot)
            {
                foreach (Bot BotDef in mBotDefinitions.Values)
                {
                    if (BotDef.RoomId == RoomId)
                    {
                        NewInstances.Add(CreateNewInstance(BotDef, RoomId, BotDef.InitialPosition));
                    }
                }
            }

            return NewInstances.AsReadOnly();           
        }

        public static Bot GetBotByBehavior(string BehaviorType)
        {
            lock (mSyncRoot)
            { 
                foreach(Bot BotDef in mBotDefinitions.Values)
                {
                    if(BotDef.BehaviorType == BehaviorType)
                    {
                        return BotDef;
                    }
                }
            }
            
            return null;
        }
        public static Bot GetBotDefinition(uint DefinitionId)
        {
            if (mBotDefinitions.ContainsKey(DefinitionId))
            {
                return mBotDefinitions[DefinitionId];
            }   

            return null;
        }

        public static BotRandomSpeech GetRandomSpeechForBotDefinition(uint DefinitionId)
        {
            if (!mDefinedSpeech.ContainsKey(DefinitionId))
            {
                return null;
            }

            return mDefinedSpeech[DefinitionId][RandomGenerator.GetNext(0, mDefinedSpeech[DefinitionId].Count - 1)];
        }

        public static Bot GetHandlerDefinitionForPetType(int PetType)
        {
            return mPetHandlerIndex.ContainsKey(PetType) ? mPetHandlerIndex[PetType] : null;
        }
    }
}
