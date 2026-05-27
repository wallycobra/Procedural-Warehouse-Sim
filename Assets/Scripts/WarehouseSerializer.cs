public static class WarehouseSerializer
{
    public static WarehouseSaveData CreateSaveData(
        WarehouseGrid grid,
        string warehouseName,
        float cellSize)
    {
        WarehouseSaveData saveData = new()
        {
            id = System.Guid.NewGuid().ToString(),
            name = warehouseName,
            width = grid.Width,
            height = grid.Height,
            cellSize = cellSize
        };

        for (int x = 0; x < grid.Width; x++)
        {
            for (int z = 0; z < grid.Height; z++)
            {
                GridNode node = grid.GetNode(x, z);

                saveData.cells.Add(new CellSaveData
                {
                    x = x,
                    z = z,
                    cellType = node.CellType.ToString()
                });
            }
        }

        return saveData;
    }

    public static WarehouseGrid LoadFromSaveData(
        WarehouseSaveData saveData)
    {
        WarehouseGrid grid = new(
            saveData.width,
            saveData.height);

        foreach (CellSaveData cell in saveData.cells)
        {
            if (System.Enum.TryParse(
                cell.cellType,
                out CellType type))
            {
                grid.SetCellType(
                    cell.x,
                    cell.z,
                    type);
            }
        }

        return grid;
    }
}