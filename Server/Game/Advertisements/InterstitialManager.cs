using System;
using System.Linq;
using System.Data;
using System.Collections.Generic;

using Snowlight.Storage;

namespace Snowlight.Game.Advertisements
{
    public static class InterstitialManager
    {
        private static Dictionary<uint, Interstitial> mInterstitials;
        private static Dictionary<uint, Interstitial> mRoomAds;

        public static void Initialize(SqlDatabaseClient MySqlClient)
        {
            mInterstitials = new Dictionary<uint, Interstitial>();
            mRoomAds = new Dictionary<uint, Interstitial>();

            ReloadInterstitials(MySqlClient);
            ReloadRoomAds(MySqlClient);
        }

        public static void ReloadInterstitials(SqlDatabaseClient MySqlClient)
        {
            int CountLoaded = 0;

            lock (mInterstitials)
            {
                mInterstitials.Clear();

                DataTable Table = MySqlClient.ExecuteQueryTable("SELECT * FROM interstitials WHERE enabled = '1'");

                foreach (DataRow Row in Table.Rows)
                {
                    mInterstitials.Add((uint)Row["id"], new Interstitial((uint)Row["id"], (string)Row["url"],
                        (string)Row["image"]));

                    CountLoaded++;
                }
            }

            Output.WriteLine("Loaded " + CountLoaded + " room interstitial(s).", OutputLevel.DebugInformation);
        }

        public static void ReloadRoomAds(SqlDatabaseClient MySqlClient)
        {
            int CountLoaded = 0;

            lock (mRoomAds)
            {
                mRoomAds.Clear();

                DataTable Table = MySqlClient.ExecuteQueryTable("SELECT * FROM room_ads WHERE enabled = '1'");

                foreach (DataRow Row in Table.Rows)
                {
                    mRoomAds.Add((uint)Row["room_id"], new Interstitial((uint)Row["id"], (string)Row["url"], (string)Row["image"]));
                    CountLoaded++;
                }
            }

            Output.WriteLine("Loaded " + CountLoaded + " room advertisement(s).", OutputLevel.DebugInformation);
        }

        public static Interstitial GetRandomInterstitial(bool IncrecementViews)
        {
            if (mInterstitials.Count == 0)
            {
                return null;
            }

            Interstitial Interstitial = mInterstitials.ElementAt(new Random().Next(0, mInterstitials.Count)).Value;

            if (IncrecementViews)
            {
                Interstitial.IncrecementViews();
            }

            return Interstitial;
        }

        public static Interstitial GetRoomAdsForRoomId(uint RoomId)
        {
            return mRoomAds.ContainsKey(RoomId) ? mRoomAds[RoomId] : null;
        }
    }
}