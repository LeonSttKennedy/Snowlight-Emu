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
        private int mCatId;
        private string mCatName;

        public int CatId
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
        public MessengerCategories(int Id, string Label)
        {
            mCatId = Id;
            mCatName = Label;
        }
    }
    public static class MessengerFriendListComposer
    {
        public static ServerMessage Compose(Session Session, ReadOnlyCollection<uint> Friends, ReadOnlyCollection<MessengerCategories> Categories)
        { 
            // @LXKAXKAXVBXaCHIkXuzd0zoNeXxIHHH01-01-2011 13:18:32Alex BrookerPYQA
            ServerMessage Message = new ServerMessage(OpcodesOut.MESSENGER_FRIENDS_LIST);
            Message.AppendInt32(Session.FriendListSizeLimit);               // Session Friend Limit
            Message.AppendInt32(ServerSettings.NormalUserFriendListSize);   // Normal User Friend Limit
            Message.AppendInt32(ServerSettings.HcUserFriendListSize);       // Hc User Friend Limit
            Message.AppendInt32(ServerSettings.VipUserFriendListSize);      // Vip User Friend Limit
            
            Message.AppendInt32(Categories.Count);
            foreach(MessengerCategories Category in Categories)
            {
                Message.AppendInt32(Category.CatId);
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

                    Session TargetSession = SessionManager.GetSessionByCharacterId(Info.Id);

                    int CategoryId = (int)MessengerHandler.GetFriendshipCategoryId(Session.CharacterId, Info.Id);

                    Message.AppendUInt32(Info.Id);
                    Message.AppendStringWithBreak(Info.Username);
                    Message.AppendBoolean(true);
                    Message.AppendBoolean(Info.HasLinkedSession);
                    Message.AppendBoolean(TargetSession != null && TargetSession.InRoom);
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
