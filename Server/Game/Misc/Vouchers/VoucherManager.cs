using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using Snowlight.Storage;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Items;
using Snowlight.Specialized;
using Snowlight.Util;
using System.Web.UI.WebControls;

namespace Snowlight.Game.Misc
{
    public static class VoucherManager
    {
        private static object mSyncRoot = new object();

        private static Dictionary<string, List<uint>> mVouchersReedem = new Dictionary<string, List<uint>>();

        private static readonly List<char> mNotAllowedChars = new List<char>() { 'i', 'l', 'o', 'w', 'I', 'L', 'O', 'W', ' ' } ;

        public static void Initialize(SqlDatabaseClient MySqlClient)
        {
            DataTable VoucherTable = MySqlClient.ExecuteQueryTable("SELECT * FROM vouchers WHERE enabled = '1' AND uses > 0");
            foreach (DataRow Row in VoucherTable.Rows)
            {
                string Code = Row["code"].ToString();

                if(!mVouchersReedem.ContainsKey(Code))
                {
                    mVouchersReedem.Add(Code, new List<uint>());
                }

                string UserIdList = Row["user_id_list"].ToString();

                foreach (string Uid in UserIdList.Split('|'))
                {
                    if(Uid == string.Empty)
                    {
                        continue;
                    }

                    mVouchersReedem[Code].Add(uint.Parse(Uid));
                }
            }
        }

        public static bool TryRedeemVoucher(SqlDatabaseClient MySqlClient, Session Session, string Code)
        {
            lock (mSyncRoot)
            {
                VoucherValueData ValueData = GetVoucherValue(Code);

                if (ValueData == null || CheckUserOnUsedList(Code, Session.CharacterId) || !ValueData.CanReedemInCatalog)
                {
                    return false;
                }

                if (ValueData.ValueCredits > 0)
                {
                    Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, ValueData.ValueCredits);
                    Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                }

                if (ValueData.ValueActivityPoints > 0)
                {
                    Session.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, ValueData.SeasonalCurrency, ValueData.ValueActivityPoints);
                    if (ValueData.SeasonalCurrency == SeasonalCurrencyList.Pixels)
                    {
                        Session.SendData(UpdatePixelsBalanceComposer.Compose(Session.CharacterInfo.ActivityPoints[0], ValueData.ValueActivityPoints));
                    }

                    Session.SendData(UserActivityPointsBalanceComposer.Compose(Session.CharacterInfo.ActivityPoints));
                }

                if (ValueData.ValueFurni.Count > 0)
                {
                    foreach (uint ItemId in ValueData.ValueFurni)
                    {
                        Item Item = ItemFactory.CreateItem(MySqlClient, ItemId, Session.CharacterId, string.Empty,
                            string.Empty, 0, false);

                        if (Item != null)
                        {
                            int NotifyTabId = Item.Definition.Type == ItemType.WallItem ? 2 : 1;

                            Session.InventoryCache.Add(Item);
                            Session.NewItemsCache.MarkNewItem(MySqlClient, (NewItemsCategory)NotifyTabId, Item.Id);
                        }
                    }

                    Session.SendData(InventoryRefreshComposer.Compose());
                    Session.NewItemsCache.SendNewItems(Session);
                }

                MarkVoucherUsed(Code, Session.CharacterId);
                return true;
            }
        }

        public static VoucherValueData GetVoucherValue(string Code)
        {
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                MySqlClient.SetParameter("code", Code);
                DataRow Row = MySqlClient.ExecuteQueryRow("SELECT value_credits,value_activity_points,seasonal_currency,value_furni,can_reedem_in_catalog FROM vouchers WHERE code = @code AND enabled = '1' AND uses > 0 LIMIT 1");

                if (Row == null)
                {
                    return null;
                }

                List<uint> FurniValue = new List<uint>();

                foreach (string FurniValueBit in Row["value_furni"].ToString().Split(','))
                {
                    uint.TryParse(FurniValueBit, out uint NewValue);

                    if (NewValue > 0)
                    {
                        FurniValue.Add(NewValue);
                    }
                }

                return new VoucherValueData((int)Row["value_credits"], (int)Row["value_activity_points"],
                    SeasonalCurrency.FromStringToEnum(Row["seasonal_currency"].ToString()), FurniValue,
                    ((string)Row["can_reedem_in_catalog"] == "1"));
            }
        }

        public static void MarkVoucherUsed(string Code, uint UserId)
        {
            if (!mVouchersReedem.ContainsKey(Code))
            {
                mVouchersReedem.Add(Code, new List<uint>());
            }

            mVouchersReedem[Code].Add(UserId);

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                MySqlClient.SetParameter("code", Code);
                MySqlClient.SetParameter("userlist", string.Join("|", mVouchersReedem[Code]));
                MySqlClient.ExecuteNonQuery("UPDATE vouchers SET uses = uses - 1, user_id_list = @userlist WHERE code = @code LIMIT 1; DELETE FROM vouchers WHERE uses < 1;");
            }
        }

        public static bool CheckUserOnUsedList(string Code, uint UserId)
        {
            return mVouchersReedem.ContainsKey(Code) && mVouchersReedem[Code].Contains(UserId);
        }

        public static RedeemError ErrorChecker(string Code)
        {
            VoucherValueData Data = GetVoucherValue(Code);
            if (Data != null && !Data.CanReedemInCatalog)
            {
                return RedeemError.ReedemHabboWeb;
            }

            for (int i = 0; i < Code.Length; i++)
            {
                if (mNotAllowedChars.Contains(Code[i]))
                {
                    return RedeemError.TechnicalError;
                }
            }

            return RedeemError.InvalidCode;
        }
    }
}
