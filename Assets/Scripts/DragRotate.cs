using UnityEngine;
using UnityEngine.EventSystems;

public class DragRotate : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 0.2f;

    private bool isDragging;
    private float previousMouseX;
    public static Quaternion Rotation { get; private set; }

    private void Start()
    {
        Rotation = transform.rotation;
    }
    private void Update()
    {
        if (EventSystem.current != null &&
        EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        // Start dragging
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            previousMouseX = Input.mousePosition.x;
        }

        // Stop dragging
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // Rotate while dragging
        if (isDragging)
        {
            float currentMouseX = Input.mousePosition.x;

            float deltaX = currentMouseX - previousMouseX;

            transform.Rotate(0f, -deltaX * rotationSpeed, 0f);
            Rotation = transform.rotation;

            previousMouseX = currentMouseX;
        }
    }
}