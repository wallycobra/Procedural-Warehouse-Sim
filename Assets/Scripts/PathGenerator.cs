using System.Collections.Generic;
using UnityEngine;

public class PathGenerator
{
    private readonly Vector2Int[] directions =
    {
        new Vector2Int(0, 2),
        new Vector2Int(0, -2),
        new Vector2Int(2, 0),
        new Vector2Int(-2, 0)
    };

    public WarehouseGrid Generate(int width, int height, float maxPathRatio = 0.33f, float straightBias = 0.75f)
    {
        WarehouseGrid grid = new WarehouseGrid(width, height);

        int totalCells = width * height;
        int maxPathCells = Mathf.FloorToInt(totalCells * maxPathRatio);

        GeneratePaths(grid, 1, 1, maxPathCells, straightBias);
        GenerateParking(grid, 3);
        return grid;
    }

    private void GeneratePaths(
    WarehouseGrid grid,
    int startX,
    int startZ,
    int maxPathCells,
    float straightBias)
    {
        Stack<PathStep> stack = new();

        GridNode start = grid.GetNode(startX, startZ);
        start.SetCellType(CellType.Path);

        int pathCount = 1;

        stack.Push(new PathStep(start, Vector2Int.zero));

        while (stack.Count > 0 && pathCount < maxPathCells)
        {
            PathStep step = stack.Peek();
            GridNode current = step.Node;

            List<Vector2Int> validDirections =
                GetValidDirections(grid, current);

            if (validDirections.Count == 0)
            {
                stack.Pop();
                continue;
            }

            Vector2Int direction =
                ChooseDirection(
                    validDirections,
                    step.PreviousDirection,
                    straightBias);

            GridNode middle = grid.GetNode(
                current.X + direction.x / 2,
                current.Z + direction.y / 2);

            GridNode next = grid.GetNode(
                current.X + direction.x,
                current.Z + direction.y);

            if (middle.CellType == CellType.Floor)
            {
                middle.SetCellType(CellType.Path);
                pathCount++;
            }

            if (pathCount >= maxPathCells)
            {
                break;
            }

            if (next.CellType == CellType.Floor)
            {
                next.SetCellType(CellType.Path);
                pathCount++;
            }

            stack.Push(new PathStep(next, direction));
        }

        Debug.Log($"Generated path ratio: {(float)pathCount / (grid.Width * grid.Height):P}");
    }
    private void GenerateParking(WarehouseGrid grid, int parkingCount)
    {
        List<GridNode> validParkingNodes = new();

        for (int x = 1; x < grid.Width - 1; x++)
        {
            for (int z = 1; z < grid.Height - 1; z++)
            {
                GridNode node = grid.GetNode(x, z);

                if (node.CellType != CellType.Floor)
                {
                    continue;
                }

                if (!HasExactlyOneAdjacentPath(grid, node))
                {
                    continue;
                }

                validParkingNodes.Add(node);
            }
        }

        Shuffle(validParkingNodes);

        int count = Mathf.Min(parkingCount, validParkingNodes.Count);

        for (int i = 0; i < count; i++)
        {
            validParkingNodes[i].SetCellType(CellType.Parking);
        }

        Debug.Log($"Generated {count} parking nodes. Valid candidates: {validParkingNodes.Count}");
    }

    private List<Vector2Int> GetValidDirections(
    WarehouseGrid grid,
    GridNode current)
    {
        List<Vector2Int> valid = new();

        foreach (Vector2Int direction in directions)
        {
            int nextX = current.X + direction.x;
            int nextZ = current.Z + direction.y;

            if (!IsInsidePathArea(grid, nextX, nextZ))
            {
                continue;
            }

            GridNode next = grid.GetNode(nextX, nextZ);

            if (next.CellType == CellType.Floor)
            {
                valid.Add(direction);
            }
        }

        return valid;
    }
    private bool IsInsidePathArea(WarehouseGrid grid, int x, int z)
    {
        return x > 0 &&
            x < grid.Width - 1 &&
            z > 0 &&
            z < grid.Height - 1;
    }
    private Vector2Int ChooseDirection(
    List<Vector2Int> validDirections,
    Vector2Int previousDirection,
    float straightBias)
    {
        bool canContinueStraight =
            previousDirection != Vector2Int.zero &&
            validDirections.Contains(previousDirection);

        if (canContinueStraight && Random.value < straightBias)
        {
            return previousDirection;
        }

        return validDirections[Random.Range(0, validDirections.Count)];
    }
    private bool HasExactlyOneAdjacentPath(WarehouseGrid grid, GridNode node)
    {
        int adjacentPathCount = 0;

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (Vector2Int direction in directions)
        {
            int neighborX = node.X + direction.x;
            int neighborZ = node.Z + direction.y;

            GridNode neighbor = grid.GetNode(neighborX, neighborZ);

            if (neighbor == null)
            {
                continue;
            }

            if (neighbor.CellType == CellType.Path)
            {
                adjacentPathCount++;
            }
        }

        return adjacentPathCount == 1;
    }
    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex =
                Random.Range(i, list.Count);

            (list[i], list[randomIndex]) =
                (list[randomIndex], list[i]);
        }
    }
}