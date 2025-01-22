using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Items;
using Snowlight.Game.Rooms;
using Snowlight.Game.Rights;
using Snowlight.Communication;
using Snowlight.Game.Sessions;
using Snowlight.Game.Achievements;
using Snowlight.Communication.Incoming;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Catalog
{
    public class SubscriptionOfferManager
    {
        private static Dictionary<uint, SubscriptionOffer> mSubscriptionOffers;

        public static void Initialize(SqlDatabaseClient MySqlClient)
        {
            mSubscriptionOffers = new Dictionary<uint, SubscriptionOffer>();
            
            DataRouter.RegisterHandler(OpcodesIn.PURCHASE_BASIC_CLUB_OFFER, new ProcessRequestCallback(BuyClubOfferSubscription));
            DataRouter.RegisterHandler(OpcodesIn.PURCHASE_VIP_CLUB_OFFER, new ProcessRequestCallback(BuyClubOfferSubscription));

            ReloadSubscriptionOffers(MySqlClient);
        }

        public static void ReloadSubscriptionOffers(SqlDatabaseClient MySqlClient)
        {
            uint CountLoaded = 0;

            lock (mSubscriptionOffers)
            {
                mSubscriptionOffers.Clear();
                MySqlClient.SetParameter("current_timestamp", UnixTimestamp.GetCurrent());
                DataTable SubOffersTable = MySqlClient.ExecuteQueryTable("SELECT * FROM subscription_offers WHERE expire_timestamp > @current_timestamp");

                foreach (DataRow Row in SubOffersTable.Rows)
                {
                    uint OfferId = (uint)Row["id"];

                    string[] SplitedIds = Row["user_ids_list"].ToString().Split('|');

                    List<uint> UserIdList = new List<uint>();

                    foreach (string StringUIds in SplitedIds)
                    {
                        if (uint.TryParse(StringUIds, out uint UserId))
                        {
                            if (!UserIdList.Contains(UserId))
                            {
                                UserIdList.Add(UserId);
                            }
                        }
                    }

                    if (!mSubscriptionOffers.ContainsKey(OfferId))
                    {
                        mSubscriptionOffers.Add(OfferId, new SubscriptionOffer(OfferId, int.Parse(Row["discount_percentage"].ToString()),
                                 UserIdList, double.Parse(Row["expire_timestamp"].ToString()), (uint)Row["offer_id"],
                                 (Row["catalog_enabled"].ToString() == "1"), (Row["basic_club_reminder_expiration"].ToString() == "1"),
                                 (Row["show_extend_notification"].ToString() == "1"), (Row["only_for_user_never_been_club_member"].ToString() == "1")));
                    }

                    CountLoaded++;
                }
            }

            Output.WriteLine("Loaded " + CountLoaded + " club subscription offer(s)", OutputLevel.DebugInformation);

        }

        public static SubscriptionOffer CheckForSubOffer(ClubSubscriptionLevel UserLevel, uint UserId)
        {
            return mSubscriptionOffers.Values.Where(O => O.BaseOffer.Level == UserLevel && !O.UserIds.Contains(UserId) && O.Enabled).ToList().FirstOrDefault();
        }

        public static SubscriptionOffer CheckForSubOfferReminder()
        {
            return mSubscriptionOffers.Values.Where(O => O.BasicSubscriptionReminder && O.Enabled).ToList().FirstOrDefault();
        }

        public static SubscriptionOffer GetOffer(uint OfferId)
        {
            mSubscriptionOffers.TryGetValue(OfferId, out SubscriptionOffer SubOffer);
            return SubOffer;
        }

        public static void BuyClubOfferSubscription(Session Session, ClientMessage Message)
        {
            uint OfferId = Message.PopWiredUInt32();
            SubscriptionOffer SubOffer = GetOffer(OfferId);

            using(SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                if (SubOffer.Price > 0)
                {
                    Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, -SubOffer.Price);
                    Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                }

                double Length = 86400 * SubOffer.BaseOffer.LengthDays;

                // Extend membership
                Session.SubscriptionManager.AddOrExtend((int)SubOffer.BaseOffer.Level, Length);
                
                Session.SubscriptionManager.UpdateUserBadge();

                // Clear catalog cache for user (in case of changes)
                CatalogManager.ClearCacheGroup(Session.CharacterId);

                // Send new data to client
                Session.SendData(FuseRightsListComposer.Compose(Session));
                Session.SendData(SubscriptionStatusComposer.Compose(Session.SubscriptionManager, true));

                if (Session.SubscriptionManager.GiftPoints > 0)
                {
                    Session.SendData(ClubGiftReadyComposer.Compose(Session.SubscriptionManager.GiftPoints));
                }

                SubOffer.UpdateUserIdList(MySqlClient, Session.CharacterId);
            }
        }
    }
}
