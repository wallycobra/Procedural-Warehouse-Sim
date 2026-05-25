using System.Collections.Generic;
using UnityEngine;

public class RobotPathfinder
{
    private readonly WarehouseGrid grid;

    public RobotPathfinder(WarehouseGrid grid)
    {
        this.grid = grid;
    }

    public List<GridNode> FindPath(GridNode startNode, GridNode endNode)
    {
        List<GridNode> openSet = new();
        HashSet<GridNode> closedSet = new();

        Dictionary<GridNode, GridNode> cameFrom = new();
        Dictionary<GridNode, int> gCost = new();
        Dictionary<GridNode, int> hCost = new();

        openSet.Add(startNode);

        gCost[startNode] = 0;
        hCost[startNode] = GetDistance(startNode, endNode);

        while (openSet.Count > 0)
        {
            GridNode currentNode = GetLowestCostNode(openSet, gCost, hCost);

            if (currentNode == endNode)
            {
                return ReconstructPath(cameFrom, currentNode);
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            foreach (GridNode neighbor in grid.GetTraversableNeighbors(currentNode))
            {
                if (closedSet.Contains(neighbor))
                {
                    continue;
                }

                int tentativeGCost = gCost[currentNode] + GetDistance(currentNode, neighbor);

                if (!gCost.ContainsKey(neighbor) || tentativeGCost < gCost[neighbor])
                {
                    cameFrom[neighbor] = currentNode;
                    gCost[neighbor] = tentativeGCost;
                    hCost[neighbor] = GetDistance(neighbor, endNode);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        Debug.LogWarning("No path found.");
        return new List<GridNode>();
    }

    private GridNode GetLowestCostNode(
        List<GridNode> openSet,
        Dictionary<GridNode, int> gCost,
        Dictionary<GridNode, int> hCost)
    {
        GridNode lowest = openSet[0];

        foreach (GridNode node in openSet)
        {
            int nodeFCost = gCost[node] + hCost[node];
            int lowestFCost = gCost[lowest] + hCost[lowest];

            if (nodeFCost < lowestFCost)
            {
                lowest = node;
            }
            else if (nodeFCost == lowestFCost && hCost[node] < hCost[lowest])
            {
                lowest = node;
            }
        }

        return lowest;
    }

    private int GetDistance(GridNode a, GridNode b)
    {
        int xDistance = Mathf.Abs(a.X - b.X);
        int zDistance = Mathf.Abs(a.Z - b.Z);

        return xDistance + zDistance;
    }

    private List<GridNode> ReconstructPath(
        Dictionary<GridNode, GridNode> cameFrom,
        GridNode currentNode)
    {
        List<GridNode> path = new()
        {
            currentNode
        };

        while (cameFrom.ContainsKey(currentNode))
        {
            currentNode = cameFrom[currentNode];
            path.Add(currentNode);
        }

        path.Reverse();

        return path;
    }
}