using System;
using Snowlight.Storage;
using System.Collections.Generic;
using System.Data;

using System.Threading;
using Snowlight.Game.Sessions;
using Snowlight.Communication;
using Snowlight.Game.Catalog;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Rooms;
using Snowlight.Game.Items;
using Snowlight.Communication.Incoming;
using Snowlight.Util;

namespace Snowlight.Game.Catalog
{
    public class CatalogClubGifts
    {
        private static List<CatalogClubGifts> mClubGifts;
        private static string mName;
        private static int mBaseItem;
        private static int mDaysNeed;
        private static bool mIsVip;
        private static object mSyncRoot;

        public static List<CatalogClubGifts> ClubGifts
        {
            get
            {
                return mClubGifts;
            }
        }
        public string DisplayName
        {
            get
            {
                return mName;
            }
        }
        public int BaseItem
        { 
            get
            {
                return mBaseItem;
            }
        }
        public int DaysNeed
        {
            get
            {
                return mDaysNeed;
            }
        }
        public bool IsVip
        {
            get
            {
                return mIsVip;
            }
        }
        public CatalogClubGifts(string Name, int BaseItem, int DaysNeed, bool IsVip)
        {
            mName = Name;
            mBaseItem = BaseItem;
            mDaysNeed = DaysNeed;
            mIsVip = IsVip;
        }
        public static void Initialize(SqlDatabaseClient MySqlClient)
        {
            mSyncRoot = new object();
            mClubGifts = new List<CatalogClubGifts>();

            ReloadGifts(MySqlClient);

            DataRouter.RegisterHandler(OpcodesIn.CATALOG_CLUB_GIFT, new ProcessRequestCallback(GetClubGifts));
            DataRouter.RegisterHandler(OpcodesIn.CATALOG_SELECTED_CLUB_GIFT, new ProcessRequestCallback(GetClubGift));
        }
        public static void ReloadGifts(SqlDatabaseClient MySqlClient)
        {
            lock (mSyncRoot)
            {
                DataTable Table = MySqlClient.ExecuteQueryTable("SELECT * FROM catalog_subscriptions_gifts");
                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        ClubGifts.Add(new CatalogClubGifts(
                            (string)Row["item_name"],
                            (int)Row["item_id"],
                            (int)Row["days_need"],
                            (Row["isvip"].ToString() == "1")));
                    }
                }
            }
        }
        private static CatalogClubGifts GetGiftByName(string Name)
        {
            foreach (CatalogClubGifts Gift in ClubGifts)
            {
                if (Gift.DisplayName.ToLower() == Name.ToLower())
                {
                    return Gift;
                }
            }

            return null;
        }
        private static void GetClubGifts(Session Session, ClientMessage Message)
        {
            ServerMessage Response = (CatalogManager.CacheEnabled ? 
                CatalogManager.CacheController.TryGetResponse(Session.CharacterId, Message) : null);
            
            if (Response != null)
            {
                Session.SendData(Response);
            }

            lock (mSyncRoot)
            {
                
                // Not sure how this handled.
                Response = new ServerMessage(OpcodesOut.CATALOG_CLUB_GIFT);
                Response.AppendInt32(0); // Days until next gift.
                Response.AppendInt32(0); // Gifts available.
                
                Response.AppendInt32(1);
                {
                    Response.AppendInt32(207);
                    Response.AppendStringWithBreak("throne");
                    Response.AppendInt32(0);
                    Response.AppendInt32(0);
                    Response.AppendInt32(0);
                    Response.AppendInt32(0);
                    {
                        Response.AppendStringWithBreak("s");
                        Response.AppendInt32(230);
                        Response.AppendStringWithBreak(string.Empty);
                        Response.AppendInt32(0);
                        Response.AppendInt32(0);
                    }
                    Response.AppendInt32(0);
                }

                Response.AppendInt32(1);
                {
                    Response.AppendInt32(230);
                    Response.AppendBoolean(false);  // is item vip
                    Response.AppendInt32(1);        // days to unlock
                    Response.AppendBoolean(false);  // canselect
                }
            }

            if (CatalogManager.CacheEnabled)
            {
                CatalogManager.CacheController.AddIfNeeded(Session.CharacterId, Message, Response);
            }

            Session.SendData(Response);
        }
        private static void GetClubGift(Session Session, ClientMessage Message)
        {
            string ItemName = Message.PopString();

            CatalogClubGifts SelectedItem = GetGiftByName(ItemName);
            ItemDefinition Definition = ItemDefinitionManager.GetDefinition((uint)SelectedItem.BaseItem);
            if (SelectedItem == null || Definition == null)
            {
                return;
            }

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                // Todo ADD one gift time to user subscription table
            }

            ServerMessage ServerMessage = new ServerMessage(OpcodesOut.CATALOG_REEDEM_CLUB_GIFT);
            ServerMessage.AppendStringWithBreak(SelectedItem.DisplayName);
            ServerMessage.AppendBoolean(true);
            ServerMessage.AppendStringWithBreak(Definition.TypeLetter);
            ServerMessage.AppendUInt32(Definition.SpriteId);
            ServerMessage.AppendStringWithBreak("");
            ServerMessage.AppendBoolean(SelectedItem.IsVip);
            ServerMessage.AppendBoolean(false); // canselect :D
            Session.SendData(ServerMessage);
            Session.SendData(InventoryRefreshComposer.Compose());

            // Todo delivery item
        }
    }
}
