using UnityEngine;

internal class PathStep
{
    public GridNode Node { get; }
    public Vector2Int PreviousDirection { get; }

    public PathStep(GridNode node, Vector2Int previousDirection)
    {
        Node = node;
        PreviousDirection = previousDirection;
    }
}