using UnityEngine;

public class WarehouseRenderer : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject pathPrefab;
    [SerializeField] private GameObject parkingPrefab;

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

    private void SpawnNode(
        GridNode node,
        float cellSize)
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

    private void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}