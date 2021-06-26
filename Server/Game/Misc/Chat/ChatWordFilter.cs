using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Sessions;
using Snowlight.Game.Moderation;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    public static class ChatWordFilter
    {
        private static List<string> mWords;
        
        private static object mSyncRoot;

        public static List<string> BlockedWords
        {
            get
            {
                return mWords;
            }
        }

        public static void Initialize(SqlDatabaseClient MySqlClient)
        {
            mSyncRoot = new object();
            mWords = new List<string>();

            lock (mSyncRoot)
            {
                DataTable Table = MySqlClient.ExecuteQueryTable("SELECT word FROM wordfilter");
                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        BlockedWords.Add((string)Row["word"]);
                    }
                }
            }
        }
        public static string CheckWords(string Message, Session Session)
        {
            for (int i = 0; i < BlockedWords.Count; i++)
            {
                string Word = BlockedWords[i];

                if (Word.Length < 0 || Word == "" || Word == null)
                {
                    continue;
                }

                if (Message.ToLower().Contains(Word.ToLower()))
                {
                    HandleInfraction(Message, Session);

                    Message = Message.Replace(Word, ServerSettings.WordFilterReplacementWord);
                }
            }

            return Message;
        }

        public static void HandleInfraction(string Word, Session Session)
        {
            int Index = 0;
            List<string> WordList = new List<string>();

            if (Session.CharacterInfo.InFraction.ContainsKey(Word))
            {
                Session.CharacterInfo.InFraction[Word]++;
            }
            else
            {
                Session.CharacterInfo.InFraction.Add(Word, 1);
            }

            foreach (string CheckedWords in Session.CharacterInfo.InFraction.Keys)
            {
                Index += Session.CharacterInfo.InFraction[CheckedWords];
                WordList.Add((Session.CharacterInfo.InFraction[CheckedWords] > 1 ? Session.CharacterInfo.InFraction[CheckedWords] + "x " + CheckedWords : CheckedWords));
            }

            if (Index >= ServerSettings.WordFilterMaximumCount)
            {
                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    string ActionDetail = string.Concat(new object[] 
                    {
                        "User '" + Session.CharacterInfo.Username + "' (ID " + Session.CharacterId + ")",
                        " ",
                        "was muted automatically for " + (int)Math.Round((double)ServerSettings.WordFilterTimeToMute / 60, 1) + " min",
                        " ",
                        "by sayng offensive words",
                        " ",
                        "(Words spoken: " + string.Join(", ", WordList) + ")."
                    }); ;

                    Session.CharacterInfo.Mute(MySqlClient, ServerSettings.WordFilterTimeToMute);
                    Session.SendData(NotificationMessageComposer.Compose("You have been muted by scolding."));
                    ModerationLogs.LogModerationAction(MySqlClient, Session, "Muted automatically", ActionDetail);

                    // Clear params;
                    Session.CharacterInfo.InFraction.Clear();
                    WordList.Clear();
                    Index = 0;
                }

            }
        }
    }
}
