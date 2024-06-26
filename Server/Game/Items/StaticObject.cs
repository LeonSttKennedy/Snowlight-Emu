﻿using System;

using Snowlight.Specialized;

namespace Snowlight.Game.Items
{
    public class StaticObject
    {
        private uint mId;
        private string mName;
        private Vector3 mPosition;
        private int mSizeX;
        private int mSizeY;
        private int mRotation;
        private float mHeight;
        private bool mWalkable;
        private bool mIsSeat;

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

        public Vector3 Position
        {
            get
            {
                return mPosition;
            }
        }
        public int SizeX
        {
            get
            {
                return mSizeX;
            }
        }
        public int SizeY
        {
            get
            {
                return mSizeY;
            }
        }
        public int Rotation
        {
            get
            {
                return mRotation;
            }
        }
        public float Height
        {
            get 
            {
                return mHeight;
            }
        }
        public bool Walkable
        {
            get
            {
                return mWalkable;
            }
        }
        public bool IsSeat
        {
            get
            {
                return mIsSeat;
            }
        }

        public bool FirstIlustration
        {
            get
            {
                return mRotation == 0;
            }
        }

        public StaticObject(uint Id, string Name, Vector3 Position, int SizeX, int SizeY, int Rotation, float Height, bool Walkable, bool IsSeat)
        {
            mId = Id;
            mName = Name;
            mPosition = Position;
            mSizeX = SizeX;
            mSizeY = SizeY;
            mRotation = Rotation;
            mHeight = Height;
            mWalkable = Walkable;
            mIsSeat = IsSeat;
        }
    }
}
