using Snowlight.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Snowlight.Util
{
    public static class ExternalTexts
    {
        private static Dictionary<string, string> mDatabaseTexts = new Dictionary<string, string>();

        public static void Clear()
        {
            mDatabaseTexts.Clear();
        }

        public static string GetValue(string Identifier, string Arg)
        {
            return GetValue(Identifier, new string[] { Arg });
        }

        public static string GetValue(string Identifier, string[] Args = null)
        {
            lock (mDatabaseTexts)
            {
                if (mDatabaseTexts == null || !mDatabaseTexts.ContainsKey(Identifier))
                {
                    return Identifier;
                }

                string ReturnValue = mDatabaseTexts[Identifier];
                if (Args != null)
                {
                    for (int i = 0; i < Args.Length; i++)
                    {
                        ReturnValue = ReturnValue.Replace("%" + i + "%", Args[i]);
                    }
                }

                ReturnValue = ReturnValue.Replace("\\n", "\n");
                return ReturnValue;
            }
        }

        public static void Initialize(SqlDatabaseClient MySqlClient)
        {
            int CountLoaded = 0;
            Clear();
            DataTable TextsTable = MySqlClient.ExecuteQueryTable("SELECT identifier, display_text FROM server_ingame_texts ORDER BY identifier ASC");
            
            foreach(DataRow Row in TextsTable.Rows)
            {
                mDatabaseTexts.Add((string)Row["identifier"], (string)Row["display_text"]);
                CountLoaded++;
            }

            Output.WriteLine("Loaded " + CountLoaded + " external texts(s).", OutputLevel.DebugInformation);
        }
    }
}
