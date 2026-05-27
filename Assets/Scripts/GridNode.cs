using UnityEngine;

public class GridNode
{
    public int X { get; }
    public int Z { get; }

    public CellType CellType { get; private set; }

    // Set when creating grid in WarehouseGrid.cs //
    public GridNode North { get; set; }
    public GridNode South { get; set; }
    public GridNode East { get; set; }
    public GridNode West { get; set; }
    ///////
    public bool HasNorthPath => North?.Traversable == true;
    public bool HasSouthPath => South?.Traversable == true;
    public bool HasEastPath => East?.Traversable == true;
    public bool HasWestPath => West?.Traversable == true;
    public bool Traversable =>
        CellType == CellType.Path;

    public bool IsRobotSpawnable =>
        CellType == CellType.Parking;

    public float MovementCost { get; set; } = 1f;

    public GameObject VisualObject { get; set; }

    public GridNode(int x, int z, CellType cellType)
    {
        X = x;
        Z = z;
        CellType = cellType;
    }

    public void SetCellType(CellType type)
    {
        CellType = type;
    }

    public Vector3 GetWorldPosition(float cellSize)
    {
        return new Vector3(
            X * cellSize,
            0f,
            Z * cellSize);
    }

    public PathShape GetPathShape()
    {
        if (CellType != CellType.Path)
        {
            return PathShape.None;
        }

        bool north = North?.Traversable == true;
        bool south = South?.Traversable == true;
        bool east = East?.Traversable == true;
        bool west = West?.Traversable == true;

        int connectionCount = 0;

        if (north) connectionCount++;
        if (south) connectionCount++;
        if (east) connectionCount++;
        if (west) connectionCount++;

        if (connectionCount == 1)
        {
            return PathShape.DeadEnd;
        }

        if (connectionCount == 2)
        {
            bool isStraight =
                north && south ||
                east && west;

            return isStraight
                ? PathShape.Straight
                : PathShape.Corner;
        }

        if (connectionCount == 3)
        {
            return PathShape.ThreeWayIntersection;
        }

        if (connectionCount == 4)
        {
            return PathShape.FourWayIntersection;
        }

        return PathShape.None;
    }
    
}