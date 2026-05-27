using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 8f;

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

        GridNode endParkingNode = GetRandomDifferentParkingNode(startParkingNode);

        if (endParkingNode == null)
        {
            Debug.LogWarning("No valid destination parking node found.");
            return;
        }

        GridNode endPathNode = grid.GetAdjacentPathNode(endParkingNode);

        if (endPathNode == null)
        {
            Debug.LogWarning("Destination parking node has no adjacent path.");
            return;
        }

        RobotPathfinder pathfinder = new RobotPathfinder(grid);

        List<GridNode> path = pathfinder.FindPath(startPathNode, endPathNode);

        if (path.Count == 0)
        {
            Debug.LogWarning("Robot could not find path.");
            return;
        }

        path.Insert(0, startParkingNode);
        path.Add(endParkingNode);

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
            Vector3 targetPosition = node.GetWorldPosition(cellSize);
            targetPosition.y = transform.position.y;

            while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
            {
                Vector3 direction =
                    (targetPosition - transform.position).normalized;

                direction.y = 0f;

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation =
                        Quaternion.LookRotation(direction);

                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime);
                }

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    moveSpeed * Time.deltaTime);

                yield return null;
            }

            transform.position = targetPosition;
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
}