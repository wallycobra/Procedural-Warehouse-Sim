using System.Collections.Generic;
using UnityEngine;

public class WarehouseManager : MonoBehaviour
{
    [Header("Generation")]
    [SerializeField] private int width = 51;
    [SerializeField] private int height = 51;
    [SerializeField] private float pathRatio = 0.33f;
    [SerializeField] private float straightBias = 0.8f;
    [SerializeField] private float cellSize = 2f;

    [Header("References")]
    [SerializeField] private WarehouseRenderer renderer;
    [SerializeField] private WarehouseApiClient apiClient;
    [SerializeField] private GameObject robotPrefab;
    private float robotSpawnHeight = 0.75f;
    private string warehouseIdToLoad = "a7b79220-1687-4f11-9f69-f2e0c2345e86";

    public WarehouseGrid Grid { get; private set; }

    private void Start()
    {
        //GenerateWarehouse();
    }

    public void GenerateWarehouse()
    {
        PathGenerator generator = new();

        Grid = generator.Generate(
            width,
            height,
            pathRatio,
            straightBias);

        renderer.Render(Grid, cellSize);
        SpawnRobot();
    }
    private void SpawnRobot()
    {
        List<GridNode> parkingNodes = Grid.GetNodesOfType(CellType.Parking);

        if (parkingNodes.Count == 0)
        {
            Debug.LogWarning("No parking nodes found.");
            return;
        }

        GridNode parkingNode = parkingNodes[Random.Range(0, parkingNodes.Count)];

        Vector3 spawnPosition = parkingNode.GetWorldPosition(cellSize);
        spawnPosition.y = robotSpawnHeight;

        GameObject robotObject = Instantiate(
            robotPrefab,
            spawnPosition,
            Quaternion.identity);

        RobotController robot =
            robotObject.GetComponent<RobotController>();

        if (robot == null)
        {
            Debug.LogError("Robot prefab is missing RobotController.");
            return;
        }

        robot.Initialize(Grid, parkingNode, cellSize);
    }

    public void SaveCurrentWarehouse()
    {
        if (Grid == null)
        {
            Debug.LogWarning("No warehouse grid exists.");
            return;
        }

        WarehouseSaveData saveData =
            WarehouseSerializer.CreateSaveData(
                Grid,
                "Test Warehouse",
                cellSize);

        StartCoroutine(apiClient.SaveWarehouse(
            saveData,
            response => Debug.Log($"Saved warehouse: {response}"),
            error => Debug.LogError($"Save failed: {error}")));
    }
    public void LoadWarehouseById()
    {
        if (string.IsNullOrWhiteSpace(warehouseIdToLoad))
        {
            Debug.LogWarning("Warehouse ID is empty.");
            return;
        }

        StartCoroutine(apiClient.LoadWarehouse(
            warehouseIdToLoad,
            OnWarehouseLoaded,
            error => Debug.LogError($"Load failed: {error}")));
    }

    private void OnWarehouseLoaded(WarehouseSaveData saveData)
    {
        Grid = WarehouseSerializer.LoadFromSaveData(saveData);

        renderer.Render(Grid, cellSize);
        SpawnRobot();

        Debug.Log($"Loaded warehouse: {saveData.name}");
    }
}