using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Snowlight.Communication.Outgoing;
using Snowlight.Util;


namespace Snowlight.Game.Bots
{
    public class BotRandomSpeech
    {
        private string mMessage;
        private ChatType mMessageMode;

        public string Message
        {
            get
            {
                return mMessage;
            }
        }

        public ChatType MessageMode
        {
            get
            {
                return mMessageMode;
            }
        }

        public BotRandomSpeech(string Message, ChatType MessageMode)
        {
            mMessage = Message;
            mMessageMode = MessageMode;
        }
    }
}
