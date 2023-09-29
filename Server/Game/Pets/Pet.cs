using System;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Bots;
using Snowlight.Game.Rooms;
using Snowlight.Specialized;
using Snowlight.Game.Sessions;
using Snowlight.Game.Achievements;
using Snowlight.Game.Quests;
using Snowlight.Communication.Outgoing;
using System.Linq;
using Snowlight.Game.Characters;

namespace Snowlight.Game.Pets
{
    public class Pet
    {
        public static readonly List<int> PET_EXP_LEVELS = new List<int>() { 100, 200, 400, 600, 900, 1300, 1800, 2400, 3100,
            3900, 4800, 5800, 6900, 8100, 9400, 10800, 12300, 14000, 15800, 17700 };
        public static readonly List<int> PET_ENERGY_LEVELS = new List<int>() { 120, 140, 160, 180, 200, 220, 240, 260, 280,
            300, 320, 340, 360, 380, 400, 420, 440, 460, 480, 500 };

        private uint mId;
        private string mName;
        private int mType;
        private int mRace;
        private string mColor;
        private uint mOwnerId;
        private uint mRoomId;
        private Vector3 mRoomPosition;
        private int mRoomRotation;
        private double mTimestamp;
        private int mExperience;
        private int mEnergy;
        private int mHappiness;
        private int mScore;
        private uint mVirtualId;

        public uint Id
        {
            get
            {
                return mId;
            }
        }

        public string Name
        {
            get
            {
                return mName;
            }
        }

        public string Look
        {
            get
            {
                return mType + " " + mRace + " " + ColorCode.ToLower();
            }
        }

        public int Type
        {
            get
            {
                return mType;
            }
        }

        public int Race
        {
            get
            {
                return mRace;
            }
        }

        public uint OwnerId
        {
            get
            {
                return mOwnerId;
            }
        }

        public string OwnerName
        {
            get
            {
                return CharacterResolverCache.GetNameFromUid(mOwnerId);
            }
        }

        public uint RoomId
        {
            get
            {
                return mRoomId;
            }
        }

        public bool IsInRoom
        {
            get
            {
                return mRoomId > 0;
            }
        }
        public Vector3 RoomPosition
        {
            get
            {
                return mRoomPosition;
            }
        }

        public int RoomRotation
        {
            get
            {
                return mRoomRotation;
            }
        }
        public double Timestamp
        {
            get
            {
                return mTimestamp;
            }
        }

        public double AgeSeconds
        {
            get
            {
                return UnixTimestamp.GetCurrent() - mTimestamp;
            }
        }

        public int AgeDays
        {
            get
            {
                return (int)(AgeSeconds / 86400);
            }
        }

        public string ColorCode
        {
            get
            {
                return mColor;
            }
        }

        public int TotalLevels
        {
            get
            {
                return PET_EXP_LEVELS.Count;
            }
        }

        public int Level
        {
            get
            {
                int Level = 1;

                foreach (int Exp in PET_EXP_LEVELS)
                {
                    if (Exp > mExperience)
                    {
                        break;
                    }

                    Level++;
                }

                return Level > TotalLevels ? TotalLevels : Level;
            }
        }

        public int ExperienceTarget
        {
            get
            {
                return PET_EXP_LEVELS[Level - 1];
            }
        }

        public int Experience
        {
            get
            {
                return mExperience;
            }
        }

        public int Energy
        {
            get
            {
                return mEnergy;
            }

            set
            {
                mEnergy = value;

                if (mEnergy < 0)
                {
                    mEnergy = 0;
                }

                if (mEnergy > EnergyLimit)
                {
                    mEnergy = EnergyLimit;
                }
            }
        }

        public int Happiness
        {
            get
            {
                return mHappiness;
            }

            set
            {
                mHappiness = value;

                if (mHappiness < 0)
                {
                    mHappiness = 0;
                }

                if (mHappiness > HappinessLimit)
                {
                    mHappiness = HappinessLimit;
                }
            }
        }

        public int HappinessLimit
        {
            get
            {
                return 100;
            }
        }

        public int EnergyLimit
        {
            get
            {
                return PET_ENERGY_LEVELS[Level - 1];
            }
        }

        public int Score
        {
            get
            {
                return mScore;
            }

            set
            {
                mScore = value;
            }
        }
        public List<PetCommands> CommandList
        {
            get
            {
                return PetDataManager.GetCommandsForType(mType);
            }
        }

        public List<PetCommands> AvailablePetCommands
        {
            get
            {
                return CommandList.Where(C => Level >= C.MinLevel).ToList();
            }
        }

        public uint VirtualId
        {
            get
            {
                return mVirtualId;
            }
        }

        public Pet(uint Id, string Name, int PetType, int Race, string Color, uint UserId, uint RoomId, Vector3 RoomPosition,
            double Timestamp, int Experience, int Energy, int Happiness, int Score)
        {
            mId = Id;
            mName = Name;
            mType = PetType;
            mRace = Race;
            mColor = Color;
            mOwnerId = UserId;
            mRoomId = RoomId;
            mRoomPosition = RoomPosition;
            mTimestamp = Timestamp;
            mExperience = Experience;
            mEnergy = Energy;
            mHappiness = Happiness;
            mScore = Score;
        }

        public void MoveToUserInventory()
        {
            mRoomId = 0;
            mRoomPosition = new Vector3(0, 0, 0);
        }

        public void MoveToRoom(uint RoomId, Vector3 RoomPosition, int RoomRotation = 4)
        {
            mRoomId = RoomId;
            mRoomPosition = RoomPosition;
            mRoomRotation = RoomRotation;
        }
        public void OnRespect(SqlDatabaseClient MySqlClient, RoomInstance Instance)
        {
            mScore++;

            Instance.BroadcastMessage(PetRespectComposer.Compose(this));

            if (mExperience >= PET_EXP_LEVELS[PET_EXP_LEVELS.Count - 1])
            {
                return;
            }

            AddExperience(MySqlClient, Instance, 10);
        }
        public void AddExperience(SqlDatabaseClient MySqlClient, RoomInstance Instance, int Amount)
        {
            int CurrentExperienceTarget = ExperienceTarget;

            mExperience += Amount;

            if (mExperience >= CurrentExperienceTarget && Level != TotalLevels)
            {
                CharacterInfo OwnerInfo = CharacterInfoLoader.GetCharacterInfo(MySqlClient, mOwnerId);

                if(OwnerInfo.HasLinkedSession)
                {
                    Session OwnerSession = SessionManager.GetSessionByCharacterId(mOwnerId);

                    if (OwnerSession.CurrentRoomId.Equals(mRoomId))
                    {
                        OwnerSession.SendData(PetLevelUpComposer.Compose(this));
                    }

                    AchievementManager.ProgressUserAchievement(MySqlClient, OwnerSession, "ACH_PetLevelUp", 1);
                    QuestManager.ProgressUserQuest(OwnerSession, QuestType.LEVEL_UP_A_PET, 1);
                }
                else
                {
                    AchievementManager.OfflineProgressUserAchievement(MySqlClient, mOwnerId, "ACH_PetLevelUp", 1);
                }                

                mEnergy = PET_ENERGY_LEVELS[Level + 1];

                RoomActor Actor = Instance.GetActor(mVirtualId);
                if (Actor != null)
                {
                    Actor.SetStatus("gst", "exp");
                    Actor.UpdateNeeded = true;
                }

                Instance.BroadcastMessage(RoomChatComposer.Compose(mVirtualId, "*leveled up to level " + Level + "*", 0, ChatType.Say));
            }

            Instance.BroadcastMessage(PetAddExperiencePointsComposer.Compose(this, Amount));
            RoomManager.MarkWriteback(this);
        }

        public void PetHappiness(int Add)
        {
            int AditionalHappiness = Happiness + Add;

            Happiness = AditionalHappiness >= HappinessLimit ? HappinessLimit: AditionalHappiness;

            RoomManager.MarkWriteback(this);
        }

        public void PetEnergy(bool Add, bool Isfood = false, RoomInstance Instance = null)
        {
            int MaxE;
            if (Add)
            {
                if (Energy == EnergyLimit)
                {
                    return;
                }

                if (Energy > 85)
                {
                    MaxE = EnergyLimit - Energy;
                }
                else
                {
                    MaxE = 10;
                }

            }
            else
            {
                MaxE = 15;
            }

            if (MaxE <= 4)
            {
                MaxE = 15;
            }

            int Random = RandomGenerator.GetNext(4, MaxE);

            if (!Add)
            {
                Energy -= Random;

                if (Energy < 0)
                {
                    Energy = 1;
                    Random = 1;
                }
            }
            else
            {
                Energy += Random;

                RoomActor Actor = Instance.GetActor(mVirtualId);
                if (Actor != null)
                {
                    Actor.SetStatus("gst", "nrg");
                    Actor.UpdateNeeded = true;
                }

                if(Isfood)
                {
                    using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                    {
                        CharacterInfo RoomOwnerInfo = CharacterInfoLoader.GetCharacterInfo(MySqlClient, Instance.Info.OwnerId);

                        if(RoomOwnerInfo.HasLinkedSession) 
                        {
                            Session TargetSession = SessionManager.GetSessionByCharacterId(RoomOwnerInfo.Id);
                            AchievementManager.ProgressUserAchievement(MySqlClient, TargetSession, "ACH_PetFeeding", Random);
                        }
                        else
                        {
                            AchievementManager.OfflineProgressUserAchievement(MySqlClient, RoomOwnerInfo.Id, "ACH_PetFeeding", Random);
                        }
                    }
                }
            }

            RoomManager.MarkWriteback(this);
        }

        public void SetVirtualId(uint VirtualId)
        {
            mVirtualId = VirtualId;
        }

        public void SynchronizeDatabase(SqlDatabaseClient MySqlClient)
        {
            MySqlClient.SetParameter("id", mId);
            MySqlClient.SetParameter("roomid", mRoomId);
            MySqlClient.SetParameter("roompos", mRoomPosition.ToString());
            MySqlClient.SetParameter("roomrot", mRoomRotation);
            MySqlClient.SetParameter("exp", mExperience);
            MySqlClient.SetParameter("energy", mEnergy);
            MySqlClient.SetParameter("happy", mHappiness);
            MySqlClient.SetParameter("score", mScore);
            MySqlClient.ExecuteNonQuery("UPDATE user_pets SET room_id = @roomid, room_pos = @roompos, room_rot = @roomrot, experience = @exp, energy = @energy, happiness = @happy, score = @score WHERE id = @id LIMIT 1");
        }
    }
}
