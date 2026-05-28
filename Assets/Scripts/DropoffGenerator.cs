using System.Collections.Generic;
using UnityEngine;

public class DropoffGenerator
{
    private readonly Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    public List<DropoffData> GenerateDropoffs(
        WarehouseGrid grid,
        int dropoffCount)
    {
        List<DropoffData> candidates = new();

        for (int x = 1; x < grid.Width - 1; x++)
        {
            for (int z = 1; z < grid.Height - 1; z++)
            {
                GridNode pathNode = grid.GetNode(x, z);

                if (pathNode.CellType != CellType.Path)
                {
                    continue;
                }

                foreach (Vector2Int direction in directions)
                {
                    GridNode frontCell = grid.GetNode(
                        x + direction.x,
                        z + direction.y);

                    GridNode backCell = grid.GetNode(
                        x + direction.x * 2,
                        z + direction.y * 2);

                    if (frontCell == null || backCell == null)
                    {
                        continue;
                    }

                    if (frontCell.CellType != CellType.Floor ||
                        backCell.CellType != CellType.Floor)
                    {
                        continue;
                    }

                    candidates.Add(new DropoffData(frontCell, backCell));
                }
            }
        }

        Shuffle(candidates);

        List<DropoffData> placed = new();

        for (int i = 0; i < candidates.Count && placed.Count < dropoffCount; i++)
        {
            DropoffData dropoff = candidates[i];

            if (dropoff.FrontCell.CellType != CellType.Floor ||
                dropoff.BackCell.CellType != CellType.Floor)
            {
                continue;
            }

            dropoff.FrontCell.SetCellType(CellType.DropoffFront);
            dropoff.BackCell.SetCellType(CellType.DropoffBack);

            placed.Add(dropoff);
        }

        Debug.Log($"Generated {placed.Count}/{dropoffCount} dropoff points.");

        return placed;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}