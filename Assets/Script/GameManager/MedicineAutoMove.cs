using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class MedicineAutoMove : MonoBehaviour
{
    public float moveDelay = 0.2f;
    public float moveDuration = 0.3f;
    private bool isMoving = false;

    private LayerMask itemLayerMask;
    public static bool isPlayPressed = false;

    public int currentTypeIndex = 0;
    public Sprite[] medicineSprites;
    public SpriteRenderer spriteRenderer;

    void Start()
    {
        itemLayerMask = LayerMask.GetMask("ItemLayer");
        UpdateAppearance();
    }

    void Update()
    {
        if (!isPlayPressed || isMoving) return;
        TryMoveToNextBlock();
    }

    void TryMoveToNextBlock()
    {
        isMoving = true;
        Vector2Int currentGridPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));

        if (MapSpawner.blockMap.TryGetValue(currentGridPos, out BlockID currentBlockID))
        {
            Vector2 moveDirection = currentBlockID.exitDirection;
            Vector2 targetPosition = (Vector2)transform.position + moveDirection;

            Collider2D[] hits = Physics2D.OverlapCircleAll(targetPosition, 0.2f, itemLayerMask);

            // ƯU TIÊN 1: KIỂM TRA TRẠM TRUNG CHUYỂN (FULLMOVE)
            foreach (var hit in hits)
            {
                if (hit.GetComponent<CrossroadsJunction>() != null)
                {
                    Vector3 finalDestination = transform.position + ((Vector3)moveDirection * 2);
                    MoveToTarget(finalDestination, moveDuration * 1.5f);
                    return;
                }
            }

            // ƯU TIÊN 2: KIỂM TRA CẦU NỐI MỘT CHIỀU (INSTANTBRIDGE)
            foreach (InstantBridge bridge in InstantBridge.ActiveBridges)
            {
                if (bridge.entryBlock == currentBlockID.transform)
                {
                    MoveToTarget(bridge.exitBlock.position);
                    return;
                }
            }

            // ƯU TIÊN 3: DI CHUYỂN BÌNH THƯỜNG
            MoveToTarget(transform.position + (Vector3)moveDirection);
        }
        else
        {
            isMoving = false;
        }
    }

    void MoveToTarget(Vector3 destination, float duration)
    {
        transform.DOMove(destination, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                CheckForItem();
                DOVirtual.DelayedCall(moveDelay, () => isMoving = false);
            });
    }

    void MoveToTarget(Vector3 destination)
    {
        MoveToTarget(destination, moveDuration);
    }

    void CheckForItem()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.1f, itemLayerMask);
        foreach (var hit in hits)
        {
            ItemType itemType = hit.GetComponent<ItemType>();
            if (itemType != null && hit.GetComponent<InstantBridge>() == null && hit.GetComponent<CrossroadsJunction>() == null)
            {
                if (itemType.typeIndex != currentTypeIndex)
                {
                    currentTypeIndex = itemType.typeIndex;
                    UpdateAppearance();
                }
            }
        }
    }

    void UpdateAppearance()
    {
        if (spriteRenderer == null || currentTypeIndex < 0 || currentTypeIndex >= medicineSprites.Length) return;
        transform.DOScale(0.8f, 0.15f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            spriteRenderer.sprite = medicineSprites[currentTypeIndex];
            switch (currentTypeIndex)
            {
                case 0: tag = "mdCircle"; break;
                case 1: tag = "mdSquare"; break;
                case 2: tag = "mdTriangle"; break;
            }
            transform.DOScale(1f, 0.15f).SetEase(Ease.InQuad);
        });
    }
}