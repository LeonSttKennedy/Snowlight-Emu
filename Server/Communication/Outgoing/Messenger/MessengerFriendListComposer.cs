using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Snowlight.Game.Sessions;
using Snowlight.Util;
using Snowlight.Game.Characters;
using Snowlight.Storage;
using Snowlight.Game.Messenger;

namespace Snowlight.Communication.Outgoing
{
    public class MessengerCategories
    {
        private uint mCatId;
        private string mCatName;

        public uint CatId
        {
            get
            {
                return mCatId;
            }
        }
        public string CatName
        {
            get
            {
                return mCatName;
            }
        }
        public MessengerCategories(uint Id, string Label)
        {
            mCatId = Id;
            mCatName = Label;
        }
    }
    public static class MessengerFriendListComposer
    {
        public static ServerMessage Compose(uint CharacterId, ReadOnlyCollection<uint> Friends, ReadOnlyCollection<MessengerCategories> Categories)
        { 
            // @LXKAXKAXVBXaCHIkXuzd0zoNeXxIHHH01-01-2011 13:18:32Alex BrookerPYQA
            ServerMessage Message = new ServerMessage(OpcodesOut.MESSENGER_FRIENDS_LIST);
            Message.AppendInt32(300);
            Message.AppendInt32(300);
            Message.AppendInt32(800);
            Message.AppendInt32(1100);
            
            Message.AppendInt32(Categories.Count);
            foreach(MessengerCategories Category in Categories)
            {
                Message.AppendUInt32(Category.CatId);
                Message.AppendStringWithBreak(Category.CatName);
            }

            Message.AppendInt32(Friends.Count);

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                foreach (uint FriendId in Friends)
                {
                    CharacterInfo Info = CharacterInfoLoader.GetCharacterInfo(MySqlClient, FriendId);

                    if (Info == null)
                    {
                        continue;
                    }

                    Session Session = SessionManager.GetSessionByCharacterId(Info.Id);

                    int CategoryId = (int)MessengerHandler.GetFriendshipCategoryId(MySqlClient, CharacterId, Info.Id);

                    Message.AppendUInt32(Info.Id);
                    Message.AppendStringWithBreak(Info.Username);
                    Message.AppendBoolean(true);
                    Message.AppendBoolean(Info.HasLinkedSession);
                    Message.AppendBoolean(Session != null && Session.InRoom);
                    Message.AppendStringWithBreak(Info.HasLinkedSession ? Info.Figure : string.Empty);
                    Message.AppendInt32(CategoryId);
                    Message.AppendStringWithBreak(Info.HasLinkedSession ? Info.Motto : string.Empty);

                    if (Info.HasLinkedSession)
                    {
                        Message.AppendStringWithBreak(string.Empty);
                    }
                    else
                    {
                        DateTime LastOnline = UnixTimestamp.GetDateTimeFromUnixTimestamp(Info.TimestampLastOnline);
                        Message.AppendStringWithBreak(LastOnline.ToShortDateString() + " " + LastOnline.ToShortTimeString());
                    }

                    Message.AppendStringWithBreak(Info.RealName);
                    Message.AppendStringWithBreak(string.Empty); // Unknown - since RELEASE63-33530-33497
                }
            }

            return Message;
        }
    }
}
