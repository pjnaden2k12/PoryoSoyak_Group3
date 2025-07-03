using UnityEngine;
using UnityEngine.InputSystem;

public class DragItem : MonoBehaviour
{
    private Camera cam;
    private Vector3 offset;
    private bool isDragging = false;
    private Vector3 startPosition;
    private Transform currentBlock = null;

    // LayerMask chỉ raycast vào ItemLayer
    private LayerMask itemLayer;

    void Start()
    {
        cam = Camera.main;
        startPosition = transform.position;
        itemLayer = LayerMask.GetMask("Item");

        if (gameObject.layer != LayerMask.NameToLayer("Item"))
        {
            Debug.LogWarning($"{gameObject.name} không ở layer 'ItemLayer'. Vui lòng gán layer đúng để kéo được.");
        }
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
            worldPoint.z = 0f;

            // Raycast chỉ vào ItemLayer để tránh bị block chặn
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, itemLayer);

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
            bool hitNoBlock = false;
            bool snapped = false;

            // 1. Nếu đụng noBlock → không snap, quay lại chỗ cũ
            foreach (Collider2D col in hits)
            {
                if (col.CompareTag("noBlock"))
                {
                    hitNoBlock = true;
                    break;
                }
            }

            if (hitNoBlock)
            {
                Debug.Log("Vùng cấm snap (noBlock) → Quay về vị trí cũ");
                if (currentBlock != null)
                    transform.position = currentBlock.position;
                else
                    transform.position = startPosition;

                return; // dừng lại luôn, không xét tiếp
            }

            // 2. Nếu không đụng noBlock → tìm block để snap
            foreach (Collider2D col in hits)
            {
                if (col.CompareTag("Block"))
                {
                    transform.position = col.transform.position;
                    currentBlock = col.transform;
                    snapped = true;
                    break;
                }
            }

            // 3. Nếu không snap được vào block nào → về vị trí trước đó
            if (!snapped)
            {
                if (currentBlock != null)
                    transform.position = currentBlock.position;
                else
                    transform.position = startPosition;
            }
        }

    }
}
