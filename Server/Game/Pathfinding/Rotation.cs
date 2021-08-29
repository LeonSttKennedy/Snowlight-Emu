﻿using System;
using Snowlight.Specialized;

namespace Snowlight.Game.Pathfinding
{
    public static class Rotation
    {
        public static int Calculate(Vector2 PositionOne, Vector2 PositionTwo, bool MoonWalk)
        {
            int Rotation = Calculate(PositionOne, PositionTwo);

            return (MoonWalk ? CalculateInverse(Rotation) : Rotation);
        }

        public static int Calculate(Vector2 PositionOne, Vector2 PositionTwo)
        {
            if (PositionOne.X > PositionTwo.X && PositionOne.Y > PositionTwo.Y)
            {
                return 7;
            }
            else if (PositionOne.X < PositionTwo.X && PositionOne.Y < PositionTwo.Y)
            {
                return 3;
            }
            else if (PositionOne.X > PositionTwo.X && PositionOne.Y < PositionTwo.Y)
            {
                return 5;
            }
            else if (PositionOne.X < PositionTwo.X && PositionOne.Y > PositionTwo.Y)
            {
                return 1;
            }
            else if (PositionOne.X > PositionTwo.X)
            {
                return 6;
            }
            else if (PositionOne.X < PositionTwo.X)
            {
                return 2;
            }
            else if (PositionOne.Y < PositionTwo.Y)
            {
                return 4;
            }
            else if (PositionOne.Y > PositionTwo.Y)
            {
                return 0;
            }

            return 0;
        }

        public static int CalculateInverse(int Rotation)
        {
            if (Rotation > 3)
            {
                Rotation -= 4;
            }
            else
            {
                Rotation += 4;
            }

            return Rotation;
        }
    }
}
