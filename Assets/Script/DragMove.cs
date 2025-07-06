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

        // --- LOGIC MỚI KHI THẢ CHUỘT RA ---
        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;

            Vector2Int snappedGridPos = new Vector2Int(
                Mathf.RoundToInt(transform.position.x),
                Mathf.RoundToInt(transform.position.y)
            );

            bool isOccupied = false;

            // 1. Kiểm tra xem có Block ở đó không (dùng data map)
            if (MapSpawner.blockMap.ContainsKey(snappedGridPos))
            {
                isOccupied = true;
            }
            else
            {
                // 2. Nếu không có Block, kiểm tra xem có Item khác ở đó không (dùng vật lý)
                Collider2D[] hits = Physics2D.OverlapCircleAll(snappedGridPos, 0.2f, itemLayerMask);
                foreach (Collider2D hit in hits)
                {
                    // Nếu tìm thấy một item khác không phải là chính item đang được kéo
                    if (hit.gameObject != this.gameObject)
                    {
                        isOccupied = true;
                        break; // Thoát vòng lặp ngay khi tìm thấy
                    }
                }
            }

            // Dựa vào kết quả kiểm tra để quyết định đặt xuống hay trả về
            if (isOccupied)
            {
                // Vị trí đã có vật cản -> Trả về vị trí cũ.
                transform.position = startPosition;
                transform.SetParent(startParent);
                isSnapped = false;

                // Nếu là cầu nối, cập nhật lại trạng thái của nó là không active
                InstantBridge bridge = GetComponent<InstantBridge>();
                if (bridge != null)
                {
                    bridge.CheckConnections();
                }
            }
            else
            {
                // Vị trí trống -> Đặt item xuống.
                transform.position = (Vector3)(Vector2)snappedGridPos;
                isSnapped = true;

                // Kích hoạt kiểm tra kết nối cho cầu nối
                InstantBridge bridge = GetComponent<InstantBridge>();
                if (bridge != null)
                {
                    bridge.CheckConnections();
                }
            }
        }
    }
}