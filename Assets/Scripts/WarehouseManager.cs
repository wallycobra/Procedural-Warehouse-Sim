using System.Collections;
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
    [SerializeField] private GameObject warehouseContainer;

    [Header("Regenerate Animation")]
    [SerializeField] private float regenerateDropDistance = 40f;
    [SerializeField] private float regenerateDropDuration = 0.35f;
    [SerializeField] private float regenerateStaggerDelay = 0.005f;
    [SerializeField] private bool isRegenerating;

    [SerializeField] public static List<GridNode> parkingNodes;
    [SerializeField] public static List<GridNode> dropoffNodes;
    [SerializeField] public static List<GridNode> pickupNodes;
    [SerializeField] private float dropDuration = 0.4f;
    [SerializeField] private float spawnHeight = 25f;
    private float robotSpawnHeight = 0.75f;
    private string warehouseIdToLoad = "a7b79220-1687-4f11-9f69-f2e0c2345e86";

    public WarehouseGrid Grid { get; private set; }

    private void Start()
    {
        GenerateWarehouse();
    }

    public void GenerateWarehouse()
    {
        PathGenerator generator = new();
        RackGenerator rackGenerator = new();
        DropoffGenerator dropoffGenerator = new();

        Grid = generator.Generate(
            width,
            height,
            pathRatio,
            straightBias);


        rackGenerator.GenerateRacks(Grid, Random.Range(2, 4));
        dropoffGenerator.GenerateDropoffs(Grid, 1);

        renderer.Render(Grid, cellSize);
        warehouseContainer.transform.position = new Vector3(Grid.Height / 2f, 0, Grid.Width / 2f);
        gameObject.transform.SetParent(warehouseContainer.transform, true);

        SpawnRobot();
    }

    public void RegenerateWarehouse()
    {
        if (isRegenerating)
        {
            return;
        }

        StartCoroutine(RegenerateWarehouseRoutine());
    }

    private IEnumerator RegenerateWarehouseRoutine()
    {
        isRegenerating = true;

        RobotController[] robots =
            GetComponentsInChildren<RobotController>(true);

        foreach (RobotController robot in robots)
        {
            robot.Shutdown();
        }

        List<Transform> children = new();

        foreach (Transform child in transform)
        {
            children.Add(child);
        }

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] != null)
            {
                float delay = Random.Range(0f, 0.6f);

                StartCoroutine(DropOutAndDestroy(
                    children[i],
                    delay));
            }
        }

        float totalWait = regenerateDropDuration + 0.6f;

        yield return new WaitForSeconds(totalWait);

        pickupNodes.Clear();
        dropoffNodes.Clear();
        parkingNodes.Clear();

        transform.SetParent(null);
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        warehouseContainer.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        yield return null;

        GenerateWarehouse();

        isRegenerating = false;
    }
    private void SpawnRobot()
    {
        parkingNodes = Grid.GetNodesOfType(CellType.Parking);
        dropoffNodes = Grid.GetNodesOfType(CellType.DropoffFront);

        if (parkingNodes.Count == 0)
        {
            Debug.LogWarning("No parking nodes found.");
            return;
        }

        if (dropoffNodes.Count == 0)
        {
            Debug.LogWarning("No dropoff nodes found.");
            return;
        }

        GridNode parkingNode =
            parkingNodes[Random.Range(0, parkingNodes.Count)];

        Vector3 finalPosition =
            parkingNode.GetWorldPosition(cellSize);

        finalPosition.y = robotSpawnHeight;

        Vector3 startPosition =
            finalPosition + Vector3.up * spawnHeight;

        GameObject robotObject = Instantiate(
            robotPrefab,
            gameObject.transform);

        robotObject.transform.localPosition = startPosition;
        robotObject.transform.localRotation = Quaternion.identity;

        RobotController robot =
            robotObject.GetComponent<RobotController>();

        if (robot == null)
        {
            Debug.LogError("Robot prefab is missing RobotController.");
            return;
        }

        StartCoroutine(SpawnRobotRoutine(
            robot,
            parkingNode,
            startPosition,
            finalPosition));
    }

    private IEnumerator SpawnRobotRoutine(
    RobotController robot,
    GridNode parkingNode,
    Vector3 startPosition,
    Vector3 finalPosition)
    {
        yield return DropIntoPlace(
            robot.transform,
            startPosition,
            finalPosition,
            0.5f);

        robot.Initialize(
            Grid,
            parkingNode,
            cellSize,
            warehouseContainer.transform,
            false);
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
    private IEnumerator DropIntoPlace(
    Transform target,
    Vector3 startPosition,
    Vector3 endPosition,
    float delay)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;

        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / dropDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            target.localPosition = Vector3.Lerp(
                startPosition,
                endPosition,
                t);

            yield return null;
        }

        target.localPosition = endPosition;
    }

    private IEnumerator DropOutAndDestroy(
    Transform target,
    float delay)
    {
        yield return new WaitForSeconds(delay);

        if (target == null)
        {
            yield break;
        }

        Vector3 startPosition = target.localPosition;
        Vector3 endPosition =
            startPosition + Vector3.down * regenerateDropDistance;

        float elapsed = 0f;

        while (elapsed < regenerateDropDuration)
        {
            if (target == null)
            {
                yield break;
            }

            elapsed += Time.deltaTime;

            float t = elapsed / regenerateDropDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            target.localPosition = Vector3.Lerp(
                startPosition,
                endPosition,
                t);

            yield return null;
        }

        if (target != null)
        {
            Destroy(target.gameObject);
        }
    }
}