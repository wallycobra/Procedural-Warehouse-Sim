using System.Collections.Generic;
using UnityEngine;

public class WarehouseGrid
{
    public int Width { get; }
    public int Height { get; }

    private readonly GridNode[,] nodes;

    public WarehouseGrid(int width, int height)
    {
        Width = width;
        Height = height;

        nodes = new GridNode[width, height];

        InitializeNodes();
    }

    private void InitializeNodes()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                nodes[x, z] = new GridNode(
                    x,
                    z,
                    CellType.Floor);
            }
        }
    }

    public bool IsInBounds(int x, int z)
    {
        return x >= 0 &&
               x < Width &&
               z >= 0 &&
               z < Height;
    }

    public GridNode GetNode(int x, int z)
    {
        if (!IsInBounds(x, z))
        {
            return null;
        }

        return nodes[x, z];
    }

    public void SetCellType(int x, int z, CellType type)
    {
        if (!IsInBounds(x, z))
        {
            return;
        }

        nodes[x, z].SetCellType(type);
    }

    public List<GridNode> GetTraversableNeighbors(GridNode node)
    {
        List<GridNode> neighbors = new();

        Vector2Int[] directions =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        foreach (Vector2Int direction in directions)
        {
            int nextX = node.X + direction.x;
            int nextZ = node.Z + direction.y;

            if (!IsInBounds(nextX, nextZ))
            {
                continue;
            }

            GridNode neighbor = nodes[nextX, nextZ];

            if (neighbor.Traversable)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }
    
    /// HELPERS
    public List<GridNode> GetNodesOfType(CellType type)
    {
        List<GridNode> results = new();

        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                if (nodes[x, z].CellType == type)
                {
                    results.Add(nodes[x, z]);
                }
            }
        }

        return results;
    }

    public GridNode GetAdjacentPathNode(GridNode node)
    {
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (Vector2Int direction in directions)
        {
            GridNode neighbor = GetNode(node.X + direction.x, node.Z + direction.y);

            if (neighbor != null && neighbor.CellType == CellType.Path)
            {
                return neighbor;
            }
        }

        return null;
    }
}