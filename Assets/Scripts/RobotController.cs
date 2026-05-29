using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1f);
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Load")]
    [SerializeField] private GameObject loadPosition;
    [SerializeField] private Rigidbody carriedObject;
    [SerializeField] private float throwForce = 1f;
    [SerializeField] private float upwardForce = 1f;
    private bool isCarryingItem;
    private bool isMoving;

    private WarehouseGrid grid;
    private float cellSize;
    private Transform warehouseTransform;
    private RobotPathfinder pathfinder;

    public GridNode CurrentNode { get; private set; }
    private bool isShuttingDown;


    public void Initialize(
        WarehouseGrid warehouseGrid,
        GridNode startParkingNode,
        float gridCellSize,
        Transform warehouseTransform,
        bool isShuttingDown)
    {
        grid = warehouseGrid;
        cellSize = gridCellSize;
        this.warehouseTransform = warehouseTransform;
        this.isShuttingDown = isShuttingDown;

        pathfinder = new RobotPathfinder(grid);

        CurrentNode = startParkingNode;

        Vector3 startPosition = startParkingNode.GetWorldPosition(cellSize);
        startPosition.y = transform.localPosition.y;
        transform.localPosition = startPosition;

        StartCoroutine(MissionLoop());
    }

    public void Shutdown()
    {
        isShuttingDown = true;
        StopAllCoroutines();
    }

    private IEnumerator MissionLoop()
    {
        while (true)
        {
            GridNode pickupNode = GetRandomPickupNode();

            if (pickupNode == null)
            {
                Debug.LogWarning("No pickup node found.");
                yield break;
            }

            yield return GoToPickup(pickupNode);

            GridNode dropoffNode = GetRandomDropoffNode();

            if (dropoffNode == null)
            {
                Debug.LogWarning("No dropoff node found.");
                yield break;
            }

            yield return GoToDropoff(dropoffNode);

            GridNode parkingNode =
                GetRandomDifferentParkingNode(CurrentNode);

            if (parkingNode == null)
            {
                Debug.LogWarning("No parking node found.");
                yield break;
            }

            yield return GoToParking(parkingNode);

            yield return _waitForSeconds1;
        }
    }

    public IEnumerator GoToPickup(GridNode pickupNode)
    {
        yield return GoToNode(pickupNode);

        yield return PickUpItem();
    }

    public IEnumerator GoToDropoff(GridNode dropoffNode)
    {
        yield return GoToNode(dropoffNode);

        yield return ThrowJunk();
    }

    public IEnumerator GoToParking(GridNode parkingNode)
    {
        yield return GoToNode(parkingNode);

        yield return RotateAroundInParking();
    }

    private IEnumerator GoToNode(GridNode targetNode)
    {
        if (CurrentNode == null)
        {
            yield break;
        }

        if (grid == null)
        {
            yield break;
        }

        if (CurrentNode == null || targetNode == null)
        {
            Debug.LogWarning("Current node or target node is null.");
            yield break;
        }

        GridNode startPathNode = grid.GetAdjacentPathNode(CurrentNode);
        GridNode targetPathNode = grid.GetAdjacentPathNode(targetNode);

        if (startPathNode == null || targetPathNode == null)
        {
            Debug.LogWarning($"Could not find adjacent path node for route to {targetNode.CellType}.");
            yield break;
        }

        List<GridNode> path = pathfinder.FindPath(startPathNode, targetPathNode);

        if (path.Count == 0)
        {
            Debug.LogWarning($"No path found to {targetNode.CellType}.");
            yield break;
        }

        path.Insert(0, CurrentNode);
        path.Add(targetNode);

        yield return FollowPath(path);

        CurrentNode = targetNode;
    }

    private IEnumerator PickUpItem()
    {
        yield return _waitForSeconds1;
        GameObject item = Instantiate(
            Resources.Load<GameObject>("item"),
            loadPosition.transform.position,
            Quaternion.identity,
            loadPosition.transform);
        carriedObject = item.GetComponent<Rigidbody>();    
        isCarryingItem = true;

        Debug.Log("Robot picked up item.");
    }

    private IEnumerator FollowPath(List<GridNode> path)
    {
        foreach (GridNode node in path)
        {
            if (isShuttingDown)
            {
                yield break;
            }

            Vector3 targetLocalPosition = node.GetWorldPosition(cellSize);
            targetLocalPosition.y = transform.localPosition.y;

            while (Vector3.Distance(transform.localPosition, targetLocalPosition) > 0.05f)
            {
                Vector3 localDirection = targetLocalPosition - transform.localPosition;
                localDirection.y = 0f;

                if (localDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetLocalRotation =
                        Quaternion.LookRotation(localDirection.normalized);

                    transform.localRotation = Quaternion.Slerp(
                        transform.localRotation,
                        targetLocalRotation,
                        rotationSpeed * Time.deltaTime);
                }

                transform.localPosition = Vector3.MoveTowards(
                    transform.localPosition,
                    targetLocalPosition,
                    moveSpeed * Time.deltaTime);

                yield return null;
            }

            transform.localPosition = targetLocalPosition;
        }
    }

    private GridNode GetRandomPickupNode()
    {
        WarehouseManager.pickupNodes = grid.GetNodesOfType(CellType.Pickup);

        if (WarehouseManager.pickupNodes.Count == 0)
        {
            return null;
        }

        return WarehouseManager.pickupNodes[Random.Range(0, WarehouseManager.pickupNodes.Count)];
    }

    private GridNode GetRandomDropoffNode()
    {
        WarehouseManager.dropoffNodes = grid.GetNodesOfType(CellType.DropoffFront);

        if (WarehouseManager.dropoffNodes.Count == 0)
        {
            return null;
        }

        return WarehouseManager.dropoffNodes[Random.Range(0, WarehouseManager.dropoffNodes.Count)];
    }

    private GridNode GetRandomDifferentParkingNode(GridNode currentNode)
    {
        WarehouseManager.parkingNodes = grid.GetNodesOfType(CellType.Parking);

        WarehouseManager.parkingNodes.Remove(currentNode);

        if (WarehouseManager.parkingNodes.Count == 0)
        {
            return null;
        }

        return WarehouseManager.parkingNodes[Random.Range(0, WarehouseManager.parkingNodes.Count)];
    }

    private IEnumerator RotateAroundInParking()
    {
        Quaternion startRotation = transform.localRotation;
        Quaternion targetRotation =
            startRotation * Quaternion.Euler(0f, 180f, 0f);

        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.5f)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);

            yield return null;
        }

        transform.localRotation = targetRotation;
    }

    private IEnumerator ThrowJunk()
    {
        if (carriedObject == null)
        {
            yield break;
        }

        yield return new WaitForSeconds(2);

        carriedObject.transform.SetParent(warehouseTransform);
        carriedObject.isKinematic = false;

        Vector3 throwDirection =
            transform.forward + Vector3.up * 2f;

        throwDirection.Normalize();

        carriedObject.linearVelocity = Vector3.zero;
        carriedObject.angularVelocity = Vector3.zero;

        carriedObject.AddForce(
            throwDirection * throwForce,
            ForceMode.Impulse);

        carriedObject.AddTorque(
            Random.insideUnitSphere * upwardForce,
            ForceMode.Impulse);

        Destroy(carriedObject.gameObject, 5f);
        carriedObject = null;
        isCarryingItem = false;

        yield return new WaitForSeconds(1f);

    }
}