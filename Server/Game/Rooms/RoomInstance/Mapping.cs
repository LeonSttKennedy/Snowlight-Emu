﻿using System;
using System.Text;
using System.Collections.Generic;

using Snowlight.Specialized;
using Snowlight.Game.Items;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Rooms
{
    public enum UserMovementNode
    {
        Blocked = 0,
        Walkable = 1,
        WalkableEndOfRoute = 2
    }

    public partial class RoomInstance : IDisposable
    {
        private RoomModel mCachedModel;
        private string mRelativeHeightmap;
        private TileState[,] mTileStates;
        private List<RoomActor>[,] mUserGrid;
        private UserMovementNode[,] mUserMovementNodes;
        private uint[,] mFurniMap;

        public string RelativeHeightmap
        {
            get
            {
                return mRelativeHeightmap;
            }
        }

        public RoomModel Model
        {
            get
            {
                return mCachedModel;
            }
        }

        public List<RoomActor>[,] RoomActorGrid
        {
            get
            {
                return mUserGrid;
            }
        }

        public uint[,] FurniMap
        {
            get
            {
                return mFurniMap;
            }
        }

        public Vector2 GetRedirectedTarget(Vector2 Target)
        {
            return (mRedirectGrid[Target.X, Target.Y] != null ? mRedirectGrid[Target.X, Target.Y] : Target);
        }
        public bool IsValidPosition(Vector2 Position)
        {
            return (Position.X >= 0 && Position.Y >= 0 && Position.X < mCachedModel.Heightmap.SizeX &&
                Position.Y < mCachedModel.Heightmap.SizeY);
        }

        public double GetUserStepHeight(Vector2 Position)
        {
            RoomTileEffect Effect = mTileEffects[Position.X, Position.Y];

            double Height = mStackHeight[Position.X, Position.Y];

            if (Effect != null && Effect.InteractionHeight >= 0)
            {
                Height -= mStackTopItemHeight[Position.X, Position.Y];
            }

            return Height;
        }

        public bool CanInitiateMoveToPosition(Vector2 To)
        {
            return (IsValidPosition(To) && mTileStates[To.X, To.Y] != TileState.Blocked && mUserGrid[To.X, To.Y] == null
                && mUserMovementNodes[To.X, To.Y] != UserMovementNode.Blocked);
        }

        public bool IsValidStep(Vector2 From, Vector2 To, bool EndOfPath, bool EnableClipping = false, List<RoomActor>[,] ActorBlockGrid = null)
        {
            if (ActorBlockGrid == null)
            {
                ActorBlockGrid = mUserGrid;
            }

            if(!IsValidPosition(To))
            {
                return false;
            }

            double HeightDiff = GetUserStepHeight(From) - GetUserStepHeight(To);

            if (EnableClipping)
            {
                return true;
            }

            if (mTileStates[To.X, To.Y] == TileState.Blocked || 
                ((!Info.DisableRoomBlocking || EndOfPath) && ActorBlockGrid[From.X, From.Y] != null) ||
                mUserMovementNodes[To.X, To.Y] == UserMovementNode.Blocked ||
                (mUserMovementNodes[To.X, To.Y] == UserMovementNode.WalkableEndOfRoute && !EndOfPath) ||
                HeightDiff > 1.5 || (Info.Type == RoomType.Public && HeightDiff < -1.5))
            {
                return false;
            }

            return true;
        }

        public List<RoomActor> GetActorsOnPosition(Vector2 Position)
        {
            return mUserGrid[Position.X, Position.Y] == null ? new List<RoomActor>() : mUserGrid[Position.X, Position.Y];
        }

        /// <summary>
        /// (Re)generates the relative heightmap for this room. Neccessary after furniture updates.
        /// </summary>
        public void RegenerateRelativeHeightmap(bool Broadcast = false)
        {
            lock (mItems)
            {
                mGuestsCanPlaceStickies = false;

                mFurniMap = new uint[mCachedModel.Heightmap.SizeX, mCachedModel.Heightmap.SizeY];
                mStackHeight = new double[mCachedModel.Heightmap.SizeX, mCachedModel.Heightmap.SizeY];
                mStackTopItemHeight = new double[mCachedModel.Heightmap.SizeX, mCachedModel.Heightmap.SizeY];
                mUserMovementNodes = new UserMovementNode[mCachedModel.Heightmap.SizeX, mCachedModel.Heightmap.SizeY];
                mTileEffects = new RoomTileEffect[mCachedModel.Heightmap.SizeX, mCachedModel.Heightmap.SizeY];
                mTileTriggers = new RoomTriggers[mCachedModel.Heightmap.SizeX, mCachedModel.Heightmap.SizeY];
                mRedirectGrid = new Vector2[mCachedModel.Heightmap.SizeX, mCachedModel.Heightmap.SizeY];

                for (int y = 0; y < mCachedModel.Heightmap.SizeY; y++)
                {
                    for (int x = 0; x < mCachedModel.Heightmap.SizeX; x++)
                    {
                        mStackHeight[x, y] = mCachedModel.Heightmap.FloorHeight[x, y];
                        mStackTopItemHeight[x, y] = 0;
                        mUserMovementNodes[x, y] = UserMovementNode.Walkable;
                        mTileEffects[x, y] = new RoomTileEffect();
                        mRedirectGrid[x, y] = null;
                    }
                }

                bool[,] AnyItemInStack = new bool[mCachedModel.Heightmap.SizeX, mCachedModel.Heightmap.SizeY];

                foreach (Item Item in mItems.Values)
                {
                    if (Item.Definition.StackingBehavior == ItemStackingBehavior.Ignore)
                    {
                        continue;
                    }

                    double TotalItemStackHeight = Item.RoomPosition.Z + Item.ActiveHeight;
                    List<Vector2> AffectedTiles = CalculateAffectedTiles(Item, Item.RoomPosition.GetVector2(), Item.RoomRotation);

                    RoomTileEffect Effect = new RoomTileEffect();
                    UserMovementNode MovementNode = UserMovementNode.Blocked;

                    switch (Item.Definition.Behavior)
                    {
                        case ItemBehavior.Seat:
                        case ItemBehavior.LoveShuffler:

                            Effect = new RoomTileEffect(RoomTileEffectType.Sit, Item.RoomRotation,
                                new Vector2(Item.RoomPosition.X, Item.RoomPosition.Y), Item.ActiveHeight, 0,
                                Item.DefinitionId);
                            MovementNode = UserMovementNode.WalkableEndOfRoute;
                            break;

                        case ItemBehavior.Bed:

                            Effect = new RoomTileEffect(RoomTileEffectType.Lay, Item.RoomRotation,
                                new Vector2(Item.RoomPosition.X, Item.RoomPosition.Y), Item.ActiveHeight, 0,
                                Item.DefinitionId);
                            MovementNode = UserMovementNode.WalkableEndOfRoute;
                            break;

                        case ItemBehavior.Gate:

                            MovementNode = Item.Flags == "1" ? UserMovementNode.Walkable : UserMovementNode.Blocked;
                            break;

                        case ItemBehavior.AvatarEffectGenerator:

                            Effect = new RoomTileEffect(RoomTileEffectType.Effect, 0, Item.RoomPosition.GetVector2(), 0,
                                Item.Definition.BehaviorData, Item.DefinitionId);
                            break;

                        case ItemBehavior.StickyPole:

                            mGuestsCanPlaceStickies = true;
                            break;
                    }

                    foreach (Vector2 Tile in AffectedTiles)
                    {
                        if (TotalItemStackHeight >= mStackHeight[Tile.X, Tile.Y])
                        {
                            if (Item.Definition.Walkable == ItemWalkableMode.Always)
                            {
                                MovementNode = UserMovementNode.Walkable;
                            }
                            else if (Item.Definition.Walkable == ItemWalkableMode.Limited)
                            {
                                MovementNode = (AnyItemInStack[Tile.X, Tile.Y] && mUserMovementNodes[Tile.X, Tile.Y] != UserMovementNode.Walkable) ? UserMovementNode.Blocked : UserMovementNode.Walkable;
                            }

                            mStackHeight[Tile.X, Tile.Y] = TotalItemStackHeight;
                            mStackTopItemHeight[Tile.X, Tile.Y] = Item.Definition.Height;
                            mUserMovementNodes[Tile.X, Tile.Y] = MovementNode;
                            mTileEffects[Tile.X, Tile.Y] = Effect;
                            mFurniMap[Tile.X, Tile.Y] = Item.Id;

                            if (Item.Definition.Behavior == ItemBehavior.Bed)
                            {
                                if (Item.RoomRotation == 2 || Item.RoomRotation == 6)
                                {
                                    mRedirectGrid[Tile.X, Tile.Y] = new Vector2(Item.RoomPosition.X, Tile.Y);
                                }

                                if (Item.RoomRotation == 0 || Item.RoomRotation == 4)
                                {
                                    mRedirectGrid[Tile.X, Tile.Y] = new Vector2(Tile.X, Item.RoomPosition.Y);
                                }
                            }
                            else
                            {
                                mRedirectGrid[Tile.X, Tile.Y] = null;
                            }

                            AnyItemInStack[Tile.X, Tile.Y] = true;
                        }
                    }
                }

                foreach (StaticObject Object in mStaticObjects)
                {
                    double TotalItemStackHeightOld = mCachedModel.Heightmap.FloorHeight[Object.Position.X, Object.Position.Y] + Object.Height;
                    double TotalItemStackHeight = Object.Position.Z + Object.Height;

                    List<Vector2> AffectedTiles = CalculateAffectedTiles(Object, Object.Position.GetVector2(), Object.Rotation);
                    UserMovementNode MovimentNode = UserMovementNode.Blocked;
                    
                    if (Object.Walkable)
                    {
                        MovimentNode = UserMovementNode.Walkable;
                    }
                    else if (Object.IsSeat)
                    {
                        MovimentNode = UserMovementNode.WalkableEndOfRoute;
                    }

                    foreach (Vector2 Tile in AffectedTiles)
                    {
                        if (TotalItemStackHeight >= mStackHeight[Tile.X, Tile.Y])
                        {
                            mStackHeight[Tile.X, Tile.Y] = TotalItemStackHeight;
                            mStackTopItemHeight[Tile.X, Tile.Y] = Object.Height;
                            mUserMovementNodes[Tile.X, Tile.Y] = MovimentNode;
                            mFurniMap[Tile.X, Tile.Y] = Object.Id;

                            if (Object.IsSeat)
                            {
                                mTileEffects[Tile.X, Tile.Y] = new RoomTileEffect(RoomTileEffectType.Sit, Object.Rotation, Object.Position.GetVector2(), Object.Height);
                            }
                        }
                    }
                }

                foreach (RoomTriggers Triggers in mRoomTriggers)
                {
                    mTileTriggers[Triggers.RoomPosition.X, Triggers.RoomPosition.Y] = new RoomTriggers(Triggers.Id, Triggers.RoomPosition, Triggers.Action, Triggers.ToRoomPosition, Triggers.ToRoomId, Triggers.ToRoomRotation);
                }

                lock (mTileStates)
                {
                    StringBuilder RelativeHeightmap = new StringBuilder();
                    mTileStates = new TileState[mCachedModel.Heightmap.SizeX, mCachedModel.Heightmap.SizeY];

                    for (int y = 0; y < mCachedModel.Heightmap.SizeY; y++)
                    {
                        for (int x = 0; x < mCachedModel.Heightmap.SizeX; x++)
                        {
                            if (mInfo.Type == RoomType.Flat && mCachedModel.DoorPosition.X == x && mCachedModel.DoorPosition.Y == y)
                            {
                                mTileStates[x, y] = TileState.Door;
                                RelativeHeightmap.Append(mCachedModel.DoorPosition.Z);
                                continue;
                            }

                            if (mCachedModel.Heightmap.TileStates[x, y] == TileState.Blocked)
                            {
                                mTileStates[x, y] = TileState.Blocked;
                                RelativeHeightmap.Append('x');
                                continue;
                            }

                            mTileStates[x, y] = TileState.Open;
                            RelativeHeightmap.Append(mCachedModel.Heightmap.FloorHeight[x, y]);
                        }

                        RelativeHeightmap.Append(Convert.ToChar(13));
                    }

                    lock (mRelativeHeightmap)
                    {
                        string NewRelativeMap = RelativeHeightmap.ToString();

                        if (NewRelativeMap != mRelativeHeightmap)
                        {
                            mRelativeHeightmap = RelativeHeightmap.ToString();

                            if (Broadcast)
                            {
                                BroadcastMessage(RoomRelativeHeightmapComposer.Compose(mRelativeHeightmap));
                            }
                        }
                    }
                }
            }
        }
    }
}
