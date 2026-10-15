using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Snowlight.Specialized;
using Snowlight.Game.Rooms;
using Snowlight.Game.Items;

namespace Snowlight.Game.Misc
{
    public static class WaterBorderHelper
    {
        private static List<int> mClearFlags = new List<int>();

        private static Vector2[] PossiblePoints = new Vector2[]
        {
            new Vector2(-1, -1),
            new Vector2(0, -1),
            new Vector2(1, -1),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(-1, 1),
            new Vector2(-1, 0)
        };

        public static string GetFlagsString(RoomInstance Instance, Item Item)
        {
            GenerateRawFlags();

            List<Vector2> OrderedListAffectedTiles = GetAffectedShoreTiles(Instance, Item);

            for (int i = 0; i < mClearFlags.Count; i++)
            {
                Vector2 CheckTile = OrderedListAffectedTiles[i];

                if (!Instance.CanMoveItem(CheckTile))
                {
                    mClearFlags[i] = 1;
                }
            }

            NormalizeFlags(OrderedListAffectedTiles, mClearFlags);

            return string.Join("", mClearFlags);
        }

        private static void NormalizeFlags(List<Vector2> Tiles, List<int> Flags)
        {
            int Count = Math.Min(Tiles.Count, Flags.Count);

            int[] Original = Flags.Take(Count).ToArray();

            bool HasOneAt(float X, float Y)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (Tiles[i].X == X &&
                        Tiles[i].Y == Y &&
                        Original[i] == 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            for (int i = 0; i < Count; i++)
            {
                if (Original[i] != 1)
                {
                    continue;
                }

                Vector2 Position = Tiles[i];

                bool Left = HasOneAt(Position.X - 1, Position.Y);
                bool Right = HasOneAt(Position.X + 1, Position.Y);
                bool Up = HasOneAt(Position.X, Position.Y - 1);
                bool Down = HasOneAt(Position.X, Position.Y + 1);

                bool HorizontalEnd = Left != Right;

                bool VerticalEnd = Up != Down;

                if (HorizontalEnd || VerticalEnd)
                {
                    Flags[i] = 0;
                }
            }
        }

        private static List<Vector2> GetAffectedShoreTiles(RoomInstance Instance, Item Item)
        {
            List<Vector2> List = new List<Vector2>();

            List<Vector2> AffectedTiles = Instance.CalculateAffectedTiles(Item, Item.RoomPosition.GetVector2(), Item.RoomRotation);

            foreach (Vector2 Tile in AffectedTiles)
            {
                for (int i = 0; i < PossiblePoints.Length; i++)
                {
                    Vector2 AffectedPosition = Tile + PossiblePoints[i];

                    if (!List.Contains(AffectedPosition) &&
                        !AffectedTiles.Contains(AffectedPosition))
                    {
                        List.Add(AffectedPosition);
                    }
                }
            }

            return List.OrderBy(T => T.X).OrderBy(T => T.Y).ToList();
        }

        private static void GenerateRawFlags()
        {
            mClearFlags.Clear();

            List<int> List = new List<int>();

            for (int i = 0; i < 12; i++)
            {
                List.Add(0);
            }

            mClearFlags.AddRange(List);
        }
    }
}
