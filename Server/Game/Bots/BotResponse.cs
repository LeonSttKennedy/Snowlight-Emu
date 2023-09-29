using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Snowlight.Communication.Outgoing;
using Snowlight.Util;


namespace Snowlight.Game.Bots
{
    public class BotResponse
    {
        private List<string> mTriggers;
        private List<string> mResponse;
        private ChatType mResponseMode;
        private int mResponseServeId;

        public ChatType ResponseMode
        {
            get
            {
                return mResponseMode;
            }
        }

        public int ResponseServeId
        {
            get
            {
                return mResponseServeId;
            }
        }

        public BotResponse(List<string> Triggers, List<string> Responses, ChatType ResponseMode, int ResponseServeId)
        {
            mTriggers = Triggers;
            mResponse = Responses;
            mResponseMode = ResponseMode;
            mResponseServeId = ResponseServeId;
        }

        public bool MatchesTrigger(string UserQuery)
        {
            UserQuery = UserQuery.ToLower();

            foreach (string Trigger in mTriggers)
            {
                if (UserQuery.Contains(Trigger.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

        public string GetResponse()
        {
            if (mResponse.Count < 1)
            {
                return null;
            }

            return mResponse[RandomGenerator.GetNext(0, (mResponse.Count - 1))];
        }
    }
}
