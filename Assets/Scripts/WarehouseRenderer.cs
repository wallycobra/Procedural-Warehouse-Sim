using System.Collections.Generic;
using UnityEngine;

public class WarehouseRenderer : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int width = 51;
    [SerializeField] private int height = 51;
    [SerializeField] private float cellSize = 2f;
    [SerializeField] private float pathRatio = 0.33f;
    [SerializeField] private float straightBias = 0.75f;

    [Header("Prefabs")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject pathPrefab;
    [SerializeField] private GameObject parkingPrefab;
    [SerializeField] private GameObject robotPrefab;

    private WarehouseGrid grid;

    private void Start()
    {
        GenerateWarehouse();
    }

    private void GenerateWarehouse()
    {
        PathGenerator generator = new();

        grid = generator.Generate(width, height, pathRatio, straightBias);

        RenderGrid();
        SpawnRobot();
    }

    private void RenderGrid()
    {
        for (int x = 0; x < grid.Width; x++)
        {
            for (int z = 0; z < grid.Height; z++)
            {
                GridNode node = grid.GetNode(x, z);

                SpawnNode(node);
            }
        }
    }

    private void SpawnNode(GridNode node)
    {
        GameObject prefab = node.CellType switch
        {
            CellType.Path => pathPrefab,
            CellType.Parking => parkingPrefab,
            _ => floorPrefab
        };

        Vector3 worldPosition =
            node.GetWorldPosition(cellSize);

        GameObject spawned =
            Instantiate(
                prefab,
                worldPosition,
                Quaternion.identity,
                transform);

        node.VisualObject = spawned;
    }

    private void SpawnRobot()
    {
        List<GridNode> parkingNodes = grid.GetNodesOfType(CellType.Parking);

        if (parkingNodes.Count == 0)
        {
            Debug.LogWarning("No parking nodes found.");
            return;
        }

        GridNode parkingNode = parkingNodes[Random.Range(0, parkingNodes.Count)];

        Vector3 spawnPosition = parkingNode.GetWorldPosition(cellSize);
        spawnPosition.y = 0.75f;

        GameObject robotObject = Instantiate(robotPrefab, spawnPosition, Quaternion.identity);

        RobotController robot = robotObject.GetComponent<RobotController>();
        robot.Initialize(grid, parkingNode, cellSize);
    }
}