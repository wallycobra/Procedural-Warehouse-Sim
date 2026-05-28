using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private GameObject loadPosition;
    [SerializeField] private Rigidbody carriedObject;
    [SerializeField] private float throwForce = 1f;
    [SerializeField] private float upwardForce = 1f;

    private WarehouseGrid grid;
    private float cellSize;

    public void Initialize(WarehouseGrid warehouseGrid, GridNode startParkingNode, float gridCellSize)
    {
        grid = warehouseGrid;
        cellSize = gridCellSize;

        GridNode startPathNode = grid.GetAdjacentPathNode(startParkingNode);

        if (startPathNode == null)
        {
            Debug.LogWarning("Start parking node has no adjacent path.");
            return;
        }

        GridNode endDropoffNode = GetRandomDropoffNode();

        if (endDropoffNode == null)
        {
            Debug.LogWarning("No valid destination dropoff node found.");
            return;
        }

        GridNode endPathNode = grid.GetAdjacentPathNode(endDropoffNode);

        if (endPathNode == null)
        {
            Debug.LogWarning("Dropoff node has no adjacent path.");
            return;
        }

        RobotPathfinder pathfinder = new(grid);

        List<GridNode> path = pathfinder.FindPath(startPathNode, endPathNode);

        if (path.Count == 0)
        {
            Debug.LogWarning("Robot could not find path.");
            return;
        }

        path.Insert(0, startParkingNode);
        path.Add(endDropoffNode);

        StartCoroutine(FollowPath(path));
    }

    private GridNode GetRandomPathNode()
    {
        List<GridNode> pathNodes = grid.GetNodesOfType(CellType.Path);
        return pathNodes[Random.Range(0, pathNodes.Count)];
    }

    private IEnumerator FollowPath(List<GridNode> path)
    {
        foreach (GridNode node in path)
        {
            Vector3 targetLocalPosition = node.GetWorldPosition(cellSize);
            targetLocalPosition.y = transform.localPosition.y;

            while (Vector3.Distance(transform.localPosition, targetLocalPosition) > 0.05f)
            {
                Vector3 localDirection =
                    targetLocalPosition - transform.localPosition;

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
        GridNode finalNode = path[^1];

        if (finalNode.CellType == CellType.Parking)
        {
            yield return RotateToFaceAdjacentPath(finalNode);
        }
        if (finalNode.CellType == CellType.DropoffFront)
        {
            yield return ThrowJunk();
        }
    }
    private GridNode GetRandomDifferentParkingNode(GridNode startParkingNode)
    {
        List<GridNode> parkingNodes = grid.GetNodesOfType(CellType.Parking);

        parkingNodes.Remove(startParkingNode);

        if (parkingNodes.Count == 0)
        {
            return null;
        }

        return parkingNodes[Random.Range(0, parkingNodes.Count)];
    }
    private GridNode GetRandomDropoffNode()
    {
        List<GridNode> dropoffNodes = grid.GetNodesOfType(CellType.DropoffFront);

        if (dropoffNodes.Count == 0)
        {
            return null;
        }

        return dropoffNodes[Random.Range(0, dropoffNodes.Count)];
    }

    private IEnumerator RotateAroundInParking()
    {
        Quaternion startRotation = transform.localRotation;

        Quaternion targetRotation =
            startRotation * Quaternion.Euler(0f, 180f, 0f);

        while (Quaternion.Angle(transform.localRotation, targetRotation) > .5f)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                4 * Time.deltaTime);

            yield return null;
        }

        transform.localRotation = targetRotation;
    }
    private IEnumerator RotateToFaceAdjacentPath(GridNode parkingNode)
    {
        GridNode pathNode = grid.GetAdjacentPathNode(parkingNode);

        if (pathNode == null)
        {
            yield break;
        }

        Vector3 parkingPosition = parkingNode.GetWorldPosition(cellSize);
        Vector3 pathPosition = pathNode.GetWorldPosition(cellSize);

        Vector3 direction = pathPosition - parkingPosition;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            yield break;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(direction.normalized);

        while (Quaternion.Angle(transform.localRotation, targetRotation) > 1f)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                4 * Time.deltaTime);

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

        // Detach from robot
        carriedObject.transform.SetParent(null);

        // Re-enable physics
        carriedObject.isKinematic = false;

        // Throw direction = robot forward
        Vector3 throwDirection =
            transform.forward + Vector3.up * 2f;

        throwDirection.Normalize();

        // Clear old velocity just in case
        carriedObject.linearVelocity = Vector3.zero;
        carriedObject.angularVelocity = Vector3.zero;

        // Apply force
        carriedObject.AddForce(
            throwDirection * throwForce,
            ForceMode.Impulse);

        // Optional spin
        carriedObject.AddTorque(
            Random.insideUnitSphere * upwardForce,
            ForceMode.Impulse);

        carriedObject = null;

        yield return null;
    }
}