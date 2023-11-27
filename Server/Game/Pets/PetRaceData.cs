using System;

namespace Snowlight.Game.Pets
{
    public class PetRaceData
    {
        private int mBreed;
        private bool mSellable;
        private bool mIsRare;

        public int Breed
        {
            get
            {
                return mBreed;
            }
        }

        public bool Sellable
        {
            get
            {
                return mSellable;
            }
        }

        public bool IsRare
        {
            get
            {
                return mIsRare;
            }
        }

        public PetRaceData(int Breed, bool Sellable, bool IsRare)
        {
            mBreed = Breed;
            mSellable = Sellable;
            mIsRare = IsRare;
        }
    }
}
