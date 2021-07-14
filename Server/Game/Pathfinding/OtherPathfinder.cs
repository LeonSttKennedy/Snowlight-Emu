using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Snowlight.Game.Rooms;
using Snowlight.Specialized;

namespace Snowlight.Game.Pathfinding
{
    public class ComplexPathfinder : Pathfinder
    {
        private RoomInstance mCurrentInstance;
        private uint mActorId;
        private List<Vector2> mPath;
        private Vector2 mTarget;

        public override Vector2 Target
        {
            get
            {
                return mTarget;
            }
        }

        public override bool IsCompleted
        {
            get
            {
                return (mPath.Count == 0);
            }
        }

        public override void SetRoomInstance(RoomInstance Room, uint ActorId)
        {
            mCurrentInstance = Room;
            mActorId = ActorId;
            mPath = new List<Vector2>();
            mTarget = null;
        }

        public override void MoveTo(Vector2 Position)
        {
            lock (mPath)
            {
                mPath.Clear();
                mTarget = Position;
                mPath = FindPath();
            }
        }

        public override void Clear()
        {
            lock (mPath)
            {
                mPath.Clear();
                mTarget = null;
            }
        }

        public override Vector2 GetNextStep()
        {
            lock (mPath)
            {
                if (IsCompleted)
                {
                    return null;
                }

                Vector2 NextStep = mPath[0];
                mPath.Remove(NextStep);
                return NextStep;
            }
        }

        private List<Vector2> FindPath()
        {
            if (mCurrentInstance == null || mTarget == null)
            {
                return null;
            }

            RoomActor Actor = mCurrentInstance.GetActor(mActorId);

            if (Actor == null)
            {
                return null;
            }

            List<Vector2> Path = new List<Vector2>();

            PathFinderNode Nodes = FindPathReversed(mCurrentInstance.Model, mTarget, new Vector2(Actor.Position.X, Actor.Position.Y), mCurrentInstance.DisableDiagonal);

            if (Nodes != null) // make sure we do have a path first
            {
                while (Nodes.Next != null)
                {
                    Path.Add(Nodes.Next.Position);
                    Nodes = Nodes.Next;
                }
            }

            // I need to change 'IsValidStep' to not count the position the user is on (the user who wants to walk) or the emulator will error..

            return Path;
        }

        private PathFinderNode FindPathReversed(RoomModel map, Vector2 start, Vector2 end, bool diag)
        {
            MinHeap<PathFinderNode> OpenList = new MinHeap<PathFinderNode>(256);

            PathFinderNode[,] Map = new PathFinderNode[map.Heightmap.SizeX, map.Heightmap.SizeY];
            PathFinderNode Node;
            Vector2 Tmp;
            int Cost;
            int Diff;

            PathFinderNode Current = new PathFinderNode(start);
            Current.Cost = 0;

            PathFinderNode Finish = new PathFinderNode(end);
            Map[Current.Position.X, Current.Position.Y] = Current;
            OpenList.Add(Current);

            while (OpenList.Count > 0)
            {
                Current = OpenList.ExtractFirst();
                Current.InClosed = true;

                for (int i = 0; diag ? i < MovePoints.Length : i < NoDiagMovePoints.Length; i++)
                {
                    Tmp = Current.Position + (diag ? MovePoints[i] : NoDiagMovePoints[i]);
                    bool IsFinalMove = (Tmp.X == end.X && Tmp.Y == end.Y); // are we at the final position?

                    if (mCurrentInstance.IsValidStep(new Vector2(Current.Position.X, Current.Position.Y), Tmp, IsFinalMove, new List<RoomActor>[map.Heightmap.SizeX, map.Heightmap.SizeY])) // need to set the from positions
                    {
                        if (Map[Tmp.X, Tmp.Y] == null)
                        {
                            Node = new PathFinderNode(Tmp);
                            Map[Tmp.X, Tmp.Y] = Node;
                        }
                        else
                        {
                            Node = Map[Tmp.X, Tmp.Y];
                        }

                        if (!Node.InClosed)
                        {
                            Diff = 0;

                            if (Current.Position.X != Node.Position.X)
                            {
                                Diff += 1;
                            }

                            if (Current.Position.Y != Node.Position.Y)
                            {
                                Diff += 1;
                            }

                            Cost = Current.Cost + Diff + Node.Position.GetDistanceSquared(end);

                            if (Cost < Node.Cost)
                            {
                                Node.Cost = Cost;
                                Node.Next = Current;
                            }

                            if (!Node.InOpen)
                            {
                                if (Node.Equals(Finish))
                                {
                                    Node.Next = Current;
                                    return Node;
                                }

                                Node.InOpen = true;
                                OpenList.Add(Node);
                            }
                        }
                    }
                }
            }

            return null;
        }

        private static Vector2[] MovePoints = new Vector2[]
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

        private static Vector2[] NoDiagMovePoints = new Vector2[]
        {
            new Vector2(0, -1),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(-1, 0)
        };
    }
}