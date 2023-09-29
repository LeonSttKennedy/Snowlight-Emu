using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Snowlight.Util
{
    public enum SeasonalCurrencyList
    {
        Pixels = 0,
        Snowflakes = 1,
        Hearts = 2,
        Giftpoints = 3,
        Shells = 4,
        Diamonds = 5,
    }

    public static class SeasonalCurrency
    {
        public static Dictionary<int, int> ActivityPointsToDictionary(string ActivityPoints) //Tries parse activity points data as best as it can, even bit corrupted/malformed
        {
            Dictionary<int, int> ActivityPointsDictionary = new Dictionary<int, int>();

            string[] SplitAP = ActivityPoints.Split('|');

            foreach (string APData in SplitAP)
            {
                if (!string.IsNullOrEmpty(APData))
                {
                    string[] APData_ = APData.Split(',');

                    if (APData_.Length == 2)
                    {
                        int ActivityPointsId;
                        int ActivityPointsAmount;
                        if (int.TryParse(APData_[0], out ActivityPointsId) && int.TryParse(APData_[1], out ActivityPointsAmount))
                        {
                            ActivityPointsDictionary.Add(ActivityPointsId, ActivityPointsAmount);
                        }
                    }
                }
            }

            return ActivityPointsDictionary;
        }

        public static string ActivityPointsToString(Dictionary<int, int> ActivityPoints)
        {
            List<string> ActivityPointsString = new List<string>();
            foreach (KeyValuePair<int, int> UserData in ActivityPoints)
            {
                ActivityPointsString.Add(UserData.Key + "," + UserData.Value);
            }
            return string.Join("|", ActivityPointsString);
        }

        public static SeasonalCurrencyList FromStringToEnum(string SeasonalCurrency)
        {
            switch (SeasonalCurrency.ToLower())
            {
                default:
                case "pixel":

                    return SeasonalCurrencyList.Pixels;

                case "snowflakes":

                    return SeasonalCurrencyList.Snowflakes;

                case "hearts":

                    return SeasonalCurrencyList.Hearts;

                case "giftpoints":

                    return SeasonalCurrencyList.Giftpoints;

                case "shells":

                    return SeasonalCurrencyList.Shells;

                case "diamonds":

                    return SeasonalCurrencyList.Diamonds;
            }
        }
    }
}
