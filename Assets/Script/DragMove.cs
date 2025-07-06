using UnityEngine;
using UnityEngine.InputSystem;

public class DragMove : MonoBehaviour
{
    private Camera cam;
    private Vector3 offset;
    private bool isDragging = false;
    private Vector3 startPosition;
    public bool isSnapped = false;
    private Transform startParent;
    private LayerMask itemLayerMask;

    void Start()
    {
        cam = Camera.main;
        startPosition = transform.position;
        startParent = transform.parent;
        itemLayerMask = LayerMask.GetMask("ItemLayer"); // Lấy LayerMask cho Item
    }

    void Update()
    {
        // Logic khi nhấn chuột xuống (giữ nguyên)
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
            worldPoint.z = 0f;

            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, itemLayerMask);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                offset = transform.position - worldPoint;
                isSnapped = false;
                transform.SetParent(null);
            }
        }

        // Logic khi đang kéo (giữ nguyên)
        if (Mouse.current.leftButton.isPressed && isDragging)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
            worldPoint.z = 0f;
            transform.position = worldPoint + offset;
        }

        // Logic khi thả chuột ra
        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;

            Vector2Int snappedGridPos = new Vector2Int(
                Mathf.RoundToInt(transform.position.x),
                Mathf.RoundToInt(transform.position.y)
            );

            bool isOccupied = false;

            // Kiểm tra xem có Block hay Item nào ở vị trí này không
            if (MapSpawner.blockMap.ContainsKey(snappedGridPos))
            {
                isOccupied = true;
            }
            else
            {
                // Kiểm tra xem có Item nào khác ở vị trí này không
                Collider2D[] hits = Physics2D.OverlapCircleAll(snappedGridPos, 0.2f, itemLayerMask);
                foreach (Collider2D hit in hits)
                {
                    if (hit.gameObject != this.gameObject)
                    {
                        isOccupied = true;
                        break;
                    }
                }
            }

            // Kiểm tra nếu thả lên đối tượng có tag "table"
            Collider2D tableCollider = Physics2D.OverlapPoint(transform.position);
            if (tableCollider != null && tableCollider.CompareTag("Table"))
            {
                // Nếu va chạm với "table", trả về vị trí spawn ban đầu
                transform.position = startPosition;
                transform.SetParent(startParent);
                isSnapped = false;

                // Nếu là cầu nối, kiểm tra lại kết nối
                InstantBridge bridge = GetComponent<InstantBridge>();
                if (bridge != null)
                {
                    bridge.CheckConnections();
                }
            }
            else if (isOccupied)
            {
                // Vị trí có vật cản, trả về vị trí cũ
                transform.position = startPosition;
                transform.SetParent(startParent);
                isSnapped = false;

                // Nếu là cầu nối, kiểm tra lại kết nối
                InstantBridge bridge = GetComponent<InstantBridge>();
                if (bridge != null)
                {
                    bridge.CheckConnections();
                }
            }
            else
            {
                // Nếu không có vật cản, đặt item xuống
                transform.position = (Vector3)(Vector2)snappedGridPos;
                isSnapped = true;

                // Kiểm tra lại kết nối cho cầu nối
                InstantBridge bridge = GetComponent<InstantBridge>();
                if (bridge != null)
                {
                    bridge.CheckConnections();
                }
            }
        }
    }
}
