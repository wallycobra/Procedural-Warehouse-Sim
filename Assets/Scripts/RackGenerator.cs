using System.Collections.Generic;
using UnityEngine;

public class RackGenerator
{
    private enum RackFacing
    {
        North,
        South,
        East,
        West
    }

    public List<RackData> GenerateRacks(
        WarehouseGrid grid,
        int rackCount)
    {
        List<RackData> racks = new();

        int attempts = rackCount * 100;

        for (int i = 0; i < attempts && racks.Count < rackCount; i++)
        {
            bool horizontalRack = Random.value > 0.5f;
            int rackWidth = Random.Range(2, 5);
            int rackHeight = Random.Range(2, 5);

            int width = horizontalRack
                ? Mathf.Max(rackWidth, rackHeight)
                : Mathf.Min(rackWidth, rackHeight);

            int height = horizontalRack
                ? Mathf.Min(rackWidth, rackHeight)
                : Mathf.Max(rackWidth, rackHeight);

            int x = Random.Range(2, grid.Width - width - 2);
            int z = Random.Range(2, grid.Height - height - 2);

            if (!TryGetValidFacing(grid, x, z, width, height, out RackFacing facing))
            {
                continue;
            }

            RackData rack = new(x, z, width, height);
            racks.Add(rack);

            MarkRackAndPickupCells(grid, rack, facing);
        }

        Debug.Log($"Generated {racks.Count}/{rackCount} racks.");

        return racks;
    }

    private bool TryGetValidFacing(
        WarehouseGrid grid,
        int startX,
        int startZ,
        int width,
        int height,
        out RackFacing validFacing)
    {
        RackFacing[] facings =
        {
            RackFacing.North,
            RackFacing.South,
            RackFacing.East,
            RackFacing.West
        };

        foreach (RackFacing facing in facings)
        {
            if (IsValidFacing(grid, startX, startZ, width, height, facing))
            {
                validFacing = facing;
                return true;
            }
        }

        validFacing = RackFacing.North;
        return false;
    }

    private bool IsValidFacing(
        WarehouseGrid grid,
        int startX,
        int startZ,
        int width,
        int height,
        RackFacing facing)
    {
        if (!AllCellsAreFloor(grid, startX, startZ, width, height))
        {
            return false;
        }

        switch (facing)
        {
            case RackFacing.North:
                return width > height &&
                       IsHorizontalSideFloor(grid, startX, startZ + height, width) &&
                       IsHorizontalSidePath(grid, startX, startZ + height + 1, width);

            case RackFacing.South:
                return width > height &&
                       IsHorizontalSideFloor(grid, startX, startZ - 1, width) &&
                       IsHorizontalSidePath(grid, startX, startZ - 2, width);

            case RackFacing.East:
                return height > width &&
                       IsVerticalSideFloor(grid, startX + width, startZ, height) &&
                       IsVerticalSidePath(grid, startX + width + 1, startZ, height);

            case RackFacing.West:
                return height > width &&
                       IsVerticalSideFloor(grid, startX - 1, startZ, height) &&
                       IsVerticalSidePath(grid, startX - 2, startZ, height);

            default:
                return false;
        }
    }

    private bool IsHorizontalSidePath(
        WarehouseGrid grid,
        int startX,
        int z,
        int width)
    {
        for (int x = startX; x < startX + width; x++)
        {
            if (grid.GetNode(x, z)?.CellType != CellType.Path)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsHorizontalSideFloor(
        WarehouseGrid grid,
        int startX,
        int z,
        int width)
    {
        for (int x = startX; x < startX + width; x++)
        {
            if (grid.GetNode(x, z)?.CellType != CellType.Floor)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsVerticalSidePath(
        WarehouseGrid grid,
        int x,
        int startZ,
        int height)
    {
        for (int z = startZ; z < startZ + height; z++)
        {
            if (grid.GetNode(x, z)?.CellType != CellType.Path)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsVerticalSideFloor(
        WarehouseGrid grid,
        int x,
        int startZ,
        int height)
    {
        for (int z = startZ; z < startZ + height; z++)
        {
            if (grid.GetNode(x, z)?.CellType != CellType.Floor)
            {
                return false;
            }
        }

        return true;
    }

    private bool AllCellsAreFloor(
        WarehouseGrid grid,
        int startX,
        int startZ,
        int width,
        int height)
    {
        for (int x = startX; x < startX + width; x++)
        {
            for (int z = startZ; z < startZ + height; z++)
            {
                GridNode node = grid.GetNode(x, z);

                if (node == null || node.CellType != CellType.Floor)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void MarkRackAndPickupCells(
        WarehouseGrid grid,
        RackData rack,
        RackFacing facing)
    {
        MarkRackCells(grid, rack);

        switch (facing)
        {
            case RackFacing.North:
                MarkHorizontalPickup(grid, rack.X, rack.Z + rack.Height, rack.Width);
                break;

            case RackFacing.South:
                MarkHorizontalPickup(grid, rack.X, rack.Z - 1, rack.Width);
                break;

            case RackFacing.East:
                MarkVerticalPickup(grid, rack.X + rack.Width, rack.Z, rack.Height);
                break;

            case RackFacing.West:
                MarkVerticalPickup(grid, rack.X - 1, rack.Z, rack.Height);
                break;
        }
    }

    private void MarkHorizontalPickup(
        WarehouseGrid grid,
        int startX,
        int z,
        int width)
    {
        for (int x = startX; x < startX + width; x++)
        {
            grid.SetCellType(x, z, CellType.Pickup);
        }
    }

    private void MarkVerticalPickup(
        WarehouseGrid grid,
        int x,
        int startZ,
        int height)
    {
        for (int z = startZ; z < startZ + height; z++)
        {
            grid.SetCellType(x, z, CellType.Pickup);
        }
    }

    private void MarkRackCells(WarehouseGrid grid, RackData rack)
    {
        for (int x = rack.X; x < rack.X + rack.Width; x++)
        {
            for (int z = rack.Z; z < rack.Z + rack.Height; z++)
            {
                grid.SetCellType(x, z, CellType.Rack);
            }
        }
    }
}