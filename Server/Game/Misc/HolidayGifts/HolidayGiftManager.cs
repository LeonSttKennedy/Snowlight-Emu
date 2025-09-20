using Snowlight.Storage;
using Snowlight.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Game.Misc
{
    public static class HolidayGiftManager
    {
        private static Dictionary<uint, HolidayGift> mHolidayGifts;

        public static bool HasGiftToday
        {
            get
            {
                return TodayGiftList().Count > 0;
            }
        }

        public static HolidayGift TodayGift
        {
            get
            {
                return TodayGiftList().FirstOrDefault();
            }
        }


        public static void Initialize(SqlDatabaseClient MySqlClient)
        {
            uint CountLoaded = 0;

            mHolidayGifts = new Dictionary<uint, HolidayGift>();

            MySqlClient.SetParameter("enabled", "1");
            DataTable HolidayGiftTable = MySqlClient.ExecuteQueryTable("SELECT * FROM holiday_gifts WHERE enabled = @enabled");

            foreach (DataRow Row in HolidayGiftTable.Rows)
            {
                HolidayGift Gift = new HolidayGift((uint)Row["id"], (string)Row["base_id"], (string)Row["amounts"], (int)Row["credits_amount"],
                    (int)Row["activitypoints_amount"], SeasonalCurrency.FromStringToEnum(Row["seasonal_currency"].ToString()), (string)Row["badge_code"],
                    (string)Row["delivery_date"], (string)Row["gift_message"], (string)Row["gifts_required"], (string)Row["user_alert_message"]);

                if(!mHolidayGifts.ContainsKey(Gift.Id))
                {
                    mHolidayGifts.Add(Gift.Id, Gift);
                }

                CountLoaded++;
            }

            Output.WriteLine("Loaded " + CountLoaded + " holiday gift(s)", OutputLevel.DebugInformation);
        }

        public static List<HolidayGift> TodayGiftList()
        {
            return mHolidayGifts.Values.Where(G => G.DeliveryDate.ToString("dd-MM-yyyy") == DateTime.Now.ToString("dd-MM-yyyy")).ToList();
        }
    }
}
