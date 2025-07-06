using UnityEngine;
using UnityEngine.InputSystem;

public class DragItem : MonoBehaviour
{
    private Camera cam;
    private Vector3 offset;
    private bool isDragging = false;
    private Vector3 startPosition;
    private Transform currentBlock = null;
    private LayerMask itemLayer;
    public bool isSnapped = false;

    public void SetStartPosition(Vector3 pos)
    {
        startPosition = pos;
        transform.position = pos;
    }

    void Start()
    {
        cam = Camera.main;
        itemLayer = LayerMask.GetMask("ItemLayer");

        if (gameObject.layer != LayerMask.NameToLayer("ItemLayer"))
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
            bool snapped = false;
            bool droppedOnTable = false;

            foreach (Collider2D col in hits)
            {
                if (col.CompareTag("Table"))
                {
                    droppedOnTable = true;
                }
            }

            foreach (Collider2D col in hits)
            {
                if (col.CompareTag("Block"))
                {
                    bool hasNoBlock = false;

                    foreach (Collider2D c in hits)
                    {
                        if (c.CompareTag("noBlock") && c.transform.IsChildOf(col.transform))
                        {
                            hasNoBlock = true;
                            break;
                        }
                    }

                    if (hasNoBlock)
                    {
                        continue;
                    }

                    bool blockOccupied = false;
                    Collider2D[] itemsAtBlock = Physics2D.OverlapCircleAll(col.transform.position, 0.01f, itemLayer);

                    foreach (Collider2D item in itemsAtBlock)
                    {
                        if (item.gameObject != gameObject)
                        {
                            blockOccupied = true;
                            break;
                        }
                    }

                    if (!blockOccupied)
                    {
                        transform.position = col.transform.position;
                        currentBlock = col.transform;
                        isSnapped = true;
                        snapped = true;
                        break;
                    }
                }
            }

            if (!snapped)
            {
                if (droppedOnTable)
                {
                    transform.position = startPosition;
                    currentBlock = null;
                    isSnapped = false;
                }
                else if (currentBlock != null)
                {
                    transform.position = currentBlock.position;
                    isSnapped = true;
                }
                else
                {
                    transform.position = startPosition;
                    isSnapped = false;
                }
            }
        }
    }
}
