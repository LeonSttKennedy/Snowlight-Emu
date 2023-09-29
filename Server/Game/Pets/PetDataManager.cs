using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Snowlight.Game.Items;
using Snowlight.Storage;
using static Snowlight.Game.Pets.PetCommands;

namespace Snowlight.Game.Pets
{
    public static class PetDataManager
    {
        private static Dictionary<int, List<PetRaceData>> mRaces;
        private static Dictionary<int, List<PetTricks>> mTricks;
        private static Dictionary<int, List<PetCommands>> mCommands;
        private static object mSyncRoot;

        public static void Initialize(SqlDatabaseClient MySqlClient)
        {
            mRaces = new Dictionary<int, List<PetRaceData>>();
            mTricks = new Dictionary<int, List<PetTricks>>();
            mCommands = new Dictionary<int, List<PetCommands>>();
            mSyncRoot = new object();

            ReloadData(MySqlClient);
        }

        public static void ReloadData(SqlDatabaseClient MySqlClient)
        {
            mRaces.Clear();
            mTricks.Clear();
            mCommands.Clear();

            Dictionary<int, List<PetRaceData>> NewRaceData = new Dictionary<int, List<PetRaceData>>();
            Dictionary<int, List<PetTricks>> NewTrickData = new Dictionary<int, List<PetTricks>>();
            Dictionary<int, List<PetCommands>> NewCommandsData = new Dictionary<int, List<PetCommands>>();

            DataTable RaceTable = MySqlClient.ExecuteQueryTable("SELECT * FROM catalog_pet_races");

            foreach (DataRow Row in RaceTable.Rows)
            {
                int PetType = (int)Row["pet_type"];

                if (!NewRaceData.ContainsKey(PetType))
                {
                    NewRaceData.Add(PetType, new List<PetRaceData>());
                }

                NewRaceData[PetType].Add(new PetRaceData((int)Row["data1"], (int)Row["data2"], (int)Row["data3"]));
            }

            DataTable TrickTable = MySqlClient.ExecuteQueryTable("SELECT * FROM pet_tricks");

            foreach (DataRow Row in TrickTable.Rows)
            {
                int PetType = (int)Row["type"];

                if (!NewTrickData.ContainsKey(PetType))
                {
                    NewTrickData.Add(PetType, new List<PetTricks>());
                }

                NewTrickData[PetType].Add(new PetTricks((string)Row["trick"], (Row["needs_toy"].ToString() == "1"),
                    (ItemBehavior)(int)Row["toy_behavior_id"]));
            }

            DataTable CommandsTable = MySqlClient.ExecuteQueryTable("SELECT * FROM pet_commands ORDER BY order_id ASC");

            foreach (DataRow Row in CommandsTable.Rows)
            {
                int PetType = (int)Row["type"];

                if (!NewCommandsData.ContainsKey(PetType))
                {
                    NewCommandsData.Add(PetType, new List<PetCommands>());
                }

                int CommandId = int.Parse(Row["command_id"].ToString());

                MySqlClient.SetParameter("commandId", CommandId);
                DataRow SettingRow = MySqlClient.ExecuteQueryRow("SELECT command_text, owner_only FROM pet_command_settings WHERE command_id = @commandId LIMIT 1");

                PetCommands PetCommand = new PetCommands((PetCommand)CommandId, (int)Row["min_level"],
                    (SettingRow[1].ToString() == "1"));

                string[] CommandStringSplitted = SettingRow[0].ToString().Split('|');

                foreach(string CommandString in CommandStringSplitted)
                {
                    PetCommand.AddCommandText(CommandString);
                }

                NewCommandsData[PetType].Add(PetCommand);
            }

            lock (mSyncRoot)
            {
                mRaces = NewRaceData;
                mTricks = NewTrickData;
                mCommands = NewCommandsData;
            }
        }

        public static List<PetRaceData> GetRaceDataForType(int PetType)
        {
            lock (mSyncRoot)
            {
                if (mRaces.ContainsKey(PetType))
                {
                    return mRaces[PetType];
                }
            }

            return new List<PetRaceData>();
        }

        public static PetCommand TryInvokeCommand(Pet PetData, string Command, bool IsOwner)
        {
            PetCommand _Return = PetCommand.None;
            lock (mSyncRoot)
            {
                if (mRaces.ContainsKey(PetData.Type))
                {
                    PetCommands Data = mCommands[PetData.Type].Where(C => C.CommandTexts.Contains(Command) && PetData.Level >= C.MinLevel).FirstOrDefault();
                    
                    _Return = Data != null ? (Data.OnlyOwner && !IsOwner ? _Return : Data.Id) : _Return;
                }
            }

            return _Return;
        }

        public static PetCommands GetCommandData(int PetType, PetCommand Command)
        {
            PetCommands Data = null;
            lock (mSyncRoot)
            {
                if (mRaces.ContainsKey(PetType))
                {
                    Data = mCommands[PetType].Where(C => C.Id.Equals(Command)).FirstOrDefault();
                    return Data;
                }
            }

            return null;
        }

        public static List<PetCommands> GetCommandsForType(int PetType)
        {
            lock (mSyncRoot)
            {
                if (mRaces.ContainsKey(PetType))
                {
                    return mCommands[PetType];
                }
            }

            return new List<PetCommands>();
        }

        public static List<PetTricks> GetTricksForType(int PetType)
        {
            List<PetTricks> Tricks = new List<PetTricks>();

            lock (mSyncRoot)
            {
                if (mTricks.ContainsKey(0))
                {
                    Tricks.AddRange(mTricks[0]);
                }

                if (mTricks.ContainsKey(PetType))
                {
                    Tricks.AddRange(mTricks[PetType]);
                }
            }   

            return Tricks;
        }
    }
}
