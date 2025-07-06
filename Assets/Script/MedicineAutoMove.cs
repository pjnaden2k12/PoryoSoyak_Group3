using UnityEngine;
using DG.Tweening;

public class MedicineAutoMove : MonoBehaviour
{
    public float moveDelay = 0.2f;
    public float moveDuration = 0.3f;
    private bool isMoving = false;

    private int maxId = 0;
    private int blockLayerMask;
    public static bool isPlayPressed = false;

    public int currentTypeIndex = 0;
    public Sprite[] medicineSprites;
    public SpriteRenderer spriteRenderer;
    private int itemLayerMask;
    void Start()
    {
        blockLayerMask = LayerMask.GetMask("BlockLayer");
        itemLayerMask = LayerMask.GetMask("ItemLayer");

        BlockID[] allBlocks = Object.FindObjectsByType<BlockID>(FindObjectsSortMode.None);
        maxId = 0;
        foreach (var block in allBlocks)
        {
            if (block.id > maxId)
                maxId = block.id;
        }
        maxId += 1;

        UpdateAppearance();
    }

    void Update()
    {
        if (!isPlayPressed) return;

        if (!isMoving)
            TryMoveToNextBlock();
    }

    void TryMoveToNextBlock()
    {
        int currentId = -1;

        Collider2D currentBlock = Physics2D.OverlapCircle(transform.position, 0.1f, blockLayerMask);
        if (currentBlock != null && currentBlock.CompareTag("Block"))
        {
            BlockID blockIDComponent = currentBlock.GetComponent<BlockID>();
            if (blockIDComponent != null)
                currentId = blockIDComponent.id;
        }

        if (currentId == -1)
        {
            Debug.LogWarning("Medicine not on a valid block.");
            return;
        }

        int nextId = (currentId + 1) % maxId;

        BlockID[] allBlocks = Object.FindObjectsByType<BlockID>(FindObjectsSortMode.None);

        BlockID closestBlock = null;
        float closestDistance = float.MaxValue;

        foreach (var block in allBlocks)
        {
            if (block.id == nextId)
            {
                float distance = Vector3.Distance(transform.position, block.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestBlock = block;
                }
            }
        }

        if (closestBlock != null)
        {
            MoveToBlock(closestBlock.transform.position);
        }
        else
        {
            Debug.LogWarning($"No block found with ID {nextId}");
        }
    }

    void MoveToBlock(Vector3 destination)
    {
        isMoving = true;

        transform.DOMove(destination, moveDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                CheckForItem();

                DOVirtual.DelayedCall(moveDelay, () =>
                {
                    isMoving = false;
                });
            });
    }

    void CheckForItem()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.1f);
        

        foreach (var hit in hits)
        {
           

            if (hit.CompareTag("Item"))
            {
                ItemType itemType = hit.GetComponent<ItemType>();
                if (itemType != null)
                {
                    if (itemType.typeIndex != currentTypeIndex)
                    {
                        currentTypeIndex = itemType.typeIndex;
                        Debug.Log($"Changed type to {currentTypeIndex}");
                        UpdateAppearance();
                    }
                }
                else
                {
                    Debug.LogWarning("No ItemType found on item.");
                }
            }
        }
    }

    void UpdateAppearance()
    {
        if (spriteRenderer == null || currentTypeIndex < 0 || currentTypeIndex >= medicineSprites.Length)
            return;

        // Scale nhỏ lại
        transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack).OnComplete(() =>
        {
            // Đổi sprite và tag
            spriteRenderer.sprite = medicineSprites[currentTypeIndex];

            switch (currentTypeIndex)
            {
                case 0: tag = "mdCircle"; break;
                case 1: tag = "mdSquare"; break;
                case 2: tag = "mdTriangle"; break;
            }

           

            // Scale to full size
            transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack);
        });
    }


}
