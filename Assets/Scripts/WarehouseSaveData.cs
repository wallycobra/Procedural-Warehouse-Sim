using System.Collections.Generic;

[System.Serializable]
public class WarehouseSaveData
{
    public string id;
    public string name;
    public int width;
    public int height;
    public float cellSize;
    public List<CellSaveData> cells = new();
}

[System.Serializable]
public class CellSaveData
{
    public int x;
    public int z;
    public string cellType;
}