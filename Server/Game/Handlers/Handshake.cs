using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Snowlight.Util;
using Snowlight.Game.Misc;
using Snowlight.Game.Catalog;
using Snowlight.Game.Sessions;
using Snowlight.Communication;
using Snowlight.Game.Characters;
using Snowlight.Communication.Outgoing;
using Snowlight.Communication.Incoming;

namespace Snowlight.Game.Handlers
{
    public static class Handshake
    {
        public static void Initialize()
        {
            DataRouter.RegisterHandler(OpcodesIn.INIT_CRYPTO, new ProcessRequestCallback(InitCrypto), true);
            DataRouter.RegisterHandler(OpcodesIn.USER_SSO_LOGIN, new ProcessRequestCallback(SsoLogin), true);
            DataRouter.RegisterHandler(OpcodesIn.USER_GET_INFO, new ProcessRequestCallback(GetUserInfo));
            DataRouter.RegisterHandler(OpcodesIn.USER_GET_BALANCE, new ProcessRequestCallback(GetBalance));
            DataRouter.RegisterHandler(OpcodesIn.USER_GET_SUBSCRIPTION_DATA, new ProcessRequestCallback(GetSubscriptionData));
            DataRouter.RegisterHandler(OpcodesIn.USER_GET_IGNORED_USERS, new ProcessRequestCallback(GetIgnoredUsers));
        }

        private static void InitCrypto(Session Session, ClientMessage Message)
        {
            if (Session.Authenticated)
            {
                return;
            }

            Session.SendData(SessionParamsComposer.Compose());
        }

        private static void SsoLogin(Session Session, ClientMessage Message)
        {
            if (Session.Authenticated ||
                ShutdownCommandWorker.Shutdown && ShutdownCommandWorker.Comparassion.Minutes <= 5)
            {
                return;
            }

            string Ticket = UserInputFilter.FilterString(Message.PopString());
            Session.TryAuthenticate(Ticket, Session.RemoteAddress.ToString());
        }

        private static void GetUserInfo(Session Session, ClientMessage Message)
        {
            Session.SendData(UserObjectComposer.Compose(Session));
            Session.SendData(AchievementScoreUpdateComposer.Compose(Session.CharacterInfo.Score));
        }

        private static void GetBalance(Session Session, ClientMessage Message)
        {
            Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
            Session.SendData(UserActivityPointsBalanceComposer.Compose(Session.CharacterInfo.ActivityPoints));
            //Session.SendData(UpdatePixelsBalanceComposer.Compose(Session.CharacterInfo.ActivityPoints[0], 0));
        }

        private static void GetSubscriptionData(Session Session, ClientMessage Message)
        {
            Session.SendData(SubscriptionStatusComposer.Compose(Session.SubscriptionManager));

            if (Session.SubscriptionManager.IsActive && Session.SubscriptionManager.GiftPoints > 0)
            {
                Session.SendData(ClubGiftReadyComposer.Compose(Session.SubscriptionManager.GiftPoints));
            }

            SubscriptionOffer SubscriptionOffer = SubscriptionOfferManager.CheckForSubOffer(Session.SubscriptionManager.SubscriptionLevel, Session.CharacterId);
            if (SubscriptionOffer != null)
            {
                CatalogClubOffer CatalogClubOffer = CatalogManager.ClubOffers.Values.Where(O => O.Level == SubscriptionOffer.OffertedLevel && O.LengthDays == 31).FirstOrDefault();
                SubscriptionOffer.SetClubSubscriptionOffer(CatalogClubOffer);

                Session.SendData(SubscriptionOfferComposer.Compose(Session.SubscriptionManager, SubscriptionOffer));
            }
        }

        private static void GetIgnoredUsers(Session Session, ClientMessage Message)
        {
            ReadOnlyCollection<uint> IgnoredUsers = Session.IgnoreCache.List;
            List<string> Names = new List<string>();

            foreach (uint IgnoredUser in IgnoredUsers)
            {
                string Name = CharacterResolverCache.GetNameFromUid(IgnoredUser);

                if (Name != "Unknown User" && !Names.Contains(Name))
                {
                    Names.Add(Name);
                }
            }

            Session.SendData(IgnoredUserListComposer.Compose(Names));
        }
    }
}
