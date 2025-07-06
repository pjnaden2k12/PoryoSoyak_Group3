
using System.Collections.Generic;
using UnityEngine;

public class InstantBridge : MonoBehaviour
{
    [Tooltip("Hướng của mũi tên, quyết định đâu là đầu và đuôi.")]
    public Vector2 arrowDirection = Vector2.down;

    [Header("Trạng thái")]
    public bool isActive = false;
    public Transform entryBlock { get; private set; } // Block ở đuôi
    public Transform exitBlock { get; private set; }  // Block ở đầu

    // Danh sách tĩnh chứa tất cả các cầu nối đang được kích hoạt
    public static List<InstantBridge> ActiveBridges = new List<InstantBridge>();

    private SpriteRenderer spriteRenderer;
    private Color activeColor = Color.white;
    private Color inactiveColor = new Color(0.6f, 0.6f, 0.6f, 0.7f);
    private LayerMask blockLayerMask;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        blockLayerMask = LayerMask.GetMask("BlockLayer");
        UpdateVisuals();
    }

    // Hàm này sẽ được gọi từ DragMove.cs sau khi thả item
    public void CheckConnections()
    {
        // Reset trạng thái
        isActive = false;
        entryBlock = null;
        exitBlock = null;

        // Vị trí cần kiểm tra cho đuôi và đầu
        Vector2 tailCheckPos = (Vector2)transform.position - arrowDirection;
        Vector2 headCheckPos = (Vector2)transform.position + arrowDirection;

        // Kiểm tra xem có block ở đuôi không
        Collider2D tailCollider = Physics2D.OverlapCircle(tailCheckPos, 0.2f, blockLayerMask);
        if (tailCollider != null)
        {
            entryBlock = tailCollider.transform;
        }

        // Kiểm tra xem có block ở đầu không
        Collider2D headCollider = Physics2D.OverlapCircle(headCheckPos, 0.2f, blockLayerMask);
        if (headCollider != null)
        {
            exitBlock = headCollider.transform;
        }

        // Nếu cả đầu và đuôi đều được kết nối với các block khác nhau -> Kích hoạt
        if (entryBlock != null && exitBlock != null && entryBlock != exitBlock)
        {
            isActive = true;
        }

        // Cập nhật trạng thái trong danh sách tĩnh
        UpdateActiveList();
        UpdateVisuals();
    }

    private void UpdateActiveList()
    {
        // Nếu đã có trong danh sách nhưng không còn active -> Xóa đi
        if (ActiveBridges.Contains(this) && !isActive)
        {
            ActiveBridges.Remove(this);
        }
        // Nếu active nhưng chưa có trong danh sách -> Thêm vào
        else if (isActive && !ActiveBridges.Contains(this))
        {
            ActiveBridges.Add(this);
        }
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isActive ? activeColor : inactiveColor;
        }
    }

    // Đảm bảo xóa khỏi danh sách khi bị hủy
    private void OnDestroy()
    {
        if (ActiveBridges.Contains(this))
        {
            ActiveBridges.Remove(this);
        }
    }
}