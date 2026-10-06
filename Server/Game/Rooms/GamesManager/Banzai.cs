using Microsoft.VisualBasic.Devices;
using Snowlight.Game.Achievements;
using Snowlight.Game.Items;
using Snowlight.Game.Misc;
using Snowlight.Game.Quests;
using Snowlight.Game.Sessions;
using Snowlight.Specialized;
using Snowlight.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Game.Rooms
{
    public enum BanzaiSphere
    {
        PowerOff = 0,
        PowerOn = 1,
        BlinkLights = 2,
        BlinkLedsCentre = 3,

        RedLights = 10,
        GreenLights = 11,
        BlueLights = 12,
        YellowLights = 13,

        RedWins = 20,
        GreenWins = 21,
        BlueWins = 22,
        YellowWins = 23
    }

    public enum BanzaiTileStatus
    {
        PowerOff = 0,
        PowerOn = 1,
        ClearTile = 2,      // We don't use this

        StateRed1 = 3,
        StateRed2 = 4,
        StateRed3 = 5,

        StateGreen1 = 6,
        StateGreen2 = 7,
        StateGreen3 = 8,

        StateBlue1 = 9,
        StateBlue2 = 10,
        StateBlue3 = 11,

        StateYellow1 = 12,
        StateYellow2 = 13,
        StateYellow3 = 14,

        ClearTile2 = 15     // We don't use this
    }

    public partial class GameManager
    {
        private uint[,] mBanzaiGameField;
        private uint[,] mBanzaiGameConnectedTiles;
        private Dictionary<uint, int> mBanzaiHelpBlinkCount;
        private Dictionary<uint, string> mBanzaiHelpFlagsHolder;

        public bool BanzaiStarted
        {
            get
            {
                return mGameStarted.ContainsKey(GameType.BattleBanzai) ? mGameStarted[GameType.BattleBanzai] : false;
            }
        }

        public Dictionary<uint, int> BanzaiHelpBlinkCount
        {
            get
            {
                return mBanzaiHelpBlinkCount;
            }

            set
            {
                mBanzaiHelpBlinkCount = value;
            }
        }

        public Dictionary<uint, string> BanzaiHelpStateHolder
        {
            get
            {
                return mBanzaiHelpFlagsHolder;
            }

            set
            {
                mBanzaiHelpFlagsHolder = value;
            }
        }

        public void GenerateGameField()
        {
            List<Item> BanzaiTileItems = mRoomInstance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.BanzaiTile).ToList();

            foreach(Item Item in BanzaiTileItems)
            {
                Vector3 Position = Item.RoomPosition;

                mBanzaiGameField[Position.X, Position.Y] = Item.Id;
            }
        }

        public void HandleBanzaiTileLock(RoomActor Actor, Item Item, int Flags)
        {
            Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
            {
                if (Session.CharacterInfo.HasLinkedSession)
                {
                    AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_BattleBallTilesLocked", 1);
                    QuestManager.ProgressUserQuest(Session, QuestType.BANZAI_LOCK_TILE);
                }
            }

            ChangeBanzaiScoreState(1, Actor);

            HandleConnectedTiles(Item, Actor, Flags);

            ForceStopGame(GameType.BattleBanzai);
        }

        public TeamColors BanzaiTileColor(int Flags)
        {
            return (TeamColors)(((Flags - 3) / 3) + 1);
        }

        public void UpdateBanzaiSphere(BanzaiSphere Flags)
        {
            List<Item> SphereItems = mRoomInstance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.BanzaiSphere).ToList();

            foreach (Item Item in SphereItems)
            {
                if (Flags == BanzaiSphere.PowerOff)
                {
                    Item.InteractHelper = 0;
                }

                if (mBanzaiHelpBlinkCount.ContainsKey(Item.Id))
                {
                    mBanzaiHelpBlinkCount.Remove(Item.Id);
                }

                if (mBanzaiHelpFlagsHolder.ContainsKey(Item.Id))
                {
                    mBanzaiHelpFlagsHolder.Remove(Item.Id);
                }

                ItemEventDispatcher.InvokeItemEventHandler(null, Item, mRoomInstance, ItemEventType.ItemInvoking, (int)Flags);
            }
        }

        /// <summary>
        /// Update Banzai Tile State invoked by Another items
        /// </summary>
        public void UpdateBanzaiTileState(BanzaiTileStatus Flags)
        {
            List<Item> TileItems = mRoomInstance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.BanzaiTile).ToList();

            foreach (Item Item in TileItems)
            {
                if (Flags == BanzaiTileStatus.PowerOff ||
                    Flags == BanzaiTileStatus.PowerOn && Item.InteractHelper != 0)
                {
                    Item.InteractHelper = 0;
                }

                if (mBanzaiHelpBlinkCount.ContainsKey(Item.Id))
                {
                    mBanzaiHelpBlinkCount.Remove(Item.Id);
                }

                if (mBanzaiHelpFlagsHolder.ContainsKey(Item.Id))
                {
                    mBanzaiHelpFlagsHolder.Remove(Item.Id);
                }

                ItemEventDispatcher.InvokeItemEventHandler(null, Item, mRoomInstance, ItemEventType.ItemInvoking, (int)Flags);
            }
        }

        /// <summary>
        /// Update Banzai Tile State invoked by Banzai Puck
        /// </summary>
        public void UpdateBanzaiTileState(RoomActor Actor, Vector2 RoomPosition)
        {
            if (!BanzaiStarted)
            {
                return;
            }

            List<Item> ItemsOnBanzaiPuckPosition = mRoomInstance.GetItemsOnPosition(RoomPosition).Where(I => I.Definition.Behavior == ItemBehavior.BanzaiTile).ToList();

            if (ItemsOnBanzaiPuckPosition.Count > 0)
            {
                Item BanzaiTile = ItemsOnBanzaiPuckPosition.FirstOrDefault();

                if (BanzaiTile != null)
                {
                    if (BanzaiTile.InteractHelper < 3)
                    {
                        BanzaiTile.InteractHelper++;

                        int NewState = BanzaiTile.InteractHelper + ((int)Actor.PlayerTeamColor * 3) - 1;
                        ItemEventDispatcher.InvokeItemEventHandler(null, BanzaiTile, mRoomInstance, ItemEventType.ItemInvoking, NewState);

                        if (BanzaiTile.InteractHelper == 3)
                        {
                            HandleBanzaiTileLock(Actor, BanzaiTile, NewState);
                        }
                    }
                }
            }
        }

        public void ChangeBanzaiScoreState(int Request, RoomActor Actor = null)
        {
            List<Item> RoomScoreboardItems = mRoomInstance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.BanzaiScore).ToList();

            switch (Request)
            {
                default:

                    if (RoomScoreboardItems.Count > 0)
                    {
                        List<Item> TeamItems = RoomScoreboardItems.Where(I => (TeamColors)I.Definition.BehaviorData == Actor.PlayerTeamColor).ToList();

                        foreach (Item TeamItem in TeamItems)
                        {
                            ItemEventDispatcher.InvokeItemEventHandler(null, TeamItem, mRoomInstance, ItemEventType.ItemInvoking, Request);
                        }
                    }

                    break;

                case 0:

                    foreach (TeamColors Colors in Enum.GetValues(typeof(TeamColors)))
                    {
                        List<Item> TeamItems = RoomScoreboardItems.Where(I => (TeamColors)I.Definition.BehaviorData == Colors).ToList();

                        foreach(Item TeamItem in TeamItems)
                        {
                            ItemEventDispatcher.InvokeItemEventHandler(null, TeamItem, mRoomInstance, ItemEventType.ItemInvoking);
                        }
                    }

                    break;
            }
        }

        public void HandleConnectedTiles(Item Item, RoomActor Actor, int Flags)
        {
            mBanzaiGameConnectedTiles[Item.RoomPosition.X, Item.RoomPosition.Y] = Item.Id;

            List<Item> ConnectedTiles = FindConnection(mBanzaiGameConnectedTiles, Item);

            if (ConnectedTiles.Count > 4)
            {
                List<Item> InsideConnectedTiles = FindPointsInside(mBanzaiGameField, ConnectedTiles);

                if (InsideConnectedTiles.Count > 0)
                {
                    ChangeBanzaiScoreState(InsideConnectedTiles.Count, Actor);

                    using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                    {
                        Session Session = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);

                        if (Session != null)
                        {
                            AchievementManager.ProgressUserAchievement(MySqlClient, Session, "ACH_BattleBallTilesLocked", InsideConnectedTiles.Count);
                            QuestManager.ProgressUserQuest(Session, QuestType.BANZAI_LOCK_TILE, (uint)InsideConnectedTiles.Count);
                            QuestManager.ProgressUserQuest(Session, QuestType.BANZAI_LOCK_TILES, (uint)InsideConnectedTiles.Count);
                        }
                    }

                    foreach (Item InsideItem in InsideConnectedTiles)
                    {
                        InsideItem.InteractHelper = 3;
                        ItemEventDispatcher.InvokeItemEventHandler(null, InsideItem, mRoomInstance, ItemEventType.ItemInvoking, Flags);
                    }

                    List<Item> BanzaiSphereItems = mRoomInstance.GetFloorItems().Where(I => I.Definition.Behavior == ItemBehavior.BanzaiSphere).ToList();

                    if (BanzaiSphereItems.Count > 0)
                    {
                        int UpdateColor = (int)Actor.PlayerTeamColor + 9;
                        foreach (Item SphereItem in BanzaiSphereItems)
                        {
                            ItemEventDispatcher.InvokeItemEventHandler(null, SphereItem, mRoomInstance, ItemEventType.UpdateTick, UpdateColor);
                        }
                    }
                }
            }
        }

        public List<Item> FindConnection(uint[,] Grid, Item Item)
        {
            int GridWidth = Grid.GetLength(0);
            int GridHeight = Grid.GetLength(1);

            var ConnectedPoints = new List<Item>();
            var VisitedPoints = new HashSet<(int X, int Y)>();

            int[,] directions =
            {
                { -1, -1 },
                {  0, -1 },
                {  1, -1 },
                { -1,  0 },
                {  1,  0 },
                { -1,  1 },
                {  0,  1 },
                {  1,  1 }
            };

            if (Item == null)
            {
                return ConnectedPoints;
            }

            int.TryParse(Item.Flags, out int StartItemState);

            TeamColors StartItemColor = BanzaiTileColor(StartItemState);

            if (Item.InteractHelper != 3 ||
                StartItemColor == TeamColors.None)
            {
                return ConnectedPoints;
            }

            var Queue = new Queue<Item>();

            Queue.Enqueue(Item);

            int X = Item.RoomPosition.X;
            int Y = Item.RoomPosition.Y;

            VisitedPoints.Add((X, Y));

            while (Queue.Count > 0)
            {
                Item CurrentPoint = Queue.Dequeue();

                ConnectedPoints.Add(CurrentPoint);

                for (int i = 0; i < 8; i++)
                {
                    int NewX = CurrentPoint.RoomPosition.X + directions[i, 0];
                    int NewY = CurrentPoint.RoomPosition.Y + directions[i, 1];

                    if (NewX < 0 || NewX >= GridWidth ||
                        NewY < 0 || NewY >= GridHeight)
                    {
                        continue;
                    }

                    if (VisitedPoints.Contains((NewX, NewY)))
                    {
                        continue;
                    }

                    Item Neighbor = mRoomInstance.GetItem(Grid[NewX, NewY]);

                    if (Neighbor != null)
                    {
                        int.TryParse(Neighbor.Flags, out int NeighborState);

                        TeamColors NeighborItemColor = BanzaiTileColor(NeighborState);

                        if (Neighbor.InteractHelper != 3)
                        {
                            continue;
                        }

                        if (NeighborItemColor != StartItemColor)
                        {
                            continue;
                        }

                        VisitedPoints.Add((NewX, NewY));

                        Queue.Enqueue(Neighbor);
                    }
                }
            }

            return ConnectedPoints;
        }

        public List<Item> FindPointsInside(uint[,] Grid, List<Item> ConnectedPoints)
        {
            var InsideItems = new List<Item>();

            if (Grid == null ||
                ConnectedPoints == null ||
                ConnectedPoints.Count == 0)
            {
                return InsideItems;
            }

            int GridWidth = Grid.GetLength(0);
            int GridHeight = Grid.GetLength(1);

            var ConnectionPositions = ConnectedPoints.Select(I => (I.RoomPosition.X, I.RoomPosition.Y)).ToHashSet();

            var Visited = new HashSet<(int X, int Y)>();

            var Queue = new Queue<(int X, int Y)>();

            int[,] Directions =
            {
                { -1,  0 },
                {  1,  0 },
                {  0, -1 },
                {  0,  1 }
            };

            for (int x = 0; x < GridWidth; x++)
            {
                AddExteriorPoint(x, 0, Grid, ConnectionPositions, Visited, Queue);

                AddExteriorPoint(x, GridHeight - 1, Grid, ConnectionPositions, Visited, Queue);
            }

            for (int y = 0; y < GridHeight; y++)
            {
                AddExteriorPoint(0, y, Grid, ConnectionPositions, Visited, Queue);

                AddExteriorPoint(GridWidth - 1, y, Grid, ConnectionPositions, Visited, Queue);
            }

            while (Queue.Count > 0)
            {
                var Current = Queue.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    int NewX = Current.X + Directions[i, 0];
                    int NewY = Current.Y + Directions[i, 1];

                    if (NewX < 0 || NewX >= GridWidth ||
                        NewY < 0 || NewY >= GridHeight)
                    {
                        continue;
                    }

                    if (Visited.Contains((NewX, NewY)))
                    {
                        continue;
                    }

                    if (ConnectionPositions.Contains((NewX, NewY)))
                    {
                        continue;
                    }

                    Visited.Add((NewX, NewY));

                    Queue.Enqueue((NewX, NewY));
                }
            }

            var AddedPositions = new HashSet<(int X, int Y)>();
            var AddedItemIds = new HashSet<uint>();

            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    var Position = (X: x, Y: y);

                    if (ConnectionPositions.Contains(Position))
                    {
                        continue;
                    }

                    if (Visited.Contains(Position))
                    {
                        continue;
                    }

                    if (!AddedPositions.Add(Position))
                    {
                        continue;
                    }

                    Item CurrentItem = mRoomInstance.GetItem(Grid[x, y]);

                    if (CurrentItem == null)
                    {
                        continue;
                    }

                    if (!AddedItemIds.Add(CurrentItem.Id))
                    {
                        continue;
                    }

                    if (CurrentItem.InteractHelper < 3)
                    {
                        InsideItems.Add(CurrentItem);
                    }
                }
            }

            return InsideItems;
        }

        private void AddExteriorPoint(int x, int y, uint[,] Grid, HashSet<(int X, int Y)> ConnectionPositions, HashSet<(int X, int Y)> Visited, Queue<(int X, int Y)> Queue)
        {
            if (x < 0 || x >= Grid.GetLength(0) ||
                y < 0 || y >= Grid.GetLength(1))
            {
                return;
            }

            if (ConnectionPositions.Contains((x, y)))
            {
                return;
            }

            if (Visited.Contains((x, y)))
            {
                return;
            }

            Visited.Add((x, y));

            Queue.Enqueue((x, y));
        }
    }
}
