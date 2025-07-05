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

            Vector2Int snappedPos = new Vector2Int(
                Mathf.RoundToInt(transform.position.x),
                Mathf.RoundToInt(transform.position.y)
            );

            // Kiểm tra xem vị trí này đã có block chưa
            if (!BlockPositionManager.blockPositions.Contains(snappedPos))
            {
                // Kiểm tra số lượng block xung quanh vị trí (up, down, left, right)
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

                // Nếu có ít nhất 2 block xung quanh và vị trí chưa có block, snap vào vị trí này
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
                // Nếu vị trí đã có block, không snap và đưa về vị trí ban đầu
                transform.position = startPosition;
                isSnapped = false;
            }
        }
    }

}
