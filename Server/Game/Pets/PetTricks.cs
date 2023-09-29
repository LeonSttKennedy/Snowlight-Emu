using Snowlight.Game.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Game.Pets
{
    public class PetTricks
    {
        private string mTrick;
        private bool mNeedsToy;
        private ItemBehavior mToyBehavior;

        public string Trick
        {
            get
            {
                return mTrick;
            }
        }
        public bool NeedsToy
        {
            get
            {
                return mNeedsToy;
            }
        }
        public ItemBehavior ToyBehavior
        {
            get
            {
                return mToyBehavior;
            }
        }

        public PetTricks(string Trick, bool NeedsToy, ItemBehavior ToyBehavior)
        {
            mTrick = Trick;
            mNeedsToy = NeedsToy;
            mToyBehavior = ToyBehavior;
        }
    }
}
