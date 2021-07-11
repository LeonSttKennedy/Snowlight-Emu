using Snowlight.Specialized;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snowlight.Game.Pathfinding
{
    public sealed class PathFinderNode : IComparable<PathFinderNode>
    {
        public Vector2 Position;
        public PathFinderNode Next;
        public int Cost = Int32.MaxValue;
        public bool InOpen = false;
        public bool InClosed = false;

        public PathFinderNode(Vector2 Position)
        {
            this.Position = Position;
        }

        public override bool Equals(object obj)
        {
            return (obj is PathFinderNode) && ((PathFinderNode)obj).Position.Equals(this.Position);
        }

        public bool Equals(PathFinderNode Breadcrumb)
        {
            return Breadcrumb.Position.Equals(this.Position);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }

        public int CompareTo(PathFinderNode Other)
        {
            return Cost.CompareTo(Other.Cost);
        }
    }
}
