using System;

namespace Snowlight.Specialized
{
    public class Vector2
    {
        private int mX;
        private int mY;

        public int X
        {
            get
            {
                return mX;
            }

            set
            {
                mX = value;
            }
        }

        public int Y
        {
            get
            {
                return mY;
            }

            set
            {
                mY = value;
            }
        }

        public Vector2()
        {
            mX = 0;
            mY = 0;
        }

        public Vector2(int X, int Y)
        {
            mX = X;
            mY = Y;
        }

        public Vector3 GetVector3()
        {
            return new Vector3(mX, mY, 0);
        }

        public override string ToString()
        {
            return X + "|" + Y;
        }

        public static Vector2 FromString(string Input)
        {
            string[] Bits = Input.Split('|');

            int X = 0;
            int Y = 0;

            int.TryParse(Bits[0], out X);

            if (Bits.Length > 1)
            {
                int.TryParse(Bits[1], out Y);
            }

            return new Vector2(X, Y);
        }

        public int GetDistanceSquared(Vector2 Point)
        {
            int dx = this.X - Point.X;
            int dy = this.Y - Point.Y;
            return (dx * dx) + (dy * dy);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2)
            {
                Vector2 v2 = (Vector2)obj;
                return v2.X == X && v2.Y == Y;
            }

            return false;
        }
        public override int GetHashCode()
        {
            return (X + " " + Y).GetHashCode();
        }

        public static Vector2 operator +(Vector2 One, Vector2 Two)
        {
            return new Vector2(One.X + Two.X, One.Y + Two.Y);
        }
        public static Vector2 operator -(Vector2 One, Vector2 Two)
        {
            return new Vector2(One.X - Two.X, One.Y - Two.Y);
        }
    }
}
