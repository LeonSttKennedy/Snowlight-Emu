﻿using System;
using System.Collections.Generic;
using Snowlight.Game.Characters;
using Snowlight.Game.Messenger;
using Snowlight.Game.Sessions;
using Snowlight.Storage;

namespace Snowlight.Communication.Outgoing
{
    public class MessengerUpdate
    {
        private uint mOriginalCharId;
        private int mMode;
        private CharacterInfo mCharacterInfo;

        public uint OriginalCharId
        {
            get
            {
                return mOriginalCharId;
            }
        }

        public int Mode
        {
            get
            {
                return mMode;
            }
        }

        public CharacterInfo CharacterInfo
        {
            get
            {
                return mCharacterInfo;
            }
        }

        public MessengerUpdate(uint OriginalCharId, int Mode, CharacterInfo CharacterInfo)
        {
            mOriginalCharId = OriginalCharId;
            mMode = Mode;
            mCharacterInfo = CharacterInfo;
        }
    }

    public static class MessengerUpdateListComposer
    {
        public static ServerMessage Compose(List<MessengerCategories> Categories, List<MessengerUpdate> Updates)
        {
            // @MHIIjhiuZSirBamItsLee...IIIhr-155-45.hd-180-1.ch-235-91.lg-270-91.sh-305-62.wa-2007Htree
            ServerMessage Message = new ServerMessage(OpcodesOut.MESSENGER_UPDATES);

            Message.AppendInt32(Categories.Count);
            foreach (MessengerCategories Cat in Categories)
            {
                Message.AppendInt32(Cat.CatId);
                Message.AppendStringWithBreak(Cat.CatName);
            }

            Message.AppendInt32(Updates.Count);
            if (Updates.Count > 0)
            {
                foreach (MessengerUpdate Update in Updates)
                {
                    Message.AppendInt32(Update.Mode);
                    Message.AppendUInt32(Update.CharacterInfo.Id);

                    if (Update.Mode != -1)
                    {
                        Session LinkedSession = SessionManager.GetSessionByCharacterId(Update.CharacterInfo.Id);

                        int CategoryId = (int)MessengerHandler.GetFriendshipCategoryId(Update.OriginalCharId, Update.CharacterInfo.Id);

                        Message.AppendStringWithBreak(Update.CharacterInfo.Username);
                        Message.AppendBoolean(true);
                        Message.AppendBoolean(Update.CharacterInfo.HasLinkedSession);
                        Message.AppendBoolean(LinkedSession != null && LinkedSession.InRoom);
                        Message.AppendStringWithBreak(Update.CharacterInfo.HasLinkedSession ? Update.CharacterInfo.Figure : string.Empty);
                        Message.AppendInt32(CategoryId);
                        Message.AppendStringWithBreak(Update.CharacterInfo.HasLinkedSession ? Update.CharacterInfo.Motto : string.Empty);

                        if (Update.CharacterInfo.HasLinkedSession)
                        {
                            Message.AppendStringWithBreak(string.Empty);
                        }
                        else
                        {
                            DateTime LastOnline = UnixTimestamp.GetDateTimeFromUnixTimestamp(Update.CharacterInfo.TimestampLastOnline);
                            Message.AppendStringWithBreak(LastOnline.ToShortDateString() + " " + LastOnline.ToShortTimeString());
                        }

                        Message.AppendStringWithBreak(Update.CharacterInfo.RealName);
                        Message.AppendStringWithBreak(string.Empty);
                    }
                }
            }

            return Message;
        }
    }
}
