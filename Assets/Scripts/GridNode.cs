using UnityEngine;

public class GridNode
{
    public int X { get; }
    public int Z { get; }

    public CellType CellType { get; private set; }

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
}