using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Data;

using Snowlight.Storage;
using Snowlight.Game.Sessions;
using Snowlight.Game.Rights;
using Snowlight.Game.Handlers;
using Snowlight.Game.Achievements;
using Snowlight.Specialized;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Moderation;

namespace Snowlight.Game.Characters
{
    public enum CharacterGender
    {
        Male = 0,
        Female = 1
    }

    public class CharacterInfo
    {
        private uint mId;
        private uint mSessionId;
        private string mUsername;
        private string mRealName;
        private string mFigure;
        private CharacterGender mGender;
        private string mMotto;

        private int mCredits;
        private int mActivityPoints;

        private uint mHomeRoom;
        private int mScore;
        private bool mPrivacyAcceptFriends;
        private int mConfigVolume;

        private int mRespectPoints;
        private int mRespectCreditHuman;
        private int mRespectCreditPets;

        private Dictionary<int, WardrobeItem> mWardrobe;
        private List<string> mTags;

        private static Dictionary<string, int> mInFractions;
        private int mModerationTickets;
        private int mModerationTicketsAbusive;
        private double mModerationTicketsCooldown;
        private int mModerationBans;
        private int mModerationCautions;
        private double mModerationMutedUntil;

        private double mCacheAge;
        private double mTimestampLastOnline;
        private double mTimestampRegistered;
        private double mTimestampLastActivityPointsUpdate;

        private double mLastRespectUpdate;
        private bool mCalledGuideBot;
        private int mMarketplaceTokens;

        private int mRegularVisitor;
        private int mTimeOnline;

        private bool mAllowGifting;
        private DateTime mLastGiftSent;
        private int mGiftingWarningCounter;

        private bool mIsOnline;

        private bool mAllowMimic;
        private bool mAllowGifts;

        public uint Id
        {
            get
            {
                return mId;
            }
        }

        public uint SessionId
        {
            get
            {
                return mSessionId;
            }
        }

        public string Username
        {
            get
            {
                return mUsername;
            }
        }

        public string RealName
        {
            get
            {
                return mRealName;
            }
        }

        public string Figure
        {
            get
            {
                return mFigure;
            }
        }

        public CharacterGender Gender
        {
            get
            {
                return mGender;
            }
        }

        public string Motto
        {
            get
            {
                return mMotto;
            }
        }
        public int CreditsBalance
        {
            get
            {
                return mCredits;
            }

            set
            {
                mCredits = value;
            }
        }

        public int ActivityPointsBalance
        {
            get
            {
                return mActivityPoints;
            }

            set
            {
                mActivityPoints = value;
            }
        }

        public double TimeSinceLastActivityPointsUpdate
        {
            get
            {
                return (UnixTimestamp.GetCurrent() - mTimestampLastActivityPointsUpdate);
            }
        }

        public bool PrivacyAcceptFriends
        {
            get
            {
                return mPrivacyAcceptFriends;
            }
        }

        public bool HasLinkedSession
        {
            get
            {
                return (SessionId > 0);
            }
        }

        public double CacheAge
        {
            get
            {
                return (UnixTimestamp.GetCurrent() - mCacheAge);
            }
        }

        public uint HomeRoom
        {
            get
            {
                return mHomeRoom;
            }

            set
            {
                mHomeRoom = value;
            }
        }

        public int ConfigVolume
        {
            get
            {
                return mConfigVolume;
            }

            set
            {
                mConfigVolume = value;

                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    MySqlClient.SetParameter("userid", mId);
                    MySqlClient.SetParameter("volume", mConfigVolume);
                    MySqlClient.ExecuteNonQuery("UPDATE characters SET config_volume = @volume WHERE id = @userid LIMIT 1");
                }
            }
        }
        public int RespectPoints
        {
            get
            {
                return mRespectPoints;
            }

            set
            {
                mRespectPoints = value;
            }
        }

        public int RespectCreditHuman
        {
            get
            {
                return mRespectCreditHuman;
            }

            set
            {
                mRespectCreditHuman = value;
            }
        }

        public int RespectCreditPets
        {
            get
            {
                return mRespectCreditPets;
            }

            set
            {
                mRespectCreditPets = value;
            }
        }

        public int Score
        {
            get
            {
                return mScore;
            }
        }

        public Dictionary<int, WardrobeItem> Wardrobe
        {
            get
            {
                return new Dictionary<int, WardrobeItem>(mWardrobe);
            }
        }

        public ReadOnlyCollection<string> Tags
        {
            get
            {
                lock (mTags)
                {
                    List<string> Copy = new List<string>();
                    Copy.AddRange(mTags);
                    return Copy.AsReadOnly();
                }
            }
        }

        public Dictionary<string, int> InFraction
        {
            get
            {
                return mInFractions;
            }
        }

        public int ModerationTickets
        {
            get
            {
                return mModerationTickets;
            }

            set
            {
                mModerationTickets = value;
            }
        }

        public int ModerationTicketsAbusive
        {
            get
            {
                return mModerationTicketsAbusive;
            }

            set
            {
                mModerationTicketsAbusive = value;
            }
        }

        public double ModerationTicketsCooldownTimestamp
        {
            get
            {
                return mModerationTicketsCooldown;
            }
        }

        public double ModerationTicketsCooldownSeconds
        {
            get
            {
                return (mModerationTicketsCooldown - UnixTimestamp.GetCurrent());
            }

            set
            {
                mModerationTicketsCooldown = UnixTimestamp.GetCurrent() + value;
            }
        }

        public int ModerationBans
        {
            get
            {
                return mModerationBans;
            }

            set
            {
                mModerationBans = value;
            }
        }

        public int ModerationCautions
        {
            get
            {
                return mModerationCautions;
            }

            set
            {
                mModerationCautions = value;
            }
        }

        public double MutedUntilTimestamp
        {
            get
            {
                return mModerationMutedUntil;
            }
        }

        public double MutedSecondsLeft
        {
            get
            {
                return (mModerationMutedUntil - UnixTimestamp.GetCurrent());
            }
        }

        public bool IsMuted
        {
            get
            {
                return MutedSecondsLeft > 0;
            }
        }

        public double TimestampLastOnline
        {
            get
            {
                return mTimestampLastOnline;
            }

            set
            {
                mTimestampLastOnline = value;
            }
        }

        public double TimestampRegistered
        {
            get
            {
                return mTimestampRegistered;
            }
        }
        public double TimeSinceLastRespectPointsUpdate
        {
            get
            {
                return mLastRespectUpdate;
            }
            set
            {
                mLastRespectUpdate = value;
            }
        }
        internal Boolean NeedsRespectUpdate
        {
            get
            {
                if (TimeSinceLastRespectPointsUpdate == 0)
                {
                    return true;
                }
                double Time = (UnixTimestamp.GetCurrent() - TimeSinceLastRespectPointsUpdate);
                TimeSpan Diff = TimeSpan.FromSeconds(Time);
                Boolean NeedsUpdate = false;

                if (SessionManager.GetSessionByCharacterId(Id).HasRight("club_vip") && Diff.TotalHours >= 2) { NeedsUpdate = true; }
                if (!SessionManager.GetSessionByCharacterId(Id).HasRight("club_vip") && Diff.TotalDays >= 1) { NeedsUpdate = true; }

                return (NeedsUpdate && (this.RespectCreditPets < 3 || this.RespectCreditHuman < 3));
            }
        }
        public bool CalledGuideBot
        {
            get 
            {
                return mCalledGuideBot;
            }
            set 
            {
                mCalledGuideBot = value;
            }
        }
        public int MarketplaceTokensTotal
        {
            get
            {
                return mMarketplaceTokens;
            }
            set
            {
                mMarketplaceTokens = value;
            }
        }
        public int RegularVisitorinDays
        {
            get
            {
                return mRegularVisitor;
            }
            set
            {
                mRegularVisitor = value;
            }
        }
        public int TimeOnline
        {
            get
            {
                return mTimeOnline;
            }
            set
            {
                mTimeOnline = value;
            }
        }
        public bool AllowGifting
        {
            get 
            {
                return mAllowGifting;
            }
            
            set 
            {
                mAllowGifting = value;
            }
        }

        public DateTime LastGiftSent
        {
            get 
            {
                return mLastGiftSent;
            }

            set
            {
                mLastGiftSent = value;
            }
        }

        public int GiftWarningCounter
        {
            get 
            {
                return mGiftingWarningCounter;
            }

            set
            {
                mGiftingWarningCounter = value;
            }
        }
        public bool Online
        {
            get 
            {
                return mIsOnline;
            }
            set
            {
                mIsOnline = value;
            }
        }
        public bool AllowMimic
        {
            get
            {
                return mAllowMimic;
            }

            set
            {
                mAllowMimic = value;
            }
        }

        public bool AllowGifts
        {
            get
            {
                return mAllowGifts;
            }

            set
            {
                mAllowGifts = value;
            }
        }

        public CharacterInfo(SqlDatabaseClient MySqlClient, uint SessionId, uint Id, string Username, string RealName, string Figure,
            CharacterGender Gender, string Motto, int Credits, int ActivityPoints, double ActivityPointsLastUpdate,
            bool PrivacyAcceptFriends, uint HomeRoom, int Score, int ConfigVolume, int ModerationTickets,
            int ModerationTicketsAbusive, double ModerationTicketCooldown, int ModerationBans, int ModerationCautions,
            double TimestampLastOnline, double TimestampRegistered, int RespectPoints, int RespectCreditHuman,
            int RespectCreditPets, double TimestampLastRespectUpdate, double ModerationMutedUntil, int MarketplaceTokens,
            int RegularVisitor, int TimeOnline, bool Online , bool AllowMimic, bool AllowGifts)
        {
            mSessionId = SessionId;
            mId = Id;
            mUsername = Username;
            mRealName = RealName;
            mFigure = Figure;
            mGender = Gender;
            mMotto = Motto;
            mCredits = Credits;

            mActivityPoints = ActivityPoints;
            mPrivacyAcceptFriends = PrivacyAcceptFriends;
            mHomeRoom = HomeRoom;
            mScore = Score;
            mConfigVolume = ConfigVolume;

            mRespectPoints = RespectPoints;
            mRespectCreditHuman = RespectCreditHuman;
            mRespectCreditPets = RespectCreditPets;

            mInFractions = new Dictionary<string, int>();
            mModerationTickets = ModerationTickets;
            mModerationTicketsAbusive = ModerationTicketsAbusive;
            mModerationTicketsCooldown = ModerationTicketCooldown;
            mModerationCautions = ModerationCautions;
            mModerationBans = ModerationBans;
            mModerationMutedUntil = ModerationMutedUntil;

            mCacheAge = UnixTimestamp.GetCurrent();
            mTimestampLastActivityPointsUpdate = ActivityPointsLastUpdate;
            mTimestampLastOnline = TimestampLastOnline;
            mTimestampRegistered = TimestampRegistered;

            mLastRespectUpdate = TimestampLastRespectUpdate;
            mCalledGuideBot = false;
            mMarketplaceTokens = MarketplaceTokens;

            mRegularVisitor = RegularVisitor;
            mTimeOnline = TimeOnline;

            mAllowGifting = true;
            mLastGiftSent = DateTime.Now;
            mGiftingWarningCounter = 0;

            mWardrobe = new Dictionary<int, WardrobeItem>();
            mTags = new List<string>();

            mIsOnline = Online;

            mAllowMimic = AllowMimic;
            mAllowMimic = AllowGifts;

            if (MySqlClient != null)
            {
                MySqlClient.SetParameter("userid", mId);
                DataTable Table = MySqlClient.ExecuteQueryTable("SELECT * FROM wardrobe WHERE user_id = @userid LIMIT 10");

                foreach (DataRow Row in Table.Rows)
                {
                    mWardrobe.Add((int)Row["slot_id"], new WardrobeItem((string)Row["figure"], (Row["gender"].ToString().ToLower() == "m" ? CharacterGender.Male : CharacterGender.Female)));
                }

                UpdateTags(MySqlClient);
            }
        }

        public int GetRoomCount()
        {
            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                MySqlClient.SetParameter("ownerid", mId);
                return int.Parse(MySqlClient.ExecuteScalar("SELECT COUNT(*) FROM rooms WHERE owner_id = @ownerid LIMIT 1").ToString());
            }
        }
        public void UpdateTags(SqlDatabaseClient MySqlClient)
        {
            mTags.Clear();

            MySqlClient.SetParameter("userid", mId);
            DataTable TagsTable = MySqlClient.ExecuteQueryTable("SELECT * FROM tags WHERE user_id = @userid");

            foreach (DataRow Row in TagsTable.Rows)
            {
                mTags.Add((string)Row["tag"]);
            }
        }
        public void UpdateOnline(SqlDatabaseClient MySqlClient)
        {
            MySqlClient.SetParameter("userid", mId);
            MySqlClient.SetParameter("online", mIsOnline? "1" : "0");
            MySqlClient.ExecuteNonQuery("UPDATE characters SET online = @online WHERE id = @userid LIMIT 1");
        }
        public void UpdateScore(SqlDatabaseClient MySqlClient, int Amount)
        {
            mScore += Amount;

            MySqlClient.SetParameter("userid", mId);
            MySqlClient.SetParameter("score", mScore);
            MySqlClient.ExecuteNonQuery("UPDATE characters SET score = @score WHERE id = @userid LIMIT 1");
        }
        public void UpdateLastOnline(SqlDatabaseClient MySqlClient)
        {
            MySqlClient.SetParameter("userid", mId);
            MySqlClient.SetParameter("lastonline", UnixTimestamp.GetCurrent());
            MySqlClient.ExecuteNonQuery("UPDATE characters SET timestamp_lastvisit = @lastonline WHERE id = @userid LIMIT 1");
        }
        public void UpdateCreditsBalance(SqlDatabaseClient MySqlClient, int Amount)
        {
            CreditsBalance += Amount;

            MySqlClient.SetParameter("id", Id);
            MySqlClient.SetParameter("credits", CreditsBalance);
            MySqlClient.ExecuteNonQuery("UPDATE characters SET credits_balance = @credits WHERE id = @id LIMIT 1");
        }
        public void UpdateMarketplaceTokens(SqlDatabaseClient MySqlClient, int Amount)
        {
            MarketplaceTokensTotal += Amount;

            MySqlClient.SetParameter("id", mId);
            MySqlClient.SetParameter("marketplacetokens", MarketplaceTokensTotal);
            MySqlClient.ExecuteNonQuery("UPDATE characters SET marketplace_tickets = @marketplacetokens WHERE id = @id LIMIT 1");
        }
        public void UpdateRegularVisitor(SqlDatabaseClient MySqlClient, int Amount)
        {
            RegularVisitorinDays = Amount;

            MySqlClient.SetParameter("id", mId);
            MySqlClient.SetParameter("regularvisitor", RegularVisitorinDays);
            MySqlClient.ExecuteNonQuery("UPDATE characters SET regular_visitor = @regularvisitor WHERE id = @id LIMIT 1");
        }
        public void UpdateTimeOnline(SqlDatabaseClient MySqlClient, int Amount)
        {
            TimeOnline += Amount;

            MySqlClient.SetParameter("id", mId);
            MySqlClient.SetParameter("timeonline", TimeOnline);
            MySqlClient.ExecuteNonQuery("UPDATE characters SET time_online = @timeonline WHERE id = @id LIMIT 1");
        }
        public void UpdateActivityPointsBalance(SqlDatabaseClient MySqlClient, int Amount)
        {
            ActivityPointsBalance += Amount;

            MySqlClient.SetParameter("id", Id);
            MySqlClient.SetParameter("ap", ActivityPointsBalance);
            MySqlClient.ExecuteNonQuery("UPDATE characters SET activity_points_balance = @ap WHERE id = @id LIMIT 1");
        }
        public void SetLastRespectUpdate(SqlDatabaseClient MySqlClient)
        {
            mLastRespectUpdate = UnixTimestamp.GetCurrent();

            MySqlClient.SetParameter("id", mId);
            MySqlClient.SetParameter("aplru", mLastRespectUpdate);
            MySqlClient.ExecuteNonQuery("UPDATE characters SET last_respect_update = @aplru WHERE id = @id LIMIT 1");
        }
        public void SetLastActivityPointsUpdate(SqlDatabaseClient MySqlClient)
        {
            mTimestampLastActivityPointsUpdate = UnixTimestamp.GetCurrent();

            MySqlClient.SetParameter("id", mId);
            MySqlClient.SetParameter("aplu", mTimestampLastActivityPointsUpdate);
            MySqlClient.ExecuteNonQuery("UPDATE characters SET activity_points_last_update = @aplu WHERE id = @id LIMIT 1");          
        }

        public void UpdateMimicPreference(SqlDatabaseClient MySqlClient)
        {
            MySqlClient.SetParameter("userid", mId);
            MySqlClient.SetParameter("mimic", mAllowMimic ? "1" : "0");

            MySqlClient.ExecuteNonQuery("UPDATE characters SET allow_mimic = @mimic WHERE id = @userid LIMIT 1");
        }

        public void UpdateGiftsPreference(SqlDatabaseClient MySqlClient)
        {
            MySqlClient.SetParameter("userid", mId);
            MySqlClient.SetParameter("gifts", mAllowGifts ? "1" : "0");

            MySqlClient.ExecuteNonQuery("UPDATE characters SET allow_gifts = @gifts WHERE id = @userid LIMIT 1");
        }

        public void UpdateFigure(SqlDatabaseClient MySqlClient, string NewGender, string NewFigure)
        {
            mGender = (NewGender == "m" ? CharacterGender.Male : CharacterGender.Female);
            mFigure = NewFigure;

            MySqlClient.SetParameter("userid", mId);
            MySqlClient.SetParameter("figure", mFigure);
            MySqlClient.SetParameter("gender", NewGender);
            MySqlClient.ExecuteNonQuery("UPDATE characters SET gender = @gender, figure = @figure WHERE id = @userid LIMIT 1");
        }

        public void UpdateMotto(SqlDatabaseClient MySqlClient, string NewMotto)
        {
            mMotto = NewMotto;

            MySqlClient.SetParameter("userid", mId);
            MySqlClient.SetParameter("motto", NewMotto);
            MySqlClient.ExecuteNonQuery("UPDATE characters SET motto = @motto WHERE id = @userid LIMIT 1");
        }

        public void SetWardrobeSlot(SqlDatabaseClient MySqlClient, int SlotId, string Figure, CharacterGender Gender)
        {
            lock (mWardrobe)
            {
                WardrobeItem Item = new WardrobeItem(Figure, Gender);

                MySqlClient.SetParameter("userid", mId);
                MySqlClient.SetParameter("slotid", SlotId);
                MySqlClient.SetParameter("figure", Figure);
                MySqlClient.SetParameter("gender", Gender == CharacterGender.Male ? "M" : "F");

                if (!mWardrobe.ContainsKey(SlotId))
                {
                    mWardrobe.Add(SlotId, Item);
                    MySqlClient.ExecuteNonQuery("INSERT INTO wardrobe (user_id,slot_id,figure,gender) VALUES (@userid,@slotid,@figure,@gender)");
                    return;
                }

                mWardrobe[SlotId] = Item;                             
                MySqlClient.ExecuteNonQuery("UPDATE wardrobe SET figure = @figure, gender = @gender WHERE user_id = @userid AND slot_id = @slotid LIMIT 1");                        
            }
        }

        public void SetHomeRoom(SqlDatabaseClient MySqlClient, uint RoomId)
        {
            mHomeRoom = RoomId;

            MySqlClient.SetParameter("userid", mId);
            MySqlClient.SetParameter("roomid", RoomId);
            MySqlClient.ExecuteNonQuery("UPDATE characters SET home_room = @roomid WHERE id = @userid LIMIT 1");
        }

        public void SynchronizeStatistics(SqlDatabaseClient MySqlClient)
        {
            MySqlClient.SetParameter("id", mId);
            MySqlClient.SetParameter("timestamp", UnixTimestamp.GetCurrent());
            MySqlClient.SetParameter("tickets", mModerationTickets);
            MySqlClient.SetParameter("ticketsabuse", mModerationTicketsAbusive);
            MySqlClient.SetParameter("cooldown", mModerationTicketsCooldown);
            MySqlClient.SetParameter("bans", mModerationBans);
            MySqlClient.SetParameter("cautions", mModerationCautions);
            MySqlClient.ExecuteNonQuery("UPDATE characters SET timestamp_lastvisit = @timestamp, moderation_tickets = @tickets, moderation_tickets_abusive = @ticketsabuse, moderation_tickets_cooldown = @cooldown, moderation_bans = @bans, moderation_cautions = @cautions WHERE id = @id LIMIT 1");
        }

        public void SynchronizeRespectData(SqlDatabaseClient MySqlClient)
        {
            MySqlClient.SetParameter("id", mId);
            MySqlClient.SetParameter("respectpts", mRespectPoints);
            MySqlClient.SetParameter("respectcredh", mRespectCreditHuman);
            MySqlClient.SetParameter("respectcredp", mRespectCreditPets);
            MySqlClient.ExecuteNonQuery("UPDATE characters SET respect_points = @respectpts, respect_credit_humans = @respectcredh, respect_credit_pets = @respectcredp WHERE id = @id LIMIT 1");
        }

        public void Mute(SqlDatabaseClient MySqlClient, int TimeToMute)
        {
            mModerationMutedUntil = UnixTimestamp.GetCurrent() + TimeToMute;

            // Maintain in database if this mute lasts longer than 3 minutes
            if (TimeToMute >= 180)
            {
                MySqlClient.SetParameter("id", mId);
                MySqlClient.SetParameter("mutetime", mModerationMutedUntil);
                MySqlClient.ExecuteNonQuery("UPDATE characters SET moderation_muted_until_timestamp = @mutetime WHERE id = @id LIMIT 1");
            }
        }
        
        public void Unmute(SqlDatabaseClient MySqlClient)
        {
            mModerationMutedUntil = 0;

            MySqlClient.SetParameter("id", mId);
            MySqlClient.ExecuteNonQuery("UPDATE characters SET moderation_muted_until_timestamp = 0 WHERE id = @id LIMIT 1");
        }
    }
}
