using UnityEngine;

public class WarehouseRenderer : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject pathPrefab;
    [SerializeField] private GameObject parkingPrefab;
    [SerializeField] private GameObject pathPrefabCorner;
    [SerializeField] private GameObject pathPrefabThreeWay;
    [SerializeField] private GameObject pathPrefabDeadEnd;

    public void Render(
        WarehouseGrid grid,
        float cellSize)
    {
        Clear();

        for (int x = 0; x < grid.Width; x++)
        {
            for (int z = 0; z < grid.Height; z++)
            {
                GridNode node = grid.GetNode(x, z);

                SpawnNode(node, cellSize);
            }
        }
    }

    private void SpawnNode(GridNode node, float cellSize)
    {
        GameObject prefab;
        Quaternion rotation = Quaternion.identity;

        if (node.CellType == CellType.Path)
        {
            PathShape shape = node.GetPathShape();

            switch (shape)
            {
                case PathShape.Straight:
                    prefab = pathPrefab;
                    rotation = GetStraightRotation(node);
                    break;

                case PathShape.Corner:
                    prefab = pathPrefabCorner;
                    rotation = GetCornerRotation(node);
                    break;

                case PathShape.ThreeWayIntersection:
                    prefab = pathPrefabThreeWay;
                    rotation = GetThreeWayRotation(node);
                    break;

                case PathShape.DeadEnd:
                    prefab = pathPrefabDeadEnd;
                    rotation = GetDeadEndRotation(node);
                    break;

                default:
                    prefab = pathPrefab;
                    break;
            }
}
        else
        {
            prefab = node.CellType switch
            {
                CellType.Parking => parkingPrefab,
                _ => floorPrefab
            };
        }

        Vector3 worldPosition = node.GetWorldPosition(cellSize);

        GameObject spawned = Instantiate(
            prefab,
            worldPosition,
            rotation,
            transform);

        node.VisualObject = spawned;
    }

    private void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private Quaternion GetStraightRotation(GridNode node)
    {
        bool north = node.HasNorthPath;
        bool south = node.HasSouthPath;
        bool east = node.HasEastPath;
        bool west = node.HasWestPath;

        bool vertical =
            north && south;

        bool horizontal =
            east && west;

        if (horizontal)
        {
            return Quaternion.Euler(0f, 90f, 0f);
        }

        return Quaternion.identity;
    }
    private Quaternion GetCornerRotation(GridNode node)
    {
        float yRotation = 0f;
        float cornerRotationOffset = -90f;

        if (node.HasNorthPath && node.HasEastPath)
        {
            yRotation = 0f;
        }
        else if (node.HasEastPath && node.HasSouthPath)
        {
            yRotation = 90f;
        }
        else if (node.HasSouthPath && node.HasWestPath)
        {
            yRotation = 180f;
        }
        else if (node.HasWestPath && node.HasNorthPath)
        {
            yRotation = 270f;
        }

        return Quaternion.Euler(0f, yRotation + cornerRotationOffset, 0f);
    }
    private Quaternion GetDeadEndRotation(GridNode node)
    {
        float deadEndRotationOffset = 180f;

        if (node.HasNorthPath)
        {
            return Quaternion.Euler(0f, 0f + deadEndRotationOffset, 0f);
        }

        if (node.HasEastPath)
        {
            return Quaternion.Euler(0f, 90f + deadEndRotationOffset, 0f);
        }

        if (node.HasSouthPath)
        {
            return Quaternion.Euler(0f, 180f + deadEndRotationOffset, 0f);
        }

        if (node.HasWestPath)
        {
            return Quaternion.Euler(0f, 270f + deadEndRotationOffset, 0f);
        }

        return Quaternion.identity;
    }

    private Quaternion GetThreeWayRotation(GridNode node)
    {
        bool north = node.HasNorthPath;
        bool south = node.HasSouthPath;
        bool east = node.HasEastPath;
        bool west = node.HasWestPath;

        float rotationOffset = 180f;

        // Missing South
        if (north && east && west)
        {
            return Quaternion.Euler(0f, 0f + rotationOffset, 0f);
        }

        // Missing West
        if (north && east && south)
        {
            return Quaternion.Euler(0f, 90f + rotationOffset, 0f);
        }

        // Missing North
        if (east && south && west)
        {
            return Quaternion.Euler(0f, 180f + rotationOffset, 0f);
        }

        // Missing East
        if (north && south && west)
        {
            return Quaternion.Euler(0f, 270f + rotationOffset, 0f);
        }

        return Quaternion.identity;
    }
}