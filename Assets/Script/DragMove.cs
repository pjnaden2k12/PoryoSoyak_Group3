using UnityEngine;
using UnityEngine.InputSystem;

public class DragMove : MonoBehaviour
{
    private Camera cam;
    private Vector3 offset;
    private bool isDragging = false;
    private Vector3 startPosition;
    public bool isSnapped = false;

    void Start()
    {
        cam = Camera.main;
        startPosition = transform.position;
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
            worldPoint.z = 0f;

            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                offset = transform.position - worldPoint;
                offset.z = 0f;
            }
        }

        if (Mouse.current.leftButton.isPressed && isDragging)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
            worldPoint.z = 0f;
            transform.position = worldPoint + offset;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.2f);
            bool isOverTable = false;

            foreach (var col in hits)
            {
                if (col.CompareTag("Table"))
                {
                    isOverTable = true;
                    break;
                }
            }

            if (isOverTable)
            {
                transform.position = startPosition;
                isSnapped = false;
                return;
            }

            Vector2Int snappedPos = new Vector2Int(
                Mathf.RoundToInt(transform.position.x),
                Mathf.RoundToInt(transform.position.y)
            );

            if (!BlockPositionManager.blockPositions.Contains(snappedPos))
            {
                Vector2Int[] directions = {
                    Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
                };

                int nearbyBlockCount = 0;

                foreach (Vector2Int dir in directions)
                {
                    if (BlockPositionManager.blockPositions.Contains(snappedPos + dir))
                    {
                        nearbyBlockCount++;
                    }
                }

                if (nearbyBlockCount >= 2)
                {
                    transform.position = new Vector3(snappedPos.x, snappedPos.y, 0f);
                    isSnapped = true;
                }
                else
                {
                    transform.position = startPosition;
                    isSnapped = false;
                }
            }
            else
            {
                transform.position = startPosition;
                isSnapped = false;
            }
        }
    }
}
