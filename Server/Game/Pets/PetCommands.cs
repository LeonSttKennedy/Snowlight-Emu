using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Game.Pets
{
    public enum PetCommand
    {
        // Total Pet COMMANDS: 44
        None = -1,
        Free = 0,
        Sit = 1,
        Down = 2,
        Here = 3,
        Beg = 4,
        PlayDead = 5,
        Stay = 6,
        Follow = 7,
        Stand = 8,
        Jump = 9,
        Speak = 10,
        Play = 11,
        Silent = 12,
        Nest = 13,
        Drink = 14,
        FollowLeft = 15,
        FollowRight = 16,
        PlayFootball = 17,
        ComeHere = 18,
        Bounce = 19,
        Flat = 20,
        Dance = 21,
        Spin = 22,
        SwitchTV = 23,
        MoveForward = 24,
        TurnLeft = 25,
        TurnRight = 26,
        Relax = 27,
        Croak = 28,
        Dip = 29,
        Wave = 30,
        Mambo = 31,
        HighJump = 32,
        ChickenDance = 33,
        TripleJump = 34,
        SpreadWings = 35,
        BreatheFire = 36,
        Hang = 37,
        Torch = 38,
        Swing = 40,
        Roll = 41,
        RingofFire = 42,
        Eat = 43,
        WagTail = 44
    }

    public class PetCommands
    {
        private PetCommand mId;
        private int mMinLevel;
        private List<string> mCommandTexts;
        private bool mOnlyOwner;

        public PetCommand Id
        {
            get
            {
                return mId;
            }
        }
        public int MinLevel
        {
            get
            {
                return mMinLevel;
            }
        }

        public List<string> CommandTexts
        {
            get
            {
                return mCommandTexts;
            }
        }

        public bool OnlyOwner
        {
            get
            {
                return mOnlyOwner;
            }
        }

        public PetCommands(PetCommand Id, int MinLevel, bool OwnerOnly)
        {
            mId = Id;
            mMinLevel = MinLevel;
            mCommandTexts = new List<string>();
            mOnlyOwner = OwnerOnly;
        }

        public void AddCommandText(string CommandText)
        {
            if(mCommandTexts.Contains(CommandText))
            {
                return;
            }

            mCommandTexts.Add(CommandText);
        }
    }
}
