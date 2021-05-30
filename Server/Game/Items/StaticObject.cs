using System;

using Snowlight.Specialized;

namespace Snowlight.Game.Items
{
    public class StaticObject
    {
        private string mName;
        private Vector3 mPosition;
        private int mRotation;
        private bool mIsSeat;

        public string Name
        {
            get
            {
                return mName;
            }
        }

        public Vector3 Position
        {
            get
            {
                return mPosition;
            }
        }

        public int Rotation
        {
            get
            {
                return mRotation;
            }
        }

        public bool IsSeat
        {
            get
            {
                return mIsSeat;
            }
        }

        public StaticObject(string Name, Vector3 Position, int Rotation, bool IsSeat)
        {
            mName = Name;
            mPosition = Position;
            mRotation = Rotation;
            mIsSeat = IsSeat;
        }
    }
}
