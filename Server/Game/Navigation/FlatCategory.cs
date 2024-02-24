using System;

namespace Snowlight.Game.Navigation
{
    public class FlatCategory
    {
        private int mId;
        private bool mVisible;
        private string mTitle;
        private bool mAllowTrading;
        private string mRequiredRight;

        public int Id
        {
            get
            {
                return mId;
            }
        }

        public bool Visible
        {
            get
            {
                return mVisible;
            }
        }

        public string Title
        {
            get
            {
                return mTitle;
            }
        }

        public string RequiredRight
        {
            get
            {
                return mRequiredRight; 
            }
        }

        public bool AllowTrading
        {
            get
            {
                return mAllowTrading;
            }
        }

        public FlatCategory(int Id, bool Visible, string Title, string RequiredRight, bool AllowTrading)
        {
            mId = Id;
            mVisible = Visible;
            mTitle = Title;
            mRequiredRight = RequiredRight;
            mAllowTrading = AllowTrading;
        }
    }
}
